using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Combat_Realism
{
    public class StatWorker_CarryBulk : StatWorker
    {
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            float value = base.GetValueUnfinalized(req, applyPostProcess);
            Pawn p = req.Thing as Pawn;
            if (p != null)
            {
                value += MassBulkUtility.BaseCarryBulk(p);
            }
            return value;
        }
    }
}
