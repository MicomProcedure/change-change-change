using System;
using System.IO;
using Ccc.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Unity;

namespace Ccc.Bootstrap.Log
{
    /// <summary>
    /// Log設定からLogLevelフィルターと出力Providerを構成したLoggerFactoryを生成します。
    /// </summary>
    public static class LoggerFactoryComposer
    {
        private const string GeneralLogFileExtension = ".log";

        /// <summary>
        /// Log設定からLoggerFactoryを生成します。出力ProviderはEditor実行時だけ追加します。
        /// </summary>
        /// <param name="settings">検証済みのLog設定。</param>
        /// <param name="startedAt">通常Logファイル名に使うアプリケーション起動日時。</param>
        /// <returns>構成済みのLoggerFactory。</returns>
        /// <exception cref="ArgumentNullException">設定がnullの場合。</exception>
        public static ILoggerFactory Create(LogSettingsAsset settings, DateTime startedAt)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return LoggerFactory.Create(logging =>
            {
                logging.SetMinimumLevel(settings.MinimumLevel);
                for (int index = 0; index < settings.CategoryLevelOverrides.Count; index++)
                {
                    LogCategoryLevelSetting levelOverride = settings.CategoryLevelOverrides[index];
                    logging.AddFilter(levelOverride.CategoryPrefix, levelOverride.MinimumLevel);
                }

#if UNITY_EDITOR
                if (settings.IsUnityConsoleEnabled)
                {
                    logging.AddZLoggerUnityDebug(options => options.PrettyStacktrace = false);
                }

                if (settings.IsGeneralFileEnabled)
                {
                    logging.AddZLoggerFile(
                        CreateGeneralLogFilePath(settings, startedAt),
                        options => options.UsePlainTextFormatter(formatter =>
                            formatter.SetPrefixFormatter(
                                $"{0:local-longdate} [{1}] {2}: ",
                                (in MessageTemplate template, in LogInfo info) =>
                                    template.Format(info.Timestamp, info.LogLevel, info.Category))));
                }
#endif
            });
        }

        private static string CreateGeneralLogFilePath(LogSettingsAsset settings, DateTime startedAt)
        {
            string directoryPath = settings.ResolveCategoryDirectoryPath(settings.GeneralDirectoryName);
            Directory.CreateDirectory(directoryPath);
            return Path.Combine(directoryPath, settings.FormatTimestamp(startedAt) + GeneralLogFileExtension);
        }
    }
}
