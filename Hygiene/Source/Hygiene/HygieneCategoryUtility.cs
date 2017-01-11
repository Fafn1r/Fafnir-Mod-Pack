using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Hygiene
{
    public static class HygieneCategoryUtility
    {
        // Token: 0x06000FA0 RID: 4000 RVA: 0x00050BB8 File Offset: 0x0004EDB8
        public static string GetLabel(this HygieneCategory fatigue)
        {
            switch (fatigue)
            {
                case HygieneCategory.Clean:
                    return "HungerLevel_Clean".Translate();
                case HygieneCategory.Dirty:
                    return "HungerLevel_Dirty".Translate();
                case HygieneCategory.VeryDirty:
                    return "HungerLevel_VeryDirty".Translate();
                case HygieneCategory.Filthy:
                    return "HungerLevel_Filthy".Translate();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
