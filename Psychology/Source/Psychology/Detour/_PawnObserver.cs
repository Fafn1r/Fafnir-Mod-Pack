﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;

namespace Psychology.Detour
{
    internal static class _PawnObserver
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this PawnObserver _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(PawnObserver).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnObserver.pawn!", 305432421);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        // Token: 0x06000DC4 RID: 3524 RVA: 0x000456A8 File Offset: 0x000438A8
        [DetourMethod(typeof(PawnObserver), "ObserveSurroundingThings")]
        internal static void _ObserveSurroundingThings(this PawnObserver _this)
        {
            Pawn pawn = _this.GetPawn();
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                return;
            }
            Room room = RoomQuery.RoomAt(pawn);
            int num = 0;
            while ((float)num < 100f)
            {
                IntVec3 c = pawn.Position + GenRadial.RadialPattern[num];
                if (c.InBounds(pawn.Map))
                {
                    if (RoomQuery.RoomAt(c, pawn.Map) == room)
                    {
                        List<Thing> thingList = c.GetThingList(pawn.Map);
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            IThoughtGiver thoughtGiver = thingList[i] as IThoughtGiver;
                            if (thoughtGiver != null)
                            {
                                Thought_Memory thought_Memory = thoughtGiver.GiveObservedThought();
                                if (thought_Memory != null)
                                {
                                    if (thought_Memory.def == ThoughtDefOf.ObservedLayingCorpse)
                                    {
                                        if (!pawn.story.traits.HasTrait(TraitDefOfPsychology.BleedingHeart) && !pawn.story.traits.HasTrait(TraitDefOf.Psychopath) && !pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Desensitized))
                                        {
                                            if (((pawn.GetHashCode() ^ (GenLocalDate.DayOfYear(pawn) + GenLocalDate.Year(pawn) + (int)(GenLocalDate.DayPercent(pawn) * 5) * 60) * 391) % 1000) == 0)
                                            {
                                                pawn.story.traits.GainTrait(new Trait(TraitDefOfPsychology.Desensitized));
                                                pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.RecentlyDesensitized, null);
                                            }
                                        }
                                    }
                                    pawn.needs.mood.thoughts.memories.TryGainMemoryThought(thought_Memory, null);
                                }
                            }
                        }
                    }
                }
                num++;
            }
        }
    }
}
