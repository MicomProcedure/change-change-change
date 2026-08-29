using System;
using Ccc.Infrastructure.Audio;
using Ccc.Presentation.Audio;
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

        /// <summary>
        /// Application lifetimeのAudio出力を登録します。
        /// </summary>
        /// <param name="builder">VContainerの登録Builder。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            if (audioPlayer == null)
            {
                throw new InvalidOperationException(
                    "ApplicationLifetimeScope requires a UnityAudioPlayer reference.");
            }

            builder.RegisterComponent(audioPlayer).As<IAudioPlayer>();
        }
    }
}
