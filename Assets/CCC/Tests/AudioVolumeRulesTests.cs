using Ccc.Infrastructure.Audio;
using NUnit.Framework;

namespace Ccc.Tests
{
    /// <summary>
    /// 再生方法ごとの音量の有効範囲を検証します。
    /// </summary>
    public sealed class AudioVolumeRulesTests
    {
        /// <summary>
        /// 単発再生では0以上の音量を上限なしで許可することを検証します。
        /// </summary>
        /// <param name="volume">検証する音量。</param>
        [TestCase(0f)]
        [TestCase(1f)]
        [TestCase(1.5f)]
        [TestCase(100f)]
        public void IsValidOneShotVolume_WhenZeroOrMore_ShouldReturnTrue(float volume)
        {
            Assert.That(AudioVolumeRules.IsValidOneShotVolume(volume), Is.True);
        }

        /// <summary>
        /// 単発再生では負の音量と有限でない音量を拒否することを検証します。
        /// </summary>
        /// <param name="volume">検証する音量。</param>
        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void IsValidOneShotVolume_WhenNegativeOrNotFinite_ShouldReturnFalse(float volume)
        {
            Assert.That(AudioVolumeRules.IsValidOneShotVolume(volume), Is.False);
        }

        /// <summary>
        /// AudioSourceの音量としては0以上1以下を許可することを検証します。
        /// </summary>
        /// <param name="volume">検証する音量。</param>
        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        public void IsValidAudioSourceVolume_WhenWithinZeroToOne_ShouldReturnTrue(float volume)
        {
            Assert.That(AudioVolumeRules.IsValidAudioSourceVolume(volume), Is.True);
        }

        /// <summary>
        /// AudioSourceの音量としては範囲外の値と有限でない値を拒否することを検証します。
        /// </summary>
        /// <param name="volume">検証する音量。</param>
        [TestCase(-0.1f)]
        [TestCase(1.01f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void IsValidAudioSourceVolume_WhenOutOfRangeOrNotFinite_ShouldReturnFalse(float volume)
        {
            Assert.That(AudioVolumeRules.IsValidAudioSourceVolume(volume), Is.False);
        }
    }
}
