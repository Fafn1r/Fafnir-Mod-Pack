using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public class JobDriver_BathePatient : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(BatheeInd);
            yield return Toils_Goto.Goto(BatheeInd, PathEndMode.Touch);
            yield return Toils_WashOther.Wash(BatheeInd);
        }

        protected Pawn Bathee
        {
            // Token: 0x060001AB RID: 427 RVA: 0x00007308 File Offset: 0x00005508
            get
            {
                return (Pawn)base.CurJob.targetA.Thing;
            }
        }

        private const TargetIndex BatheeInd = TargetIndex.A;
    }
}
