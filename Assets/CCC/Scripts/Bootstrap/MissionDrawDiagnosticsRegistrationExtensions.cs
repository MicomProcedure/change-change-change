using System.Collections.Generic;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using VContainer;

namespace Ccc.Bootstrap.Log
{
    /// <summary>
    /// Mission抽選診断Reporterの組み立てをDI登録として提供します。
    /// </summary>
    public static class MissionDrawDiagnosticsRegistrationExtensions
    {
        /// <summary>
        /// Log出力とCSV出力へ配信するMission抽選診断ReporterをScopedで登録します。CSV出力はEditor実行時だけ構成します。
        /// </summary>
        /// <param name="builder">DI登録を行うBuilder。</param>
        /// <remarks>
        /// 親Scopeに<see cref="ILoggerFactory"/>と<see cref="LogSettingsAsset"/>、
        /// 同じScopeに<see cref="DiagnosticsSessionDescriptor"/>が登録されている必要があります。
        /// </remarks>
        public static void RegisterMissionDrawDiagnostics(this IContainerBuilder builder)
        {
            builder.RegisterLogger<MissionDrawTrace>(Lifetime.Scoped);
#if UNITY_EDITOR
            builder.Register<CsvDiagnosticsReporter<MissionDrawTrace>>(
                resolver =>
                {
                    LogSettingsAsset logSettings = resolver.Resolve<LogSettingsAsset>();
                    MissionDrawDiagnosticsSetting setting = logSettings.MissionDrawDiagnostics;
                    return new CsvDiagnosticsReporter<MissionDrawTrace>(
                        new MissionDrawTraceCsvFormatter(setting.CsvCandidateCount),
                        resolver.Resolve<DiagnosticsSessionDescriptor>(),
                        logSettings.ResolveCategoryDirectoryPath(setting.CsvDirectoryName));
                },
                Lifetime.Scoped);
#endif
            builder.Register<IDiagnosticsReporter<MissionDrawTrace>>(
                resolver =>
                {
                    MissionDrawDiagnosticsSetting setting =
                        resolver.Resolve<LogSettingsAsset>().MissionDrawDiagnostics;
                    ILogger failureLogger = resolver
                        .Resolve<ILoggerFactory>()
                        .CreateLogger<SafeDiagnosticsReporter<MissionDrawTrace>>();
                    var reporters = new List<IDiagnosticsReporter<MissionDrawTrace>>
                    {
                        new SafeDiagnosticsReporter<MissionDrawTrace>(
                            new LoggerDiagnosticsReporter<MissionDrawTrace>(
                                resolver.Resolve<ILogger<MissionDrawTrace>>(),
                                new MissionDrawTraceTextFormatter(setting.LogCandidateCount),
                                setting.LogLevel),
                            failureLogger)
                    };
#if UNITY_EDITOR
                    if (setting.IsCsvEnabled)
                    {
                        reporters.Add(new SafeDiagnosticsReporter<MissionDrawTrace>(
                            resolver.Resolve<CsvDiagnosticsReporter<MissionDrawTrace>>(),
                            failureLogger));
                    }
#endif
                    return new CompositeDiagnosticsReporter<MissionDrawTrace>(reporters);
                },
                Lifetime.Scoped);
        }
    }
}
