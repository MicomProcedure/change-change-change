using System;
using System.IO;
using Ccc.Bootstrap.Log;
using Ccc.Infrastructure.Audio;
using Ccc.Infrastructure.Logging;
using Ccc.Presentation.Audio;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Microsoft.Extensions.Logging;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// 全画面で共有するAudio instanceとLog構成を登録するApplication Scopeです。
    /// </summary>
    public sealed class ApplicationLifetimeScope : LifetimeScope
    {
        [SerializeField] private UnityAudioPlayer audioPlayer;
        [SerializeField] private bool isDebugMode;
        [SerializeField] private TitleScreenState startingTitleScreenState = TitleScreenState.MainMenu;
        [SerializeField] private LogSettingsAsset logSettingsAsset;

        /// <summary>
        /// Application lifetimeのAudio出力とLog構成を登録します。
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
            RegisterLogging(builder);
        }

        private void RegisterLogging(IContainerBuilder builder)
        {
            if (logSettingsAsset == null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScope requires a LogSettingsAsset reference.");
            }

            string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
            LogDirectoryOptions directoryOptions = logSettingsAsset.CreateLogDirectoryOptions(projectRootPath);
            LogOutputOptions outputOptions = logSettingsAsset.CreateLogOutputOptions();
            DateTime startedAt = DateTime.Now;
            builder.RegisterInstance(directoryOptions);
            builder.RegisterInstance(logSettingsAsset.CreateMissionDrawDiagnosticsOptions());
            builder.Register<ILoggerFactory>(
                _ => LoggerFactoryComposer.Create(outputOptions, directoryOptions, startedAt),
                Lifetime.Singleton);
        }
    }
}
