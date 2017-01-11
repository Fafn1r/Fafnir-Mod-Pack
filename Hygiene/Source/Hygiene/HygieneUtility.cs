using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public static class HygieneUtility
    {
        public static Need_Hygiene hygiene(this Pawn_NeedsTracker needs)
        {
            if (needs != null)
            {
                return needs.TryGetNeed<Need_Hygiene>();
            }
            return null;
        }

        public static bool Clean(this Pawn p)
        {
            return (!p.Spawned || p.needs.hygiene() == null || p.needs.hygiene().CurCategory == HygieneCategory.Clean);
        }

        public static bool DisturbancePreventsWashing(Pawn pawn)
        {
            return Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400;
        }

        public static void Reset()
        {
            HygieneUtility.washerDefsBestToWorst_RestEffectiveness = (from d in DefDatabase<ThingDef>.AllDefs
                                                                      where typeof(Building_Washer).IsAssignableFrom(d.thingClass)
                                                                      orderby d.GetStatValueAbstract(StatDefOfHygiene.BathingEffectiveness, null) descending
                                                                      select d).ToList<ThingDef>();
        }

        public static Building_Washer FindWasherFor(Pawn p, bool washerWillBePrisoner)
        {
            Predicate<Thing> washerValidator = delegate (Thing t)
            {
                Building_Washer building_Washer3 = (Building_Washer)t;
                if (!p.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Some))
                {
                    return false;
                }
                if (building_Washer3.GetCurOccupant() != null && (p.CurJob == null || !p.jobs.curDriver.layingDown || building_Washer3.pawn != p))
                {
                    return false;
                }
                if (washerWillBePrisoner)
                {
                    if (!building_Washer3.Position.IsInPrisonCell(building_Washer3.Map))
                    {
                        return false;
                    }
                }
                else
                {
                    if (building_Washer3.Faction != p.Faction)
                    {
                        return false;
                    }
                    if (building_Washer3.Position.IsInPrisonCell(building_Washer3.Map))
                    {
                        return false;
                    }
                    if (building_Washer3.Position.GetRoom(p.Map).Owners != null && building_Washer3.Position.GetRoom(p.Map).Owners.Count() > 0 && !building_Washer3.Position.GetRoom(p.Map).Owners.Contains(p))
                    {
                        return false;
                    }
                }
                if (building_Washer3.Faction != p.Faction)
                {
                    return false;
                }
                if(building_Washer3.PowerComp != null && !building_Washer3.powerComp.PowerOn)
                {
                    return false;
                }
                return (!building_Washer3.IsForbidden(p) && !building_Washer3.IsBurning());
            };
            for (int j = 0; j < HygieneUtility.washerDefsBestToWorst_RestEffectiveness.Count; j++)
            {
                ThingDef thingDef = HygieneUtility.washerDefsBestToWorst_RestEffectiveness[j];
                Predicate<Thing> validator = (Thing b) => washerValidator(b);
                Building_Washer building_Washer2 = (Building_Washer)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
                if (building_Washer2 != null)
                {
                    return building_Washer2;
                }
            }
            return null;
        }

        private static List<ThingDef> washerDefsBestToWorst_RestEffectiveness;
    }
}
