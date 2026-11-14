using Ccc.Infrastructure.Platform;
using Ccc.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Ccc.Tests
{
    /// <summary>
    /// 実行環境に応じたプレイ環境のプラットフォーム判定を検証します。
    /// </summary>
    public sealed class PlayPlatformResolverTests
    {
        /// <summary>
        /// WebGLではブラウザのモバイル判定に応じたプラットフォームを返すことを検証します。
        /// </summary>
        /// <param name="isMobilePlatform">ブラウザがモバイル端末で動作しているかどうか。</param>
        /// <param name="expectedPlatform">期待するプラットフォーム。</param>
        [TestCase(false, PlayPlatform.PC)]
        [TestCase(true, PlayPlatform.Mobile)]
        public void Resolve_WhenWebGL_ShouldUseMobilePlatformDetection(
            bool isMobilePlatform,
            PlayPlatform expectedPlatform)
        {
            PlayPlatform platform = PlayPlatformResolver.Resolve(
                PlayPlatformOverride.Auto,
                false,
                RuntimePlatform.WebGLPlayer,
                isMobilePlatform,
                null);

            Assert.That(platform, Is.EqualTo(expectedPlatform));
        }

        /// <summary>
        /// WebGL以外では既存の実行プラットフォーム判定を維持することを検証します。
        /// </summary>
        /// <param name="runtimePlatform">実行プラットフォーム。</param>
        /// <param name="expectedPlatform">期待するプラットフォーム。</param>
        /// <param name="isMobilePlatform">判定結果に影響しないモバイル端末判定。</param>
        [TestCase(RuntimePlatform.WindowsPlayer, PlayPlatform.PC, false)]
        [TestCase(RuntimePlatform.WindowsPlayer, PlayPlatform.PC, true)]
        [TestCase(RuntimePlatform.OSXPlayer, PlayPlatform.PC, false)]
        [TestCase(RuntimePlatform.OSXPlayer, PlayPlatform.PC, true)]
        [TestCase(RuntimePlatform.LinuxPlayer, PlayPlatform.PC, false)]
        [TestCase(RuntimePlatform.LinuxPlayer, PlayPlatform.PC, true)]
        [TestCase(RuntimePlatform.Android, PlayPlatform.Mobile, false)]
        [TestCase(RuntimePlatform.Android, PlayPlatform.Mobile, true)]
        [TestCase(RuntimePlatform.IPhonePlayer, PlayPlatform.Mobile, false)]
        [TestCase(RuntimePlatform.IPhonePlayer, PlayPlatform.Mobile, true)]
        public void Resolve_WhenNotWebGL_ShouldPreserveRuntimePlatformDetection(
            RuntimePlatform runtimePlatform,
            PlayPlatform expectedPlatform,
            bool isMobilePlatform)
        {
            PlayPlatform platform = PlayPlatformResolver.Resolve(
                PlayPlatformOverride.Auto,
                false,
                runtimePlatform,
                isMobilePlatform,
                null);

            Assert.That(platform, Is.EqualTo(expectedPlatform));
        }

        /// <summary>
        /// EditorではWebGLの端末判定より上書き設定と起動Sceneを優先することを検証します。
        /// </summary>
        /// <param name="editorOverride">Editorの上書き設定。</param>
        /// <param name="startupScenePlatform">起動Sceneのプラットフォーム。</param>
        /// <param name="expectedPlatform">期待するプラットフォーム。</param>
        /// <param name="isMobilePlatform">判定結果に影響しないモバイル端末判定。</param>
        [TestCase(PlayPlatformOverride.PC, PlayPlatform.Mobile, PlayPlatform.PC, false)]
        [TestCase(PlayPlatformOverride.PC, PlayPlatform.Mobile, PlayPlatform.PC, true)]
        [TestCase(PlayPlatformOverride.Mobile, PlayPlatform.PC, PlayPlatform.Mobile, false)]
        [TestCase(PlayPlatformOverride.Mobile, PlayPlatform.PC, PlayPlatform.Mobile, true)]
        [TestCase(PlayPlatformOverride.Auto, PlayPlatform.PC, PlayPlatform.PC, false)]
        [TestCase(PlayPlatformOverride.Auto, PlayPlatform.PC, PlayPlatform.PC, true)]
        [TestCase(PlayPlatformOverride.Auto, PlayPlatform.Mobile, PlayPlatform.Mobile, false)]
        [TestCase(PlayPlatformOverride.Auto, PlayPlatform.Mobile, PlayPlatform.Mobile, true)]
        [TestCase(PlayPlatformOverride.Auto, null, PlayPlatform.Mobile, false)]
        [TestCase(PlayPlatformOverride.Auto, null, PlayPlatform.Mobile, true)]
        public void Resolve_WhenEditor_ShouldPreserveEditorPlatformDetection(
            PlayPlatformOverride editorOverride,
            PlayPlatform? startupScenePlatform,
            PlayPlatform expectedPlatform,
            bool isMobilePlatform)
        {
            PlayPlatform platform = PlayPlatformResolver.Resolve(
                editorOverride,
                true,
                RuntimePlatform.WebGLPlayer,
                isMobilePlatform,
                startupScenePlatform);

            Assert.That(platform, Is.EqualTo(expectedPlatform));
        }
    }
}
