using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Hygiene
{
    public class ThoughtWorker_NeedHygiene : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.hygiene() == null)
            {
                return ThoughtState.Inactive;
            }
            switch (p.needs.hygiene().CurCategory)
            {
                case HygieneCategory.Clean:
                    if(p.needs.hygiene().CurLevel >= 0.95f)
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                    return ThoughtState.Inactive;
                case HygieneCategory.Dirty:
                    return ThoughtState.ActiveAtStage(1);
                case HygieneCategory.VeryDirty:
                    return ThoughtState.ActiveAtStage(2);
                case HygieneCategory.Filthy:
                    return ThoughtState.ActiveAtStage(3);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
