using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Psychology
{
    public class LordJob_StealHidden : LordJob
    {
        public override StateGraph CreateGraph()
        {
            return new StateGraph();
        }

        public override bool CanOpenAnyDoor(Pawn p)
        {
            return true;
        }
    }
}
