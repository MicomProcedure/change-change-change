using System;
using System.Globalization;
using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// ミッション条件とテンプレートから日本語の表示文言を生成します。
    /// </summary>
    public sealed class MissionTextFormatter
    {
        private readonly IMissionTextSource _textSource;

        /// <summary>
        /// 指定されたミッション文言の提供元からFormatterを生成します。
        /// </summary>
        /// <param name="textSource">表示テンプレートの提供元。</param>
        /// <exception cref="ArgumentNullException"><paramref name="textSource"/>がnullの場合。</exception>
        public MissionTextFormatter(IMissionTextSource textSource)
        {
            _textSource = textSource ?? throw new ArgumentNullException(nameof(textSource));
        }

        /// <summary>
        /// ミッション条件のパラメータをテンプレートへ適用します。
        /// </summary>
        /// <param name="condition">表示対象のミッション条件。</param>
        /// <returns>日本語のミッション表示文言。</returns>
        /// <exception cref="InvalidOperationException">テンプレート提供元がnullのテンプレートを返した場合。</exception>
        /// <exception cref="ArgumentOutOfRangeException">未対応のミッション種類の場合。</exception>
        public string Format(MissionCondition condition)
        {
            string template = _textSource.GetTemplate(condition.Kind, condition.IsNegated);
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new InvalidOperationException(
                    $"Mission text source returned an empty template for MissionKind.{condition.Kind}.");
            }

            if (condition.Kind == MissionKind.Between)
            {
                int upperBound = checked(condition.ParameterA + condition.ParameterB);
                return FormatTemplate(
                    CultureInfo.InvariantCulture,
                    template,
                    condition.ParameterA,
                    upperBound);
            }

            if (condition.Kind == MissionKind.Modulo)
            {
                return FormatTemplate(
                    CultureInfo.InvariantCulture,
                    template,
                    condition.ParameterA,
                    condition.ParameterB);
            }

            if (MissionCondition.IsNoParameterKind(condition.Kind))
            {
                return FormatTemplate(CultureInfo.InvariantCulture, template);
            }

            if (MissionCondition.IsSingleParameterKind(condition.Kind))
            {
                return FormatTemplate(
                    CultureInfo.InvariantCulture,
                    template,
                    condition.ParameterA);
            }

            throw new ArgumentOutOfRangeException(nameof(condition), condition.Kind, null);
        }

        private static string FormatTemplate(
            IFormatProvider formatProvider,
            string template,
            params object[] arguments)
        {
            try
            {
                return string.Format(formatProvider, template, arguments);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    "Mission text template contains an invalid placeholder.",
                    exception);
            }
        }
    }
}
