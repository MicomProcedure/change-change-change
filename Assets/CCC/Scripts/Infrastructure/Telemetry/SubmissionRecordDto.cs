using System;
using UnityEngine;

namespace Ccc.Infrastructure.Telemetry
{
    /// <summary>
    /// 1回の提出判定を表す送信用Dtoです。フィールド名がそのままJSONのキーになります。
    /// </summary>
    [Serializable]
    public sealed class SubmissionRecordDto
    {
        [SerializeField] private int section;
        [SerializeField] private int tier;
        [SerializeField] private string missionKind;
        [SerializeField] private int parameterA;
        [SerializeField] private int parameterB;
        [SerializeField] private bool isNegated;
        [SerializeField] private long missionDurationMs;
        [SerializeField] private string trigger;
        [SerializeField] private int left;
        [SerializeField] private string operatorType;
        [SerializeField] private int right;
        [SerializeField] private string calculationResultKind;
        [SerializeField] private string calculationResultValue;
        [SerializeField] private bool isSuccess;
        [SerializeField] private int remainingLife;
        [SerializeField] private long answerTimeMs;

        /// <summary>
        /// SubmissionRecordDtoを生成します。
        /// </summary>
        /// <param name="section">何問目か。</param>
        /// <param name="tier">MissionのTier。</param>
        /// <param name="missionKind">Missionの種類名。</param>
        /// <param name="parameterA">Mission条件のパラメータA。</param>
        /// <param name="parameterB">Mission条件のパラメータB。</param>
        /// <param name="isNegated">Mission条件が否定形かどうか。</param>
        /// <param name="missionDurationMs">Missionの制限時間（ミリ秒）。</param>
        /// <param name="trigger">判定トリガー名。</param>
        /// <param name="left">式の左辺。</param>
        /// <param name="operatorType">式の演算子名。</param>
        /// <param name="right">式の右辺。</param>
        /// <param name="calculationResultKind">計算結果の種類名。</param>
        /// <param name="calculationResultValue">計算結果の値。未定義の場合は空文字。</param>
        /// <param name="isSuccess">正解かどうか。</param>
        /// <param name="remainingLife">判定後の残りLife。</param>
        /// <param name="answerTimeMs">Mission提示から判定までの時間（ミリ秒）。</param>
        public SubmissionRecordDto(
            int section,
            int tier,
            string missionKind,
            int parameterA,
            int parameterB,
            bool isNegated,
            long missionDurationMs,
            string trigger,
            int left,
            string operatorType,
            int right,
            string calculationResultKind,
            string calculationResultValue,
            bool isSuccess,
            int remainingLife,
            long answerTimeMs)
        {
            this.section = section;
            this.tier = tier;
            this.missionKind = missionKind;
            this.parameterA = parameterA;
            this.parameterB = parameterB;
            this.isNegated = isNegated;
            this.missionDurationMs = missionDurationMs;
            this.trigger = trigger;
            this.left = left;
            this.operatorType = operatorType;
            this.right = right;
            this.calculationResultKind = calculationResultKind;
            this.calculationResultValue = calculationResultValue;
            this.isSuccess = isSuccess;
            this.remainingLife = remainingLife;
            this.answerTimeMs = answerTimeMs;
        }
    }
}
