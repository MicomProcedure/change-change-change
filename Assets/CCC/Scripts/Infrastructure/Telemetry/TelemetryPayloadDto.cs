using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// 1回の送信でGASへ渡すSession結果と提出判定一覧のDtoです。
    /// </summary>
    [Serializable]
    public sealed class TelemetryPayloadDto
    {
        [SerializeField] private SessionRecordDto session;
        [SerializeField] private List<SubmissionRecordDto> submissions;

        /// <summary>
        /// TelemetryPayloadDtoを生成します。
        /// </summary>
        /// <param name="session">Session結果。</param>
        /// <param name="submissions">提出判定一覧。</param>
        /// <exception cref="ArgumentNullException">引数のいずれかがnullの場合。</exception>
        public TelemetryPayloadDto(SessionRecordDto session, List<SubmissionRecordDto> submissions)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.submissions = submissions ?? throw new ArgumentNullException(nameof(submissions));
        }
    }
}
