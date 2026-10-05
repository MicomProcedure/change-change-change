using System.Collections.Generic;
using System.IO;

namespace Ccc.Infrastructure.Logging
{
    /// <summary>
    /// Log設定のInspector値で共有する検証規則を提供します。
    /// </summary>
    internal static class LogSettingsValidation
    {
        /// <summary>
        /// ディレクトリ名が空白でなく、ファイル名に使えない文字を含まないことを検証します。
        /// </summary>
        /// <param name="errors">エラーの追加先。</param>
        /// <param name="directoryName">検証するディレクトリ名。</param>
        /// <param name="settingName">エラーメッセージに使う設定名。</param>
        internal static void ValidateDirectoryName(List<string> errors, string directoryName, string settingName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                errors.Add($"{settingName} must not be blank.");
                return;
            }

            if (directoryName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errors.Add($"{settingName} contains invalid characters: {directoryName}.");
            }
        }
    }
}
