using Ccc.Infrastructure.Audio;
using Ccc.Presentation.Audio;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Microsoft.Extensions.Logging;
using ZLogger.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// 全画面で共有するAudio instanceを登録するApplication Scopeです。
    /// </summary>
    public sealed class ApplicationLifetimeScope : LifetimeScope
    {
        [SerializeField] private UnityAudioPlayer audioPlayer;
        [SerializeField] private bool isDebugMode;
        [SerializeField] private TitleScreenState startingTitleScreenState = TitleScreenState.MainMenu;
        // 出力するLogLevel設定
        [SerializeField] private LogLevel logLevel = LogLevel.Debug;

        /// <summary>
        /// Application lifetimeのAudio出力を登録します。
        /// </summary>
        /// <param name="builder">VContainerの登録Builder。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<IRuntimeMode>(new RuntimeMode(isDebugMode));

            if (audioPlayer != null)
            {
                builder.RegisterComponent(audioPlayer).As<IAudioPlayer>();
            }
            else
            {
                UnityEngine.Debug.LogError(
                    "ApplicationLifetimeScope has no UnityAudioPlayer. " +
                    "Gameplay will continue without audio.");
                builder.Register<IAudioPlayer, NullAudioPlayer>(Lifetime.Singleton);
            }

            builder.Register<ISceneNavigator, SceneNavigator>(Lifetime.Singleton);
            builder.RegisterInstance(new TitleStartRequest(startingTitleScreenState));

            // LoggerFactoryを登録
            builder.Register<ILoggerFactory>(
                _ => LoggerFactory.Create(logging => {  // 匿名関数を利用してILoggerFactoryのFactoryを登録
                    logging.SetMinimumLevel(logLevel);
#if UNITY_EDITOR
                    logging.AddZLoggerUnityDebug();
#endif
                }),
                Lifetime.Singleton
            );
        }
    }
}
