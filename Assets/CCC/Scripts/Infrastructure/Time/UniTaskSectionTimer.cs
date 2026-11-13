using System;
using System.Threading;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Ccc.Infrastructure.Time
{
    /// <summary>
    /// UnityのUpdate PlayerLoopでSectionTimerを実行する実装です。
    /// </summary>
    public sealed class UniTaskSectionTimer : ISectionTimer
    {
        private readonly Subject<SectionTimerSnapshot> _snapshotsSubject;
        private readonly Subject<SectionTimerExpiredSignal> _expiredSignalsSubject;

        private CancellationTokenSource _timerCancellationSource;
        private MissionId _currentMissionId;
        private TimeSpan _normalDuration;
        private TimeSpan _bonusDuration;
        private TimeSpan _remainingTime;
        private double _startedAtRealtimeSeconds;
        private int _timerGeneration;
        private bool _hasCurrentMission;
        private bool _isRunning;
        private bool _isExpired;
        private bool _isDisposed;

        /// <summary>
        /// Timer Snapshotストリームを取得します。
        /// </summary>
        public Observable<SectionTimerSnapshot> Snapshots { get; }

        /// <summary>
        /// Timer満了Signalストリームを取得します。
        /// </summary>
        public Observable<SectionTimerExpiredSignal> ExpiredSignals { get; }

        /// <summary>
        /// SectionTimerを生成します。
        /// </summary>
        public UniTaskSectionTimer()
        {
            _snapshotsSubject = new Subject<SectionTimerSnapshot>();
            _expiredSignalsSubject = new Subject<SectionTimerExpiredSignal>();
            Snapshots = _snapshotsSubject;
            ExpiredSignals = _expiredSignalsSubject;
        }

        /// <summary>
        /// 指定MissionのTimerを開始します。
        /// </summary>
        /// <param name="missionId">対象MissionId。</param>
        /// <param name="normalDuration">Sectionの通常制限時間。</param>
        /// <param name="bonusDuration">付与するボーナス時間。</param>
        /// <exception cref="ArgumentOutOfRangeException">通常制限時間が正でない、またはボーナス時間が負の場合。</exception>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Start(MissionId missionId, TimeSpan normalDuration, TimeSpan bonusDuration)
        {
            ThrowIfDisposed();
            if (normalDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(normalDuration), normalDuration, "Normal duration must be positive.");
            }

            if (bonusDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(bonusDuration), bonusDuration, "Bonus duration must be zero or positive.");
            }

            CancelCurrentTimer();
            int timerGeneration = checked(++_timerGeneration);
            TimeSpan duration = normalDuration + bonusDuration;
            _currentMissionId = missionId;
            _normalDuration = normalDuration;
            _bonusDuration = bonusDuration;
            _remainingTime = duration;
            _startedAtRealtimeSeconds = UnityEngine.Time.realtimeSinceStartupAsDouble;
            _hasCurrentMission = true;
            _isRunning = true;
            _isExpired = false;

            _snapshotsSubject.OnNext(CreateSnapshot(missionId, SectionTimerStatus.Running, duration));

            _timerCancellationSource = new CancellationTokenSource();
            RunTimerAsync(
                missionId,
                duration,
                timerGeneration,
                _timerCancellationSource.Token).Forget();
        }

        /// <summary>
        /// MissionIdが一致する実行中Timerを停止し、停止時点のSnapshotを返します。
        /// </summary>
        /// <param name="missionId">停止対象MissionId。</param>
        /// <param name="stoppedSnapshot">停止時点のSnapshot。停止しなかった場合はdefault。</param>
        /// <returns>停止した場合はtrue。満了済み・不一致などで停止しなかった場合はfalse。</returns>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public bool TryStop(MissionId missionId, out SectionTimerSnapshot stoppedSnapshot)
        {
            ThrowIfDisposed();
            if (!_hasCurrentMission || !_isRunning || _isExpired || _currentMissionId != missionId)
            {
                stoppedSnapshot = default;
                return false;
            }

            UpdateRemainingTime();
            checked
            {
                _timerGeneration++;
            }
            CancelCurrentTimer();
            _isRunning = false;
            stoppedSnapshot = CreateSnapshot(missionId, SectionTimerStatus.Stopped, _remainingTime);
            _snapshotsSubject.OnNext(stoppedSnapshot);
            return true;
        }

        /// <summary>
        /// 実行中Timerを停止し、満了処理を無効化します。
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void StopAll()
        {
            ThrowIfDisposed();
            checked
            {
                _timerGeneration++;
            }
            CancelCurrentTimer();
            _hasCurrentMission = false;
            _isRunning = false;
            _isExpired = false;
            _remainingTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Timer処理と公開ストリームを破棄します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            checked
            {
                _timerGeneration++;
            }
            CancelCurrentTimer();
            _hasCurrentMission = false;
            _isRunning = false;
            _snapshotsSubject.OnCompleted();
            _expiredSignalsSubject.OnCompleted();
            _snapshotsSubject.Dispose();
            _expiredSignalsSubject.Dispose();
        }

        private async UniTask RunTimerAsync(
            MissionId missionId,
            TimeSpan duration,
            int timerGeneration,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        cancellationToken);
                    if (cancellationToken.IsCancellationRequested ||
                        _isDisposed ||
                        !_hasCurrentMission ||
                        !_isRunning ||
                        _timerGeneration != timerGeneration ||
                        _currentMissionId != missionId)
                    {
                        return;
                    }

                    TimeSpan elapsedTime = TimeSpan.FromSeconds(
                        Math.Max(0d, UnityEngine.Time.realtimeSinceStartupAsDouble - _startedAtRealtimeSeconds));
                    _remainingTime = duration - elapsedTime;
                    if (_remainingTime <= TimeSpan.Zero)
                    {
                        _remainingTime = TimeSpan.Zero;
                        _isRunning = false;
                        _isExpired = true;
                        CancelCurrentTimer();
                        _snapshotsSubject.OnNext(CreateSnapshot(
                            missionId,
                            SectionTimerStatus.Expired,
                            TimeSpan.Zero));
                        _expiredSignalsSubject.OnNext(new SectionTimerExpiredSignal(missionId));
                        return;
                    }

                    _snapshotsSubject.OnNext(CreateSnapshot(
                        missionId,
                        SectionTimerStatus.Running,
                        _remainingTime));
                }
            }
            catch (OperationCanceledException)
            {
                // Start、Stop、StopAll、Disposeによる正常なキャンセルです。
            }
        }

        private void UpdateRemainingTime()
        {
            if (!_isRunning)
            {
                return;
            }

            TimeSpan elapsedTime = TimeSpan.FromSeconds(
                Math.Max(0d, UnityEngine.Time.realtimeSinceStartupAsDouble - _startedAtRealtimeSeconds));
            _remainingTime = _normalDuration + _bonusDuration - elapsedTime;
            if (_remainingTime < TimeSpan.Zero)
            {
                _remainingTime = TimeSpan.Zero;
            }
        }

        private void CancelCurrentTimer()
        {
            if (_timerCancellationSource == null)
            {
                return;
            }

            _timerCancellationSource.Cancel();
            _timerCancellationSource.Dispose();
            _timerCancellationSource = null;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(UniTaskSectionTimer));
            }
        }

        private SectionTimerSnapshot CreateSnapshot(MissionId missionId, SectionTimerStatus status, TimeSpan remainingTime)
        {
            return new SectionTimerSnapshot(missionId, status, remainingTime, _normalDuration, _bonusDuration);
        }
    }
}
