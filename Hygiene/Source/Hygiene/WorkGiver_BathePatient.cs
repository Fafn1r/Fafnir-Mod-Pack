using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public class WorkGiver_BathePatient : WorkGiver_Scanner
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2 == pawn)
            {
                return false;
            }
            if (pawn2.needs.hygiene() == null || pawn2.needs.hygiene().CurLevelPercentage > 0.45f)
            {
                return false;
            }
            if (!FeedPatientUtility.ShouldBeFed(pawn2))
            {
                return false;
            }
            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1))
            {
                return false;
            }
            return true;
        }
        
        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            Pawn pawn2 = (Pawn)t;
            return new Job(JobDefOfHygiene.BathePatient, pawn2);
        }
        
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }
        
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }
    }
}
