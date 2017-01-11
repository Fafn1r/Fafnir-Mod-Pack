using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public static class Toils_Washer
    {
        public static void FailOnWasherNoLongerUsable(this Toil toil, TargetIndex washIndex)
        {
            toil.FailOnDespawnedOrNull(washIndex);
            toil.FailOn(() => ((Building_Washer)toil.actor.CurJob.GetTarget(washIndex).Thing).IsBurning());
            toil.FailOn(() => toil.actor.IsColonist && !toil.actor.CurJob.ignoreForbidden && !toil.actor.Downed && toil.actor.CurJob.GetTarget(washIndex).Thing.IsForbidden(toil.actor));
        }

        public static Toil GotoWasher(TargetIndex washIndex)
        {
            Toil gotoWasher = new Toil();
            gotoWasher.initAction = delegate
            {
                Pawn actor = gotoWasher.actor;
                Building_Washer washer = (Building_Washer)actor.CurJob.GetTarget(washIndex).Thing;
                IntVec3 washPosFor = washer.GetWashingSlotPos();
                if (actor.Position == washPosFor)
                {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
                else
                {
                    actor.pather.StartPath(washer.GetWashingSlotPos(), PathEndMode.OnCell);
                }
            };
            gotoWasher.tickAction = delegate
            {
                Pawn actor = gotoWasher.actor;
                Building_Washer building_Washer = (Building_Washer)actor.CurJob.GetTarget(washIndex).Thing;
                Pawn curOccupantAt = building_Washer.GetCurOccupantAt(actor.pather.Destination.Cell);
                if (curOccupantAt != null && curOccupantAt != actor)
                {
                    actor.pather.StartPath(building_Washer.GetWashingSlotPos(), PathEndMode.OnCell);
                }
            };
            gotoWasher.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoWasher.FailOnWasherNoLongerUsable(washIndex);
            return gotoWasher;
        }
    }
}
