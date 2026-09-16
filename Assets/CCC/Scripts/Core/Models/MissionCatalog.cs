using System;
using System.Collections.Generic;

namespace Ccc.Core.Model
{
    /// <summary>
    /// セッション中に抽選可能なミッション候補一覧を保持する不変カタログ。
    /// </summary>
    public sealed class MissionCatalog
    {
        private readonly MissionCandidate[] _candidates;

        public IReadOnlyList<MissionCandidate> Candidates => _candidates;

        public MissionCatalog(IReadOnlyList<MissionCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (candidates.Count == 0)
            {
                throw new ArgumentException("Candidates cannot be empty.", nameof(candidates));
            }

            var list = new MissionCandidate[candidates.Count];
            var duplicateChecker = new HashSet<MissionCondition>();

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null)
                {
                    throw new ArgumentException("Candidates cannot contain null elements.", nameof(candidates));
                }

                if (!duplicateChecker.Add(candidate.Condition))
                {
                    throw new ArgumentException(
                        $"Duplicate condition '{candidate.Condition.Kind}'.",
                        nameof(candidates));
                }

                list[i] = candidate;
            }

            _candidates = list;
        }
    }
}
