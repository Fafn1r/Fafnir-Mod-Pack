using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Hygiene.Detour
{
    internal static class _PawnRenderer
    {
        internal static FieldInfo _pawn;
        internal static Pawn GetPawn(this PawnRenderer _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(PawnRenderer).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect PawnRenderer.pawn!", 305432421);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        [DetourMethod(typeof(PawnRenderer),"LayingFacing")]
        internal static Rot4 _LayingFacing(this PawnRenderer _this)
        {
            if (_this.GetPawn().CurJob != null && _this.GetPawn().CurJob.targetA.HasThing && _this.GetPawn().CurJob.targetA.Thing.def == ThingDefOfHygiene.Bathtub)
            {
                return _this.GetPawn().CurJob.targetA.Thing.Rotation;
            }
            if (_this.GetPawn().GetPosture() == PawnPosture.LayingFaceUp)
            {
                return Rot4.South;
            }
            if (_this.GetPawn().RaceProps.Humanlike)
            {
                switch (_this.GetPawn().thingIDNumber % 4)
                {
                    case 0:
                        return Rot4.South;
                    case 1:
                        return Rot4.South;
                    case 2:
                        return Rot4.East;
                    case 3:
                        return Rot4.West;
                }
            }
            else
            {
                switch (_this.GetPawn().thingIDNumber % 4)
                {
                    case 0:
                        return Rot4.South;
                    case 1:
                        return Rot4.East;
                    case 2:
                        return Rot4.West;
                    case 3:
                        return Rot4.West;
                }
            }
            return Rot4.Random;
        }
    }
}
