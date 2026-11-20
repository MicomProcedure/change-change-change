using System;
using System.Collections.Generic;
using Ccc.Infrastructure.Navigation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// TitleSceneの依存関係を登録します。
    /// </summary>
    public sealed class TitleLifetimeScope : LifetimeScope
    {
        [Tooltip("プライバシーポリシーなど、アプリ外で開くWebページのURL。")]
        [SerializeField] private ExternalPageUrlAsset _externalPageUrlAsset;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<OptionMenuModel>(Lifetime.Scoped);
            builder.Register<TitleScreenModel>(Lifetime.Scoped);
            builder.Register<OptionVolumeAudioSynchronizer>(Lifetime.Scoped);
            builder.Register<TitleScreenTransitionSoundPlayer>(Lifetime.Scoped);
            builder.Register<TitleSelectorChangeSoundPlayer>(Lifetime.Scoped);
            RegisterExternalPageOpener(builder);
            builder.RegisterComponentInHierarchy<TitleScreenView>();
            builder.RegisterEntryPoint<TitleEntryPoint>();
        }

        private void RegisterExternalPageOpener(IContainerBuilder builder)
        {
            if (_externalPageUrlAsset == null)
            {
                throw new InvalidOperationException(
                    "TitleLifetimeScope requires an ExternalPageUrlAsset reference.");
            }

            IReadOnlyList<string> errors = _externalPageUrlAsset.Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "External page url validation failed: " + string.Join(" | ", errors));
            }

            builder.RegisterInstance(_externalPageUrlAsset);
            builder.Register<IExternalPageOpener, UnityExternalPageOpener>(Lifetime.Scoped);
        }
    }
}
