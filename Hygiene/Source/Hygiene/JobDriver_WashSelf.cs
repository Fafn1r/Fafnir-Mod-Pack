using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public class JobDriver_WashSelf : JobDriver
    {

        public override string GetReport()
        {
            return "ReportWashing".Translate();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool hasThing = this.pawn.CurJob.GetTarget(TargetIndex.A).HasThing;
            if (hasThing)
            {
                yield return Toils_Reserve.Reserve(TargetIndex.A);
                yield return Toils_Washer.GotoWasher(TargetIndex.A);
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }
            yield return Toils_WashSelf.WashSelf(TargetIndex.A, hasThing, true);
        }
    }
}
