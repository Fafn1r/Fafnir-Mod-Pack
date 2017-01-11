using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hygiene
{
    public class JobGiver_GetHygiene : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            Need_Hygiene hygiene = pawn.needs.hygiene();
            if (hygiene == null)
            {
                return 0f;
            }
            if (hygiene.CurCategory < this.minCategory)
            {
                return 0f;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds)
            {
                return 0f;
            }
            TimeAssignmentDef timeAssignmentDef = ((pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything);
            float curLevel = hygiene.CurLevel;
            if (timeAssignmentDef == TimeAssignmentDefOf.Anything)
            {
                if (curLevel < 0.5f)
                {
                    return 7f;
                }
                return 0f;
            }
            else
            {
                if (timeAssignmentDef == TimeAssignmentDefOf.Work)
                {
                    return 0f;
                }
                if (timeAssignmentDef == TimeAssignmentDefOf.Joy)
                {
                    if (curLevel < 0.8f)
                    {
                        return 7f;
                    }
                    return 0f;
                }
                else
                {
                    if (timeAssignmentDef != TimeAssignmentDefOf.Sleep)
                    {
                        throw new NotImplementedException();
                    }
                    if (curLevel < 0.3f)
                    {
                        return 7f;
                    }
                    return 0f;
                }
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (HygieneUtility.DisturbancePreventsWashing(pawn))
            {
                return null;
            }
            Building_Washer building_Washer = HygieneUtility.FindWasherFor(pawn, false);
            if (building_Washer != null)
            {
                return new Job(JobDefOfHygiene.WashSelf, building_Washer);
            }
            return null;
        }

        private HygieneCategory minCategory = HygieneCategory.Clean;
    }
}
