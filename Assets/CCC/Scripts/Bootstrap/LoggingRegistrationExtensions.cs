using Microsoft.Extensions.Logging;
using VContainer;

namespace Ccc.Bootstrap.Log
{
    /// <summary>
    /// LoggerFactoryから型別Loggerを生成するDI登録を提供します。
    /// </summary>
    public static class LoggingRegistrationExtensions
    {
        /// <summary>
        /// 指定型のLoggerを指定された生存期間で登録します。出力するかどうかはLoggerFactoryのLogLevel設定で決まります。
        /// </summary>
        /// <typeparam name="T">ログのカテゴリとして使用する型。</typeparam>
        /// <param name="builder">DI登録を行うBuilder。</param>
        /// <param name="lifetime">Loggerの生存期間。</param>
        public static void RegisterLogger<T>(this IContainerBuilder builder, Lifetime lifetime)
        {
            builder.Register<ILogger<T>>(
                resolver => resolver.Resolve<ILoggerFactory>().CreateLogger<T>(),
                lifetime);
        }
    }
}
