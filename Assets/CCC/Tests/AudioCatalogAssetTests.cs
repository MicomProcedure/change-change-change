using System.Collections.Generic;
using Ccc.Infrastructure.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Ccc.Tests
{
    /// <summary>
    /// AudioCatalogAssetの音量検証を、SE一覧とBGM一覧の違いに注目して検証します。
    /// </summary>
    public sealed class AudioCatalogAssetTests
    {
        private const string InvalidVolumeMessage = "invalid volume";
        private const string SEEntryParsedMessage = "SEChangeClick has no AudioClip.";
        private const string BgmEntryParsedMessage = "BgmTitle has no AudioClip.";
        private const string SEVolumeAboveOneJson = "{\"_seEntries\":[{\"_key\":200,\"_volume\":1.5}]}";
        private const string SENegativeVolumeJson = "{\"_seEntries\":[{\"_key\":200,\"_volume\":-0.5}]}";
        private const string BgmVolumeOneJson = "{\"_bgmEntries\":[{\"_key\":100,\"_volume\":1.0}]}";
        private const string BgmVolumeAboveOneJson = "{\"_bgmEntries\":[{\"_key\":100,\"_volume\":1.5}]}";

        private AudioCatalogAsset _catalog;

        /// <summary>
        /// テスト用のカタログを破棄します。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
        }

        /// <summary>
        /// SE一覧では1を超える音量を許可することを検証します。
        /// </summary>
        [Test]
        public void Validate_WhenSEVolumeIsAboveOne_ShouldNotReportVolumeError()
        {
            CreateCatalog(SEVolumeAboveOneJson);

            IReadOnlyList<string> errors = _catalog.Validate();

            Assert.That(errors, Has.Some.Contains(SEEntryParsedMessage));
            Assert.That(errors, Has.None.Contains(InvalidVolumeMessage));
        }

        /// <summary>
        /// SE一覧でも負の音量を拒否することを検証します。
        /// </summary>
        [Test]
        public void Validate_WhenSEVolumeIsNegative_ShouldReportVolumeError()
        {
            CreateCatalog(SENegativeVolumeJson);

            IReadOnlyList<string> errors = _catalog.Validate();

            Assert.That(errors, Has.Some.Contains(InvalidVolumeMessage));
        }

        /// <summary>
        /// BGM一覧では音量1を許可することを検証します。
        /// </summary>
        [Test]
        public void Validate_WhenBgmVolumeIsOne_ShouldNotReportVolumeError()
        {
            CreateCatalog(BgmVolumeOneJson);

            IReadOnlyList<string> errors = _catalog.Validate();

            Assert.That(errors, Has.Some.Contains(BgmEntryParsedMessage));
            Assert.That(errors, Has.None.Contains(InvalidVolumeMessage));
        }

        /// <summary>
        /// BGM一覧では1を超える音量を拒否することを検証します。
        /// </summary>
        [Test]
        public void Validate_WhenBgmVolumeIsAboveOne_ShouldReportVolumeError()
        {
            CreateCatalog(BgmVolumeAboveOneJson);

            IReadOnlyList<string> errors = _catalog.Validate();

            Assert.That(errors, Has.Some.Contains(InvalidVolumeMessage));
        }

        private void CreateCatalog(string catalogJson)
        {
            // 生成時のOnEnableが検証エラーをログへ出力するため無視する。設定はテストごとのLogScopeで元に戻る
            LogAssert.ignoreFailingMessages = true;
            _catalog = ScriptableObject.CreateInstance<AudioCatalogAsset>();
            JsonUtility.FromJsonOverwrite(catalogJson, _catalog);
        }
    }
}
