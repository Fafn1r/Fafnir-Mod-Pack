using System;
using RimWorld;
using Verse;

namespace Combat_Realism
{

    public class CompProperties_ShearableRenameable : CompProperties_Shearable
    {

        public string                       growthLabel = "";

        public CompProperties_ShearableRenameable()
        {
            this.compClass = typeof( CompShearableRenameable );
        }

    }

}

