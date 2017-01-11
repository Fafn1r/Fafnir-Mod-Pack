using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Reflection;

namespace Hygiene
{
    public static class Toils_WashOther
    {

        public static Toil Wash(TargetIndex batheeIndex)
        {
            Toil wash = new Toil();
            wash.initAction = delegate
            {
                Pawn patient = (Pawn)wash.actor.CurJob.GetTarget(batheeIndex).Thing;
                wash.actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt((1f - patient.needs.hygiene().CurLevelPercentage) / (0.00035f * WashingEffectiveness));
            };
            wash.tickAction = delegate
            {
                Pawn actor = wash.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Pawn patient = (Pawn)curJob.GetTarget(batheeIndex).Thing;
                actor.GainComfortFromCellIfPossible();
                patient.needs.hygiene().TickWashing(WashingEffectiveness);
                if (patient.IsHashIntervalTick(TicksBetweenBubbles) && !patient.Position.Fogged(actor.Map))
                {
                    MoteMaker.ThrowMetaIcon(patient.Position, patient.Map, ThingDefOfHygiene.Mote_WashingBubble);
                }
                if (patient.needs.mood != null)
                {
                    patient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfHygiene.Bathed, null);
                }
            };
            wash.WithProgressBar(batheeIndex, delegate
            {
                Pawn actor = wash.actor;
                Job curJob = actor.CurJob;
                Pawn patient = (Pawn)curJob.GetTarget(batheeIndex).Thing;
                if (patient == null)
                {
                    return 1f;
                }
                return patient.needs.hygiene().CurLevelPercentage;
            }, false, -0.5f);
            wash.defaultCompleteMode = ToilCompleteMode.Delay;
            wash.FailOnDespawnedOrNull(batheeIndex);
            wash.FailOn(() => ((Pawn)wash.actor.CurJob.GetTarget(batheeIndex).Thing).needs.hygiene() == null);
            wash.FailOn(() => !FeedPatientUtility.ShouldBeFed((Pawn)wash.actor.CurJob.GetTarget(batheeIndex).Thing));
            wash.AddFinishAction(delegate
            {
                Pawn actor = wash.actor;
                Job curJob = actor.CurJob;
                Pawn patient = (Pawn)curJob.GetTarget(batheeIndex).Thing;
                patient.filth.ClearFilth();
            });
            return wash;
        }

        private const int GetUpOrStartJobWhileWashingCheckInterval = 211;
        private const int TicksBetweenBubbles = 50;
        private const float WashingEffectiveness = 0.33f;
    }
}
