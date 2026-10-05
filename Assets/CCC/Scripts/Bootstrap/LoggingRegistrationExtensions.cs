using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VContainer;

namespace Ccc.Bootstrap.Log
{
    /// <summary>
    /// LoggerFactoryから型別Loggerを生成するDI登録を提供します。
    /// </summary>
    public static class LoggingRegistrationExtensions
    {
        /// <summary>
        /// 指定型のLoggerを指定された生存期間で登録します。
        /// </summary>
        /// <typeparam name="T">ログのカテゴリとして使用する型。</typeparam>
        /// <param name="builder">DI登録を行うBuilder。</param>
        /// <param name="lifetime">Loggerの生存期間。</param>
        /// <param name="isEnabled">Loggerを有効化する条件。省略時は常に有効。</param>
        public static void RegisterLogger<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Func<IObjectResolver, bool> isEnabled = null)
        {
            builder.Register<ILogger<T>>(
                resolver =>
                {
                    if (isEnabled != null && !isEnabled(resolver))
                    {
                        return NullLogger<T>.Instance;
                    }

                    ILoggerFactory factory = resolver.Resolve<ILoggerFactory>();
                    return factory.CreateLogger<T>();
                },
                lifetime);
        }
    }
}
