using System;
using RimWorld;
using Verse;

namespace Combat_Realism
{

    public class CompProperties_MilkableRenameable : CompProperties_Milkable
    {

        public string                       growthLabel = "";

        public CompProperties_MilkableRenameable()
        {
            this.compClass = typeof( CompMilkableRenameable );
        }

    }

}

