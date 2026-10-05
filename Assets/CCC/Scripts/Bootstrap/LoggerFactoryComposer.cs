using System;
using System.IO;
using Ccc.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Unity;

namespace Ccc.Bootstrap.Log
{
    /// <summary>
    /// Log出力設定からLogLevelフィルターと出力Providerを構成したLoggerFactoryを生成します。
    /// </summary>
    public static class LoggerFactoryComposer
    {
        private const string GeneralLogFileExtension = ".log";

        /// <summary>
        /// Log出力設定とファイル配置設定からLoggerFactoryを生成します。出力ProviderはEditor実行時だけ追加します。
        /// </summary>
        /// <param name="outputOptions">出力先と出力レベルの設定。</param>
        /// <param name="directoryOptions">Logファイルの配置設定。</param>
        /// <param name="startedAt">通常Logファイル名に使うアプリケーション起動日時。</param>
        /// <returns>構成済みのLoggerFactory。</returns>
        /// <exception cref="ArgumentNullException">設定がnullの場合。</exception>
        public static ILoggerFactory Create(
            LogOutputOptions outputOptions,
            LogDirectoryOptions directoryOptions,
            DateTime startedAt)
        {
            if (outputOptions == null)
            {
                throw new ArgumentNullException(nameof(outputOptions));
            }

            if (directoryOptions == null)
            {
                throw new ArgumentNullException(nameof(directoryOptions));
            }

            return LoggerFactory.Create(logging =>
            {
                logging.SetMinimumLevel(outputOptions.MinimumLevel);
                for (int index = 0; index < outputOptions.CategoryLevelOverrides.Count; index++)
                {
                    LogCategoryLevelOverride levelOverride = outputOptions.CategoryLevelOverrides[index];
                    logging.AddFilter(levelOverride.CategoryPrefix, levelOverride.MinimumLevel);
                }

#if UNITY_EDITOR
                if (outputOptions.IsUnityConsoleEnabled)
                {
                    logging.AddZLoggerUnityDebug(options => options.PrettyStacktrace = false);
                }

                if (outputOptions.IsGeneralFileEnabled)
                {
                    logging.AddZLoggerFile(
                        CreateGeneralLogFilePath(outputOptions, directoryOptions, startedAt),
                        options => options.UsePlainTextFormatter(formatter =>
                            formatter.SetPrefixFormatter(
                                $"{0:local-longdate} [{1}] {2}: ",
                                (in MessageTemplate template, in LogInfo info) =>
                                    template.Format(info.Timestamp, info.LogLevel, info.Category))));
                }
#endif
            });
        }

        private static string CreateGeneralLogFilePath(
            LogOutputOptions outputOptions,
            LogDirectoryOptions directoryOptions,
            DateTime startedAt)
        {
            string directoryPath =
                directoryOptions.ResolveCategoryDirectoryPath(outputOptions.GeneralDirectoryName);
            Directory.CreateDirectory(directoryPath);
            return Path.Combine(
                directoryPath,
                directoryOptions.FormatTimestamp(startedAt) + GeneralLogFileExtension);
        }
    }
}
