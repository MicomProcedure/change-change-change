using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Ccc.Core.Logic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Microsoft.Extensions.Logging;
using R3;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// GameplayのEventを購読して提出判定を記録し、Session終了時にまとめて送信します。
    /// </summary>
    public sealed class GameplayTelemetryRecorder : IDisposable
    {
        private const string Iso8601Format = "o";

        private readonly IGameplayFlow _flow;
        private readonly TelemetrySessionDescriptor _sessionDescriptor;
        private readonly TelemetrySender _sender;
        private readonly ILogger<GameplayTelemetryRecorder> _logger;
        private readonly List<SubmissionRecordDto> _submissionRecords = new List<SubmissionRecordDto>();
        private readonly Stopwatch _answerStopwatch = new Stopwatch();
        private IDisposable _eventSubscription;
        private Mission _currentMission;
        private TimeSpan _currentMissionDuration;
        private int _correctAnswerCount;
        private string _startedAtUtc;
        private bool _hasSent;

        /// <summary>
        /// GameplayTelemetryRecorderを生成します。
        /// </summary>
        /// <param name="flow">購読するGameplayフロー。</param>
        /// <param name="sessionDescriptor">Sessionの識別情報。</param>
        /// <param name="sender">プレイデータの送信先。</param>
        /// <param name="logger">記録失敗を出力するLogger。</param>
        /// <exception cref="ArgumentNullException">依存のいずれかがnullの場合。</exception>
        public GameplayTelemetryRecorder(
            IGameplayFlow flow,
            TelemetrySessionDescriptor sessionDescriptor,
            TelemetrySender sender,
            ILogger<GameplayTelemetryRecorder> logger)
        {
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            _sessionDescriptor = sessionDescriptor ?? throw new ArgumentNullException(nameof(sessionDescriptor));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GameplayのEvent購読を開始します。Session開始より前に呼び出してください。
        /// </summary>
        /// <exception cref="InvalidOperationException">すでに購読を開始している場合。</exception>
        public void StartRecording()
        {
            if (_eventSubscription != null)
            {
                throw new InvalidOperationException("GameplayTelemetryRecorder has already started recording.");
            }

            _startedAtUtc = DateTime.UtcNow.ToString(Iso8601Format, CultureInfo.InvariantCulture);
            _eventSubscription = _flow.Events.Subscribe(HandleEvent);
        }

        /// <summary>
        /// Event購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _eventSubscription?.Dispose();
            _eventSubscription = null;
        }

        private void HandleEvent(GameplayEvent gameplayEvent)
        {
            // 記録の失敗でGameplayを止めない
            try
            {
                switch (gameplayEvent)
                {
                    case MissionPresentedEvent presentedEvent:
                        RecordMissionPresented(presentedEvent);
                        break;
                    case SubmissionResolvedEvent resolvedEvent:
                        RecordSubmission(resolvedEvent);
                        break;
                    case SessionEndedEvent endedEvent:
                        SendSession(endedEvent.Result);
                        break;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Telemetry recording failed: {Message}", exception.Message);
            }
        }

        private void RecordMissionPresented(MissionPresentedEvent presentedEvent)
        {
            _currentMission = presentedEvent.Mission;
            _currentMissionDuration = presentedEvent.Duration;
            _answerStopwatch.Restart();
        }

        private void RecordSubmission(SubmissionResolvedEvent resolvedEvent)
        {
            if (_currentMission == null)
            {
                return;
            }

            SubmissionResolution resolution = resolvedEvent.Resolution;
            Expression expression = resolution.SubmittedExpression;
            CalculationResult calculationResult = resolution.CalculationResult;
            MissionCondition condition = _currentMission.Condition;

            // 正解したときだけSectionが進むため、Sectionは正解数 + 1で数える
            _submissionRecords.Add(new SubmissionRecordDto(
                _correctAnswerCount + 1,
                _currentMission.Tier,
                condition.Kind.ToString(),
                condition.ParameterA,
                condition.ParameterB,
                condition.IsNegated,
                (long)_currentMissionDuration.TotalMilliseconds,
                resolution.Trigger.ToString(),
                expression.Left,
                expression.Operator.ToString(),
                expression.Right,
                calculationResult.Kind.ToString(),
                calculationResult.IsDefined ? calculationResult.Value.ToString() : string.Empty,
                resolution.IsSuccess,
                resolvedEvent.RemainingLife,
                _answerStopwatch.ElapsedMilliseconds));

            if (resolution.IsSuccess)
            {
                _correctAnswerCount++;
            }
        }

        private void SendSession(SessionResult result)
        {
            if (_hasSent)
            {
                return;
            }

            _hasSent = true;
            var sessionRecord = new SessionRecordDto(
                _sessionDescriptor,
                result.FinalSection,
                result.CorrectAnswerCount,
                result.TerminationReason.ToString(),
                _startedAtUtc,
                DateTime.UtcNow.ToString(Iso8601Format, CultureInfo.InvariantCulture));
            _sender.SendWithoutWaiting(new TelemetryPayloadDto(sessionRecord, _submissionRecords));
        }
    }
}
