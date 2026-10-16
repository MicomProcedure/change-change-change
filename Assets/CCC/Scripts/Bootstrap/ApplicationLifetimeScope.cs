using Ccc.Infrastructure.Audio;
using Ccc.Presentation.Audio;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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

        /// <summary>
        /// Application lifetimeのAudio出力を登録します。
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

            builder.RegisterEntryPoint<ApplicationEntryPoint>();
        }
    }
}