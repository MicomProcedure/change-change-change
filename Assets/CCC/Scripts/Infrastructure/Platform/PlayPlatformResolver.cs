using System;
using Ccc.Presentation;
using UnityEngine;

namespace Ccc.Infrastructure.Platform
{
    /// <summary>
    /// 実行環境の情報からプレイ環境のプラットフォームを決定します。
    /// </summary>
    public static class PlayPlatformResolver
    {
        private const PlayPlatform EditorDefaultPlayPlatform = PlayPlatform.Mobile;

        /// <summary>
        /// 実行環境の情報からプレイ環境のプラットフォームを決定します。
        /// </summary>
        /// <param name="editorOverride">Editor実行時にのみ適用する上書き設定。</param>
        /// <param name="isEditor">Editor上で実行しているかどうか。</param>
        /// <param name="runtimePlatform">Unityが報告する実行プラットフォーム。</param>
        /// <param name="startupScenePlatform">起動時のSceneが一意に属するプラットフォーム。特定できない場合はnull。</param>
        /// <returns>
        /// Standaloneビルドの場合はPC、それ以外のビルドの場合はMobile。
        /// Editorでは上書き設定に従い、Autoの場合は起動時のSceneのプラットフォーム、特定できなければMobileを返します。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">未定義の上書き設定の場合。</exception>
        public static PlayPlatform Resolve(
            PlayPlatformOverride editorOverride,
            bool isEditor,
            RuntimePlatform runtimePlatform,
            PlayPlatform? startupScenePlatform)
        {
            if (isEditor)
            {
                return ResolveEditorPlatform(editorOverride, startupScenePlatform);
            }

            return IsStandalonePlayer(runtimePlatform)
                ? PlayPlatform.PC
                : PlayPlatform.Mobile;
        }

        private static PlayPlatform ResolveEditorPlatform(
            PlayPlatformOverride editorOverride,
            PlayPlatform? startupScenePlatform)
        {
            switch (editorOverride)
            {
                case PlayPlatformOverride.Auto:
                    return startupScenePlatform ?? EditorDefaultPlayPlatform;
                case PlayPlatformOverride.Mobile:
                    return PlayPlatform.Mobile;
                case PlayPlatformOverride.PC:
                    return PlayPlatform.PC;
                default:
                    throw new ArgumentOutOfRangeException(nameof(editorOverride), editorOverride, null);
            }
        }

        private static bool IsStandalonePlayer(RuntimePlatform runtimePlatform)
        {
            return runtimePlatform == RuntimePlatform.WindowsPlayer ||
                   runtimePlatform == RuntimePlatform.OSXPlayer ||
                   runtimePlatform == RuntimePlatform.LinuxPlayer;
        }
    }
}
