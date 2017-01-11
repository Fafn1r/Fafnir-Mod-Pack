using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Source.Detour;
using System.Reflection;

namespace Hygiene.Detour
{
    internal static class _Pawn_FilthTracker
    {
        internal static FieldInfo _carriedFilth;
        internal static FieldInfo _pawn;

        internal static List<Filth> CarriedFilth(this Pawn_FilthTracker _this)
        {
            if (_carriedFilth == null)
            {
                _carriedFilth = typeof(Pawn_FilthTracker).GetField("carriedFilth", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_carriedFilth == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_FilthTracker.carriedFilth!", 305432421);
                }
            }
            return (List<Filth>)_carriedFilth.GetValue(_this);
        }

        internal static Pawn GetPawn(this Pawn_FilthTracker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Pawn_FilthTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_FilthTracker.pawn!", 305432421);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        [DetourMethod(typeof(Pawn_FilthTracker),"TryPickupFilth")]
        internal static void _TryPickupFilth(this Pawn_FilthTracker _this)
        {
            if (_this.GetPawn().needs.hygiene() != null && Rand.Value <= Mathf.InverseLerp(0.9f, 0f, _this.GetPawn().needs.hygiene().CurLevel))
                return;
            var ThinCarriedFilth = typeof(Pawn_FilthTracker).GetMethod("ThinCarriedFilth", BindingFlags.Instance | BindingFlags.NonPublic);
            TerrainDef terrDef = _this.GetPawn().Map.terrainGrid.TerrainAt(_this.GetPawn().Position);
            if (terrDef.terrainFilthDef != null)
            {
                for (int i = _this.CarriedFilth().Count - 1; i >= 0; i--)
                {
                    if (_this.CarriedFilth()[i].def.filth.terrainSourced && _this.CarriedFilth()[i].def != terrDef.terrainFilthDef)
                    {
                        ThinCarriedFilth.Invoke(_this, new object[] { _this.CarriedFilth()[i] });
                    }
                }
                Filth filth = (from f in _this.CarriedFilth()
                               where f.def == terrDef.terrainFilthDef
                               select f).FirstOrDefault<Filth>();
                if (filth == null || filth.thickness < 1)
                {
                    _this.GainFilth(terrDef.terrainFilthDef);
                }
            }
            List<Thing> thingList = _this.GetPawn().Position.GetThingList(_this.GetPawn().Map);
            for (int j = thingList.Count - 1; j >= 0; j--)
            {
                Filth filth2 = thingList[j] as Filth;
                if (filth2 != null && filth2.CanFilthAttachNow)
                {
                    _this.GainFilth(filth2.def, filth2.sources);
                    filth2.ThinFilth();
                }
            }
        }
    }
}
