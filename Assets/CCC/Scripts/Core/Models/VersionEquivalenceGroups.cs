using System;
using System.Collections.Generic;

namespace Ccc.Core.Model
{
    /// <summary>
    /// 保存データを同一視するアプリケーションバージョンのグループを表す不変データです。
    /// </summary>
    public sealed class VersionEquivalenceGroups
    {
        private readonly Dictionary<string, int> _groupIndexByVersion =
            new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>
        /// VersionEquivalenceGroupsを生成します。
        /// </summary>
        /// <param name="groups">同一視するバージョンのグループ一覧。</param>
        /// <exception cref="ArgumentNullException">groupsがnullの場合。</exception>
        /// <exception cref="ArgumentException">
        /// グループがnull、バージョンが空白、または同じバージョンを複数回指定した場合。
        /// </exception>
        public VersionEquivalenceGroups(IEnumerable<IReadOnlyCollection<string>> groups)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(nameof(groups));
            }

            int groupIndex = 0;
            foreach (IReadOnlyCollection<string> group in groups)
            {
                if (group == null)
                {
                    throw new ArgumentException("Version group must not be null.", nameof(groups));
                }

                foreach (string version in group)
                {
                    if (string.IsNullOrWhiteSpace(version))
                    {
                        throw new ArgumentException("Version must not be blank.", nameof(groups));
                    }

                    if (_groupIndexByVersion.ContainsKey(version))
                    {
                        throw new ArgumentException($"Version {version} is registered more than once.", nameof(groups));
                    }

                    _groupIndexByVersion.Add(version, groupIndex);
                }

                groupIndex++;
            }
        }

        /// <summary>
        /// 指定したバージョンがいずれかのグループに登録されているかを判定します。
        /// </summary>
        /// <param name="version">判定するバージョン。</param>
        /// <returns>登録されている場合はtrue。</returns>
        public bool Contains(string version)
        {
            return version != null && _groupIndexByVersion.ContainsKey(version);
        }

        /// <summary>
        /// 2つのバージョンを同一視するかを判定します。同じバージョンは未登録でも同一視します。
        /// </summary>
        /// <param name="firstVersion">1つ目のバージョン。</param>
        /// <param name="secondVersion">2つ目のバージョン。</param>
        /// <returns>同一視する場合はtrue。</returns>
        public bool AreEquivalent(string firstVersion, string secondVersion)
        {
            if (string.Equals(firstVersion, secondVersion, StringComparison.Ordinal))
            {
                return true;
            }

            return Contains(firstVersion) &&
                   Contains(secondVersion) &&
                   _groupIndexByVersion[firstVersion] == _groupIndexByVersion[secondVersion];
        }
    }
}
