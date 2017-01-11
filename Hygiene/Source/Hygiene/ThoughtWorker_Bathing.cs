using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Hygiene
{
    public class ThoughtWorker_Bathing : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return ThoughtState.Inactive;
            if (!p.RaceProps.Humanlike)
                return ThoughtState.Inactive;
            if (p.CurJob == null)
                return ThoughtState.Inactive;
            if (!p.CurJob.targetA.HasThing)
                return ThoughtState.Inactive;
            if (!(p.CurJob.targetA.Thing is Building_Washer))
                return ThoughtState.Inactive;
            if (((Building_Washer)p.CurJob.targetA.Thing).pawn != p)
                return ThoughtState.Inactive;
            TraitDef prude = DefDatabase<TraitDef>.GetNamedSilentFail("Prude");
            if (prude != null & p.story.traits.HasTrait(prude))
            {
                return ThoughtState.ActiveAtStage(1);
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
