using Ccc.Infrastructure.Navigation;
using VContainer;
using VContainer.Unity;

namespace Ccc.Bootstrap
{
    /// <summary>
    /// TitleSceneの依存関係を登録します。
    /// </summary>
    public sealed class TitleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<OptionMenuModel>(Lifetime.Scoped);
            builder.Register<TitleScreenModel>(Lifetime.Scoped);
            builder.RegisterComponentInHierarchy<TitleScreenView>();
            builder.RegisterEntryPoint<TitleEntryPoint>();
        }
    }
}