using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Hygiene
{
    public class StatPart_Hygiene : StatPart
    {
        // Token: 0x06002681 RID: 9857 RVA: 0x000D8A84 File Offset: 0x000D6C84
        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.needs.TryGetNeed<Need_Hygiene>() != null)
                {
                    return pawn.needs.hygiene().CurCategory.GetLabel() + ": x" + this.HygieneMultiplier(pawn.needs.hygiene().CurCategory).ToStringPercent();
                }
            }
            return null;
        }

        // Token: 0x06002682 RID: 9858 RVA: 0x000D8AFC File Offset: 0x000D6CFC
        private float HygieneMultiplier(HygieneCategory hygiene)
        {
            switch (hygiene)
            {
                case HygieneCategory.Clean:
                    return this.factorClean;
                case HygieneCategory.Dirty:
                    return this.factorDirty;
                case HygieneCategory.VeryDirty:
                    return this.factorVeryDirty;
                case HygieneCategory.Filthy:
                    return this.factorFilthy;
                default:
                    throw new InvalidOperationException();
            }
        }

        // Token: 0x06002680 RID: 9856 RVA: 0x000D8A2C File Offset: 0x000D6C2C
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                Pawn pawn = req.Thing as Pawn;
                if (pawn != null && pawn.needs.TryGetNeed<Need_Hygiene>() != null)
                {
                    val *= this.HygieneMultiplier(pawn.needs.TryGetNeed<Need_Hygiene>().CurCategory);
                }
            }
        }

        // Token: 0x04001922 RID: 6434
        private float factorFilthy = 1f;

        // Token: 0x04001925 RID: 6437
        private float factorClean = 1f;

        // Token: 0x04001924 RID: 6436
        private float factorDirty = 1f;

        // Token: 0x04001923 RID: 6435
        private float factorVeryDirty = 1f;
    }
}
