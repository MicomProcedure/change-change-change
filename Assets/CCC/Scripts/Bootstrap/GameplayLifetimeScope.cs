using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.Logic;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using Ccc.Infrastructure.Random;
using Ccc.Infrastructure.Time;
using Ccc.Infrastructure.Navigation;
using Ccc.Infrastructure.Gameplay;
using Ccc.Presentation;
using Ccc.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using NavigationGameplayStartRequest = Ccc.Presentation.Navigation.GameplayNavigationRequest;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// Gameplay画面のScoped依存をComposition Rootで登録します。
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayConfigCatalogAsset configCatalogAsset;

        /// <summary>
        /// Gameplay Scopeの依存関係を登録します。
        /// </summary>
        /// <param name="builder">VContainerの登録Builder。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssetReferences();

            builder.RegisterInstance(configCatalogAsset);
            builder.Register<GameplayStartContext>(
                resolver => CreateStartContext(resolver),
                Lifetime.Scoped);
            builder.Register<GameplayConfigEntry>(
                resolver => resolver.Resolve<GameplayStartContext>().ConfigEntry,
                Lifetime.Scoped);
            builder.Register<MissionPresetAsset>(
                resolver => resolver.Resolve<GameplayConfigEntry>().MissionPresetAsset,
                Lifetime.Scoped);
            builder.Register<IMissionTextSource>(
                resolver =>
                {
                    MissionTextAsset missionTextAsset =
                        resolver.Resolve<GameplayConfigEntry>().MissionTextAsset;
                    return missionTextAsset == null
                        ? new FallbackMissionTextSource()
                        : missionTextAsset;
                },
                Lifetime.Scoped);

            builder.Register<GameplayConfig>(
                resolver => resolver
                    .Resolve<GameplayConfigEntry>()
                    .GameplayConfigAsset
                    .CreateRuntimeConfig(),
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
            if (configCatalogAsset == null)
            {
                throw new InvalidOperationException(
                    "GameplayLifetimeScope requires a GameplayConfigCatalogAsset reference.");
            }
        }

        private GameplayStartContext CreateStartContext(IObjectResolver resolver)
        {
            IReadOnlyList<string> catalogErrors = configCatalogAsset.Validate();
            if (catalogErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Gameplay config catalog validation failed: " +
                    string.Join(" | ", catalogErrors));
            }

            NavigationGameplayStartRequest request;
            if (!resolver.TryResolve(out request))
            {
                request = configCatalogAsset.CreateDefaultRequest();
                Debug.LogWarning(
                    "GameplayNavigationRequest was not provided. " +
                    "Using the default GameplayConfigCatalog entry for direct Scene Play.");
            }

            if (!configCatalogAsset.TryGetEntry(request, out GameplayConfigEntry entry))
            {
                throw new InvalidOperationException(
                    $"No GameplayConfigCatalog entry matches " +
                    $"GameMode.{request.GameMode} and GameDifficulty.{request.Difficulty}.");
            }

            return new GameplayStartContext(request, entry);
        }
    }
}
