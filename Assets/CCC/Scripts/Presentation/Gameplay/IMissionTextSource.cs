using Ccc.Core.Model;

namespace Ccc.Presentation
{
    /// <summary>
    /// ミッション表示テンプレートをPresentation境界へ提供する契約です。
    /// </summary>
    public interface IMissionTextSource
    {
        /// <summary>
        /// 指定されたミッション種類の表示テンプレートを取得します。
        /// </summary>
        /// <param name="kind">ミッションの種類。</param>
        /// <param name="isNegated">否定条件用のテンプレートを取得するかどうか。</param>
        /// <returns>表示テンプレート。</returns>
        string GetTemplate(MissionKind kind, bool isNegated);
    }
}
