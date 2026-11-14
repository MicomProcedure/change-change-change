using System.Collections.Generic;
using Ccc.Infrastructure.Assets;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Ccc.Editor
{
    /// <summary>
    /// ビルド対象のバージョンがバージョン同一視設定に登録されていない場合に、ビルドを中止します。
    /// </summary>
    public sealed class VersionEquivalenceBuildValidator : IPreprocessBuildWithReport
    {
        private const int BuildCallbackOrder = 0;

        /// <summary>
        /// ビルド前処理の実行順を取得します。
        /// </summary>
        public int callbackOrder
        {
            get { return BuildCallbackOrder; }
        }

        /// <summary>
        /// プロジェクト内の全てのバージョン同一視設定を検証します。設定アセットがない場合は何もしません。
        /// </summary>
        /// <param name="report">ビルドレポート。</param>
        /// <exception cref="BuildFailedException">設定が不正、またはビルド対象のバージョンが未登録の場合。</exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            var errors = new List<string>();
            string[] assetGuids = AssetDatabase.FindAssets($"t:{nameof(VersionEquivalenceEntryAsset)}");
            for (int index = 0; index < assetGuids.Length; index++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[index]);
                VersionEquivalenceEntryAsset asset =
                    AssetDatabase.LoadAssetAtPath<VersionEquivalenceEntryAsset>(assetPath);
                CollectErrors(asset, assetPath, errors);
            }

            if (errors.Count > 0)
            {
                throw new BuildFailedException(string.Join("\n", errors));
            }
        }

        private static void CollectErrors(
            VersionEquivalenceEntryAsset asset,
            string assetPath,
            List<string> errors)
        {
            IReadOnlyList<string> validationErrors = asset.Validate();
            IReadOnlyList<string> assetErrors = validationErrors.Count > 0
                ? validationErrors
                : asset.FindUnregisteredVersionErrors(PlayerSettings.bundleVersion);

            for (int index = 0; index < assetErrors.Count; index++)
            {
                errors.Add($"{assetPath}: {assetErrors[index]}");
            }
        }
    }
}
