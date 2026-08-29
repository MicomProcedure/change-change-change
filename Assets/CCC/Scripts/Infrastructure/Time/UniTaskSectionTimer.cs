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
        private TimeSpan _currentDuration;
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
        /// <param name="duration">制限時間。</param>
        /// <exception cref="ArgumentOutOfRangeException">制限時間が正でない場合。</exception>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Start(MissionId missionId, TimeSpan duration)
        {
            ThrowIfDisposed();
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            }

            CancelCurrentTimer();
            int timerGeneration = checked(++_timerGeneration);
            _currentMissionId = missionId;
            _currentDuration = duration;
            _remainingTime = duration;
            _startedAtRealtimeSeconds = Time.realtimeSinceStartupAsDouble;
            _hasCurrentMission = true;
            _isRunning = true;
            _isExpired = false;

            _snapshotsSubject.OnNext(new SectionTimerSnapshot(
                missionId,
                SectionTimerStatus.Running,
                duration));

            _timerCancellationSource = new CancellationTokenSource();
            RunTimerAsync(
                missionId,
                duration,
                timerGeneration,
                _timerCancellationSource.Token).Forget();
        }

        /// <summary>
        /// MissionIdが一致する実行中Timerを停止します。
        /// </summary>
        /// <param name="missionId">停止対象MissionId。</param>
        /// <exception cref="ObjectDisposedException">破棄済みの場合。</exception>
        public void Stop(MissionId missionId)
        {
            ThrowIfDisposed();
            if (!_hasCurrentMission || !_isRunning || _isExpired || _currentMissionId != missionId)
            {
                return;
            }

            UpdateRemainingTime();
            checked
            {
                _timerGeneration++;
            }
            CancelCurrentTimer();
            _isRunning = false;
            _snapshotsSubject.OnNext(new SectionTimerSnapshot(
                missionId,
                SectionTimerStatus.Stopped,
                _remainingTime));
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
                        Math.Max(0d, Time.realtimeSinceStartupAsDouble - _startedAtRealtimeSeconds));
                    _remainingTime = duration - elapsedTime;
                    if (_remainingTime <= TimeSpan.Zero)
                    {
                        _remainingTime = TimeSpan.Zero;
                        _isRunning = false;
                        _isExpired = true;
                        CancelCurrentTimer();
                        _snapshotsSubject.OnNext(new SectionTimerSnapshot(
                            missionId,
                            SectionTimerStatus.Expired,
                            TimeSpan.Zero));
                        _expiredSignalsSubject.OnNext(new SectionTimerExpiredSignal(missionId));
                        return;
                    }

                    _snapshotsSubject.OnNext(new SectionTimerSnapshot(
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
                Math.Max(0d, Time.realtimeSinceStartupAsDouble - _startedAtRealtimeSeconds));
            _remainingTime = _currentDuration - elapsedTime;
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
    }
}
