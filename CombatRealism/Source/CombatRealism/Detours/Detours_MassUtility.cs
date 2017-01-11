using RimWorld;
using System;
using Verse;
using UnityEngine;

namespace Combat_Realism
{
    internal static class Detours_MassUtility
    {
        internal static float Capacity(Pawn p)
        {
            if (!MassUtility.CanEverCarryAnything(p))
                return 0f;
            else
                return p.GetStatValue(CR_StatDefOf.CarryWeight);
        }
    }
}
