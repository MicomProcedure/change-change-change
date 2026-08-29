using System;
using Ccc.Core.Model;
using Ccc.Core.Logic;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Random;
using Ccc.Infrastructure.Time;
using Ccc.Infrastructure.Navigation;
using Ccc.Infrastructure.Gameplay;
using Ccc.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// Gameplay画面のScoped依存をComposition Rootで登録します。
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayConfigAsset configAsset;
        [SerializeField] private MissionPresetAsset missionPresetAsset;
        [SerializeField] private MissionTextAsset missionTextAsset;

        /// <summary>
        /// Gameplay Scopeの依存関係を登録します。
        /// </summary>
        /// <param name="builder">VContainerの登録Builder。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssetReferences();

            builder.RegisterInstance(configAsset);
            builder.RegisterInstance(missionPresetAsset);
            builder.RegisterInstance<MissionTextAsset>(missionTextAsset)
                .As<IMissionTextSource>();

            builder.Register<GameplayConfig>(
                _ => configAsset.CreateRuntimeConfig(),
                Lifetime.Scoped);
            builder.Register<IRandomSource, UnityRandomSource>(Lifetime.Scoped);
            builder.Register<IMissionDrawStrategy, SimpleMissionDrawStrategy>(Lifetime.Scoped);
            builder.Register<ISectionTimer, UniTaskSectionTimer>(Lifetime.Scoped);
            builder.Register<ExpressionInput>(Lifetime.Scoped);
            builder.Register<IGameplayFlow, GameplayController>(Lifetime.Scoped);
            builder.Register<RationalDecimalFormatter>(Lifetime.Scoped);
            builder.Register<MissionTextFormatter>(Lifetime.Scoped);
            builder.Register<GameplayPresenter>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<GameplayView>().As<IGameplayView>();
            builder.RegisterComponentInHierarchy<GameplayNavigation>()
                .As<IGameplayNavigation>();
            builder.RegisterEntryPoint<GameplayEntryPoint>();
        }

        private void ValidateAssetReferences()
        {
            if (configAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayLifetimeScope requires a GameplayConfigAsset reference.");
            }

            if (missionPresetAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayLifetimeScope requires a MissionPresetAsset reference.");
            }

            if (missionTextAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayLifetimeScope requires a MissionTextAsset reference.");
            }
        }
    }
}
