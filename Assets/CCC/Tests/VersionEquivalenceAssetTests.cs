using System;
using System.Collections.Generic;
using Ccc.Core.Model;
using Ccc.Core.UseCases;
using Ccc.Infrastructure.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Ccc.Tests
{
    /// <summary>
    /// バージョン同一視アセットの検証と、純粋な互換方針への変換を検証します。
    /// </summary>
    public sealed class VersionEquivalenceAssetTests
    {
        private const string ValidJson =
            "{\"_entries\":[" +
            "{\"_gameMode\":0,\"_difficulty\":0,\"_versionGroups\":[{\"_versions\":[\"1.0.1\",\"1.0.2\"]}]}," +
            "{\"_gameMode\":0,\"_difficulty\":1,\"_versionGroups\":[{\"_versions\":[\"1.0.1\",\"1.0.2\"]}]}," +
            "{\"_gameMode\":0,\"_difficulty\":2,\"_versionGroups\":[{\"_versions\":[\"1.0.1\"]},{\"_versions\":[\"1.0.2\"]}]}" +
            "]}";

        private readonly List<ScriptableObject> _createdAssets = new List<ScriptableObject>();

        /// <summary>
        /// テストで生成したアセットを破棄します。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _createdAssets.Count; index++)
            {
                UnityEngine.Object.DestroyImmediate(_createdAssets[index]);
            }

            _createdAssets.Clear();
        }

        /// <summary>
        /// 正しい設定はエラーにならず、難易度ごとの設定を持つ互換方針へ変換されることを検証します。
        /// </summary>
        [Test]
        public void CreateCompatibilityPolicy_WhenValid_ShouldUseGroupsOfEachDifficulty()
        {
            HighScoreVersionEquivalenceAsset asset = CreateHighScoreAsset(ValidJson);
            var record = new HighScoreRecord(5, "1.0.1");

            VersionEquivalenceHighScoreCompatibilityPolicy policy = asset.CreateCompatibilityPolicy();

            Assert.That(asset.Validate(), Is.Empty);
            Assert.That(
                policy.Decide(GameMode.LifeEndless, GameDifficulty.Easy, record, "1.0.2").Action,
                Is.EqualTo(HighScoreCompatibilityAction.Keep));
            Assert.That(
                policy.Decide(GameMode.LifeEndless, GameDifficulty.Hard, record, "1.0.2").Action,
                Is.EqualTo(HighScoreCompatibilityAction.Ignore));
        }

        /// <summary>
        /// 不正な設定を検出し、互換方針の生成を拒否することを検証します。
        /// </summary>
        /// <param name="json">アセットの設定内容。</param>
        [TestCase("{\"_entries\":[{\"_gameMode\":0,\"_difficulty\":0,\"_versionGroups\":[{\"_versions\":[\" \"]}]}]}")]
        [TestCase("{\"_entries\":[{\"_gameMode\":0,\"_difficulty\":0,\"_versionGroups\":[{\"_versions\":[\"1.0.1\"]},{\"_versions\":[\"1.0.1\"]}]}]}")]
        [TestCase("{\"_entries\":[{\"_gameMode\":0,\"_difficulty\":0},{\"_gameMode\":0,\"_difficulty\":0}]}")]
        [TestCase("{\"_entries\":[{\"_gameMode\":99,\"_difficulty\":0}]}")]
        [TestCase("{\"_entries\":[{\"_gameMode\":0,\"_difficulty\":99}]}")]
        public void Validate_WhenSettingIsInvalid_ShouldReportErrors(string json)
        {
            HighScoreVersionEquivalenceAsset asset = CreateHighScoreAsset(json);

            Assert.That(asset.Validate(), Is.Not.Empty);
            Assert.Throws<InvalidOperationException>(() => asset.CreateCompatibilityPolicy());
        }

        /// <summary>
        /// EntryAssetにスコアの設定がない場合はエラーになることを検証します。
        /// </summary>
        [Test]
        public void EntryAsset_WhenHighScoreIsMissing_ShouldReportError()
        {
            VersionEquivalenceEntryAsset entryAsset = CreateAsset<VersionEquivalenceEntryAsset>();

            Assert.That(entryAsset.Validate(), Is.Not.Empty);
            Assert.Throws<InvalidOperationException>(() => entryAsset.CreateHighScoreCompatibilityPolicy());
        }

        /// <summary>
        /// EntryAssetが全ての組に登録済みのバージョンでは登録漏れを報告しないことを検証します。
        /// </summary>
        [Test]
        public void FindUnregisteredVersionErrors_WhenVersionIsRegistered_ShouldReturnEmpty()
        {
            VersionEquivalenceEntryAsset entryAsset = CreateEntryAsset(ValidJson);

            Assert.That(entryAsset.Validate(), Is.Empty);
            Assert.That(entryAsset.FindUnregisteredVersionErrors("1.0.1"), Is.Empty);
        }

        /// <summary>
        /// 未登録のバージョンでは、全ての組について登録漏れを報告することを検証します。
        /// </summary>
        [Test]
        public void FindUnregisteredVersionErrors_WhenVersionIsMissing_ShouldReportEveryTarget()
        {
            VersionEquivalenceEntryAsset entryAsset = CreateEntryAsset(ValidJson);

            IReadOnlyList<string> errors = entryAsset.FindUnregisteredVersionErrors("9.9.9");

            Assert.That(
                errors,
                Has.Count.EqualTo(
                    Enum.GetValues(typeof(GameMode)).Length * Enum.GetValues(typeof(GameDifficulty)).Length));
        }

        private VersionEquivalenceEntryAsset CreateEntryAsset(string highScoreJson)
        {
            VersionEquivalenceEntryAsset entryAsset = CreateAsset<VersionEquivalenceEntryAsset>();
            var serializedObject = new SerializedObject(entryAsset);
            serializedObject.FindProperty("_highScore").objectReferenceValue = CreateHighScoreAsset(highScoreJson);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return entryAsset;
        }

        private HighScoreVersionEquivalenceAsset CreateHighScoreAsset(string json)
        {
            HighScoreVersionEquivalenceAsset asset = CreateAsset<HighScoreVersionEquivalenceAsset>();
            JsonUtility.FromJsonOverwrite(json, asset);
            return asset;
        }

        private TAsset CreateAsset<TAsset>()
            where TAsset : ScriptableObject
        {
            TAsset asset = ScriptableObject.CreateInstance<TAsset>();
            _createdAssets.Add(asset);
            return asset;
        }
    }
}
