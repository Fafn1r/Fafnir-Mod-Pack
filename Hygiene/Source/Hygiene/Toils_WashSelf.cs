using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace Hygiene
{
    public static class Toils_WashSelf
    {
        internal static FieldInfo _carriedFilth;

        internal static void ClearFilth(this Pawn_FilthTracker _this)
        {
            if (_carriedFilth == null)
            {
                _carriedFilth = typeof(Pawn_FilthTracker).GetField("carriedFilth", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_carriedFilth == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_FilthTracker.carriedFilth!", 305432421);
                }
            }
            _carriedFilth.SetValue(_this, new List<Filth>());
        }

        private static bool AlreadyQueued(Apparel a, List<Apparel> queue)
        {
            foreach(Apparel b in queue)
            {
                if (!ApparelUtility.CanWearTogether(a.def, b.def))
                    return true;
            }
            return false;
        }

        public static Toil WashSelf(TargetIndex washSpotIndex, bool hasWasher, bool lookForOtherJobs, bool canWash = true, bool gainHygiene = true)
        {
            Toil wash = new Toil();
            List<Thing> clothes = new List<Thing>();
            wash.initAction = delegate
            {
                wash.actor.pather.StopDead();
                wash.actor.apparel.DropAll(wash.actor.Position, false);
                CellRect cellRect = CellRect.CenteredOn(wash.actor.Position, 2);
                foreach(IntVec3 cell in cellRect.Cells)
                {
                    if (cell.InBounds(wash.actor.Map))
                    {
                        Thing haulable = cell.GetFirstItem(wash.actor.Map);
                        if (haulable != null && haulable is Apparel)
                        {
                            if(wash.actor.CanReserve(haulable))
                            {
                                wash.actor.Reserve(haulable);
                                clothes.Add(haulable);
                            }
                        }
                    }
                }
                JobDriver curDriver = wash.actor.jobs.curDriver;
                if(((Building_Washer)wash.actor.CurJob.GetTarget(washSpotIndex).Thing).def != ThingDefOfHygiene.Shower)
                {
                    curDriver.layingDown = true;
                }
                ((Building_Washer)wash.actor.CurJob.GetTarget(washSpotIndex).Thing).pawn = wash.actor;
            };
            wash.tickAction = delegate
            {
                Pawn actor = wash.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Building_Washer building_Washer = (Building_Washer)curJob.GetTarget(washSpotIndex).Thing;
                wash.actor.Drawer.rotator.FaceCell(building_Washer.Position);
                actor.GainComfortFromCellIfPossible();
                if (gainHygiene)
                {
                    float num = 0f;
                    if (building_Washer != null && building_Washer.def.statBases.StatListContains(StatDefOfHygiene.BathingEffectiveness))
                    {
                        num = building_Washer.GetStatValue(StatDefOfHygiene.BathingEffectiveness, true);
                    }
                    actor.needs.hygiene().TickWashing(num);
                }
                if (actor.IsHashIntervalTick(TicksBetweenBubbles) && !actor.Position.Fogged(actor.Map))
                {
                    MoteMaker.ThrowMetaIcon(actor.Position, actor.Map, ThingDefOfHygiene.Mote_WashingBubble);
                }
                if (lookForOtherJobs && actor.needs.hygiene().CurLevel >= 1f && actor.IsHashIntervalTick(GetUpOrStartJobWhileInWasherCheckInterval))
                {
                    List<Apparel> queuedAlready = new List<Apparel>();
                    actor.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame;
                    bool putBackOn = (new JobGiver_OptimizeApparel()).TryIssueJobPackage(actor) == ThinkResult.NoJob;
                    foreach (Thing c in clothes)
                    {
                        actor.Map.reservationManager.Release(c, actor);
                        if (putBackOn && !AlreadyQueued((Apparel)c, queuedAlready) && actor.CanReserveAndReach(c, PathEndMode.Touch, actor.NormalMaxDanger()))
                        {
                            queuedAlready.Add((Apparel)c);
                            actor.jobs.jobQueue.EnqueueFirst(new Job(JobDefOf.Wear, c));
                        }
                    }
                    actor.jobs.CheckForJobOverride();
                    return;
                }
                if (actor.needs.mood != null)
                {
                    if (RoomQuery.RoomAt(actor).TouchesMapEdge && building_Washer.def == ThingDefOfHygiene.Shower)
                    {
                        actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfHygiene.OutsideBath, null);
                    }
                    if (GenTemperature.GetTemperatureForCell(actor.Position, actor.Map) < actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null))
                    {
                        actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfHygiene.ColdBath, null);
                    }
                }
            };
            wash.defaultCompleteMode = ToilCompleteMode.Never;
            if (hasWasher)
            {
                wash.FailOnWasherNoLongerUsable(washSpotIndex);
            }
            wash.AddFinishAction(delegate
            {
                Pawn actor = wash.actor;
                actor.filth.ClearFilth();
                JobDriver curDriver = actor.jobs.curDriver;
                if (!actor.Dead && actor.needs.hygiene() != null && actor.needs.hygiene().CurLevel < 0.9f && actor.needs.mood != null)
                {
                    actor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfHygiene.BathingDisturbed, null);
                }
                curDriver.layingDown = false;
                ((Building_Washer)actor.CurJob.GetTarget(washSpotIndex).Thing).pawn = null;
            });
            return wash;
        }

        private const int GetUpOrStartJobWhileInWasherCheckInterval = 211;
        private const int TicksBetweenBubbles = 50;
    }
}
