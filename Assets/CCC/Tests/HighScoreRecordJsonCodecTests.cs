using Ccc.Core.Model;
using Ccc.Infrastructure.Persistence;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// ハイスコア記録と保存用JSONの相互変換を検証します。
    /// </summary>
    public sealed class HighScoreRecordJsonCodecTests
    {
        private const string AppVersion = "1.0.1";

        /// <summary>
        /// 変換した記録を同じ内容へ復元できることを検証します。
        /// </summary>
        [Test]
        public void TryDecode_WhenEncodedRecord_ShouldRestoreRecord()
        {
            string json = HighScoreRecordJsonCodec.Encode(new HighScoreRecord(12, AppVersion));

            bool isDecoded = HighScoreRecordJsonCodec.TryDecode(json, out HighScoreRecord record);

            Assert.That(isDecoded, Is.True);
            Assert.That(record.Score, Is.EqualTo(12));
            Assert.That(record.AppVersion, Is.EqualTo(AppVersion));
        }

        /// <summary>
        /// 保存形式の版数がJSONに含まれることを検証します。
        /// </summary>
        [Test]
        public void Encode_ShouldContainCurrentFormatVersion()
        {
            string json = HighScoreRecordJsonCodec.Encode(new HighScoreRecord(12, AppVersion));

            Assert.That(json, Does.Contain($"\"formatVersion\":{HighScoreRecordJsonCodec.CurrentFormatVersion}"));
        }

        /// <summary>
        /// 不正な内容は復元せず、例外も送出しないことを検証します。
        /// </summary>
        /// <param name="json">復元対象の文字列。</param>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{\"formatVersion\":1,\"score\":-1,\"appVersion\":\"1.0.1\"}")]
        [TestCase("{\"formatVersion\":1,\"score\":1,\"appVersion\":\" \"}")]
        [TestCase("{\"formatVersion\":1,\"score\":1}")]
        [TestCase("{\"formatVersion\":0,\"score\":1,\"appVersion\":\"1.0.1\"}")]
        [TestCase("{\"formatVersion\":999,\"score\":1,\"appVersion\":\"1.0.1\"}")]
        public void TryDecode_WhenContentIsInvalid_ShouldReturnFalse(string json)
        {
            bool isDecoded = HighScoreRecordJsonCodec.TryDecode(json, out HighScoreRecord record);

            Assert.That(isDecoded, Is.False);
            Assert.That(record, Is.Null);
        }
    }
}
