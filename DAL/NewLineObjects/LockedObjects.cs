using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;

namespace SportRadar.DAL.NewLineObjects
{
    public sealed class LockedObjects
    {
        /*
        IdentityList m_ilLockedGroups = new IdentityList();
        IdentityList m_ilLockedMatches = new IdentityList();
        IdentityList m_ilLockedBetDomains = new IdentityList();
        */

        IdentityList m_ilLockedOdds = new IdentityList();

        /*
        public void SyncLockedGroups(IEnumerable<long> collectionLockedGroupIds)
        {
            m_ilLockedGroups.SafelySynchronize(collectionLockedGroupIds);
        }

        public void SyncLockedMatches(IEnumerable<long> collectionLockedMatchIds)
        {
            m_ilLockedMatches.SafelySynchronize(collectionLockedMatchIds);
        }

        public void SyncLockedBetDomains(IEnumerable<long> collectionLockedBetDomainIds)
        {
            m_ilLockedBetDomains.SafelySynchronize(collectionLockedBetDomainIds);
        }
        */

        public void SyncLockedOdds(IEnumerable<long> collectionLockedOddIds)
        {
            if (collectionLockedOddIds != null)
                m_ilLockedOdds.SafelySynchronize(collectionLockedOddIds);
        }

        public bool IsOddLocked(OddLn odd)
        {
            if (m_ilLockedOdds.Contains(odd.OutcomeId))
            {
                return true;
            }

            /*
            if (m_ilLockedBetDomains.Contains(odd.BetDomainId))
            {
                return true;
            }

            if (IsMatchLocked(odd.BetDomain.Match))
            {
                return true;
            }
            */

            return false;
        }

        /*
        public bool IsMatchLocked(IMatchLn match)
        {
            if (m_ilLockedMatches.Contains(match.MatchId))
            {
                return true;
            }

            SyncList<GroupLn> lMatchGroups = LineSr.Instance.AllObjects.MatchesToGroups.GetMatchGroups(match.MatchId);
         
            foreach (GroupLn group in lMatchGroups)
            {
                if (m_ilLockedGroups.Contains(group.GroupId))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsGroupLocked(GroupLn group)
        {
            return m_ilLockedGroups.Contains(group.GroupId);
        }
        */
    }
}
