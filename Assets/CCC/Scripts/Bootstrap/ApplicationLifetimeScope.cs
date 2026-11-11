using System;
using System.Collections.Generic;
using System.Linq;
using Ccc.Bootstrap.Log;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Audio;
using Ccc.Infrastructure.Logging;
using Ccc.Infrastructure.Persistence;
using Ccc.Infrastructure.Platform;
using Ccc.Presentation.Audio;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField] private UnityAudioPlayer _audioPlayer;
        [SerializeField] private bool _isDebugMode;
        [Tooltip("DebugModeがOnの時だけ、起動時のハイスコアとして使う記録。指定のない組は記録なしで始まり、同じ組は後の指定を使う。")]
        [SerializeField] private List<DebugHighScoreEntry> _debugHighScoreEntries = new List<DebugHighScoreEntry>();
        [SerializeField] private TitleScreenState _startingTitleScreenState = TitleScreenState.MainMenu;
        [SerializeField] private LogSettingsAsset _logSettingsAsset;
        [SerializeField] private SceneCatalogAsset _sceneCatalogAsset;
        [Tooltip("保存データを同一視するバージョンの設定。未設定の場合は判定せず、全てのハイスコアを使い続ける。")]
        [SerializeField] private VersionEquivalenceEntryAsset _versionEquivalenceEntryAsset;
        [Tooltip("Editor実行時にのみ適用するプラットフォーム。Autoは起動時のSceneから判定し、特定できなければMobileとして扱う。")]
        [SerializeField] private PlayPlatformOverride _editorPlayPlatformOverride = PlayPlatformOverride.Auto;

        /// <summary>
        /// Application lifetimeのAudio出力とLog構成を登録します。
        /// </summary>
        /// <param name="builder">VContainerの登録Builder。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<IRuntimeMode>(new RuntimeMode(_isDebugMode));
            RegisterSceneCatalog(builder);
            RegisterPlayPlatform(builder);

            if (_audioPlayer != null)
            {
                builder.RegisterComponent(_audioPlayer).As<IAudioPlayer, IAudioVolumeControl>();
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
            builder.Register<ILocalDataEraser, PlayerPrefsLocalDataEraser>(Lifetime.Singleton);
            RegisterHighScore(builder);

            builder.Register<ISceneNavigator, SceneNavigator>(Lifetime.Singleton);
            builder.RegisterInstance(new TitleStartRequest(_startingTitleScreenState));

            RegisterLogging(builder);

            builder.RegisterEntryPoint<ApplicationEntryPoint>();
        }

        private void RegisterHighScore(IContainerBuilder builder)
        {
            builder.Register<IApplicationVersionProvider, UnityApplicationVersionProvider>(Lifetime.Singleton);
            RegisterHighScoreCompatibilityPolicy(builder);
            RegisterHighScoreStore(builder);
            builder.Register<HighScoreService>(Lifetime.Singleton);
        }

        private void RegisterHighScoreCompatibilityPolicy(IContainerBuilder builder)
        {
            if (_versionEquivalenceEntryAsset == null)
            {
                builder.Register<IHighScoreCompatibilityPolicy, KeepAllHighScoreCompatibilityPolicy>(Lifetime.Singleton);
                return;
            }

            IReadOnlyList<string> errors = _versionEquivalenceEntryAsset.Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Version equivalence validation failed: " + string.Join(" | ", errors));
            }

            builder.Register<IHighScoreCompatibilityPolicy>(
                resolver => CreateVersionEquivalencePolicy(
                    resolver.Resolve<IApplicationVersionProvider>().CurrentVersion),
                Lifetime.Singleton);
        }

        private void RegisterHighScoreStore(IContainerBuilder builder)
        {
            // DebugModeの動作確認で本番の記録を汚さないよう、メモリ上だけで保持する
            if (_isDebugMode)
            {
                // 互換方針に無視されないよう、注入する記録には実行中のバージョンを設定する
                builder.Register<IHighScoreStore>(
                    resolver => new InMemoryHighScoreStore(
                        _debugHighScoreEntries,
                        resolver.Resolve<IApplicationVersionProvider>().CurrentVersion),
                    Lifetime.Singleton);
            }
            else
            {
                builder.Register<IHighScoreStore, PlayerPrefsHighScoreStore>(Lifetime.Singleton);
            }
        }

        private IHighScoreCompatibilityPolicy CreateVersionEquivalencePolicy(string currentVersion)
        {
            // 登録漏れはビルド時に検出する。実行時は同じバージョンの記録だけを同一視して継続する
            IReadOnlyList<string> unregisteredVersionErrors =
                _versionEquivalenceEntryAsset.FindUnregisteredVersionErrors(currentVersion);
            for (int index = 0; index < unregisteredVersionErrors.Count; index++)
            {
                UnityEngine.Debug.LogError(unregisteredVersionErrors[index]);
            }

            return _versionEquivalenceEntryAsset.CreateHighScoreCompatibilityPolicy();
        }

        private void RegisterSceneCatalog(IContainerBuilder builder)
        {
            if (_sceneCatalogAsset == null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScope requires a SceneCatalogAsset reference.");
            }

            IReadOnlyList<string> errors = _sceneCatalogAsset.Validate()
                .Concat(_sceneCatalogAsset.ValidateBuildInclusion())
                .ToArray();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Scene catalog validation failed: " + string.Join(" | ", errors));
            }

            builder.RegisterInstance(_sceneCatalogAsset);
        }

        private void RegisterPlayPlatform(IContainerBuilder builder)
        {
            string startupSceneName = SceneManager.GetActiveScene().name;
            PlayPlatform? startupScenePlatform =
                _sceneCatalogAsset.TryGetUniquePlayPlatform(startupSceneName, out PlayPlatform scenePlatform)
                    ? scenePlatform
                    : (PlayPlatform?)null;

            builder.RegisterInstance<IPlayPlatformMode>(new PlayPlatformMode(
                PlayPlatformResolver.Resolve(
                    _editorPlayPlatformOverride,
                    Application.isEditor,
                    Application.platform,
                    startupScenePlatform)));
        }

        private void RegisterLogging(IContainerBuilder builder)
        {
            if (_logSettingsAsset == null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScope requires a LogSettingsAsset reference.");
            }

            IReadOnlyList<string> errors = _logSettingsAsset.Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Log settings validation failed: " + string.Join(" | ", errors));
            }

            DateTime startedAt = DateTime.Now;
            builder.RegisterInstance(_logSettingsAsset);
            builder.Register<ILoggerFactory>(
                _ => LoggerFactoryComposer.Create(_logSettingsAsset, startedAt),
                Lifetime.Singleton);

        }
    }
}