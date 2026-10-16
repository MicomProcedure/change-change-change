using System;
using System.Collections.Generic;
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
                builder.RegisterComponent(audioPlayer).As<IAudioPlayer, IAudioVolumeControl>();
            }
            else
            {
                UnityEngine.Debug.LogError(
                    "ApplicationLifetimeScope has no UnityAudioPlayer. " +
                    "Gameplay will continue without audio.");
                builder.Register<NullAudioPlayer>(Lifetime.Singleton)
                    .As<IAudioPlayer, IAudioVolumeControl>();
            }

            builder.Register<IOptionSettingsStore, PlayerPrefsOptionSettingsStore>(Lifetime.Singleton);
            builder.Register<OptionAudioInitializer>(Lifetime.Singleton);

            builder.Register<ISceneNavigator, SceneNavigator>(Lifetime.Singleton);
            builder.RegisterInstance(new TitleStartRequest(startingTitleScreenState));

            RegisterLogging(builder);

            builder.RegisterEntryPoint<ApplicationEntryPoint>();
        }

        private void RegisterLogging(IContainerBuilder builder)
        {
            if (logSettingsAsset == null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScope requires a LogSettingsAsset reference.");
            }

            IReadOnlyList<string> errors = logSettingsAsset.Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Log settings validation failed: " + string.Join(" | ", errors));
            }

            DateTime startedAt = DateTime.Now;
            builder.RegisterInstance(logSettingsAsset);
            builder.Register<ILoggerFactory>(
                _ => LoggerFactoryComposer.Create(logSettingsAsset, startedAt),
                Lifetime.Singleton);

        }
    }
}