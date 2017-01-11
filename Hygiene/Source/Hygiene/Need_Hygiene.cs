using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Hygiene
{
    public class Need_Hygiene : Need
    {
        public Need_Hygiene(Pawn pawn) : base(pawn)
		{
            this.threshPercents = new List<float>();
            this.threshPercents.Add(ThreshDirty);
            this.threshPercents.Add(ThreshVeryDirty);
        }

        public override void NeedInterval()
        {
            if (this.Washing)
            {
                this.CurLevel += BaseHygieneGainPerTick * 150f * this.lastWashEffectiveness;
            }
            else
            {
                this.CurLevel -= this.HygieneFallPerTick * 150f;
            }
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = Rand.Range(0.8f, 0.95f);
        }

        public void TickWashing(float washEffectiveness)
        {
            this.lastWashTick = Find.TickManager.TicksGame;
            this.lastWashEffectiveness = washEffectiveness;
        }

        // Token: 0x17000213 RID: 531
        public HygieneCategory CurCategory
        {
            // Token: 0x06000FA2 RID: 4002 RVA: 0x00050C6C File Offset: 0x0004EE6C
            get
            {
                if (this.CurLevel < 0.01f)
                {
                    return HygieneCategory.Filthy;
                }
                if (this.CurLevel < ThreshVeryDirty)
                {
                    return HygieneCategory.VeryDirty;
                }
                if (this.CurLevel < ThreshDirty)
                {
                    return HygieneCategory.Dirty;
                }
                return HygieneCategory.Clean;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (this.Washing)
                {
                    return 1;
                }
                return -1;
            }
        }

        private float HygieneFallFactor
        {
            get
            {
                if(this.pawn.Awake())
                {
                    return 1f;
                }
                return 0.5f;
            }
        }

        public float HygieneFallPerTick
        {
            get
            {
                switch (this.CurCategory)
                {
                    case HygieneCategory.Clean:
                        return BaseHygieneFallPerTick * this.HygieneFallFactor;
                    case HygieneCategory.Dirty:
                        return BaseHygieneFallPerTick * this.HygieneFallFactor * 0.7f;
                    case HygieneCategory.VeryDirty:
                        return BaseHygieneFallPerTick * this.HygieneFallFactor * 0.3f;
                    case HygieneCategory.Filthy:
                        return BaseHygieneFallPerTick * this.HygieneFallFactor * 0.6f;
                    default:
                        return 999f;
                }
            }
        }

        private bool Washing
        {
            // Token: 0x06000FA7 RID: 4007 RVA: 0x00050DA4 File Offset: 0x0004EFA4
            get
            {
                return Find.TickManager.TicksGame < this.lastWashTick + 2;
            }
        }

        public const float BaseHygieneGainPerTick = 0.00035f;
        private const float BaseHygieneFallPerTick = 1.58333332E-05f;

        private float lastWashEffectiveness = 1f;
        private int lastWashTick = -999;

        public const float NaturalWashThreshold = 1f;
        public const float ThreshDirty = 0.43f;
        public const float ThreshVeryDirty = 0.215f;
    }
}
