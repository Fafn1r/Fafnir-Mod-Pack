using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Combat_Realism
{
    [StaticConstructorOnStartup]
    public static class Utility_Loadouts
    {
        #region Fields


        private static float _labelSize = -1f;
        private static float _margin = 6f;
        private static Texture2D _overburdenedTex;

        // Base values for WeightCapacity and BulkCapacity are no longer meaningful,
        // instead the median values are calculated from the colonists themselves
        // via UpdateColonistCapacities
        public static float medianWeightCapacity = 0f;
        public static float medianBulkCapacity = 0f;

        #endregion Fields

        #region Properties

        public static float LabelSize
        {
            get
            {
                if (_labelSize < 0)
                {
                    // get size of label
                    _labelSize = (_margin + Math.Max(Text.CalcSize("CR.Weight".Translate()).x, Text.CalcSize("CR.Bulk".Translate()).x));
                }
                return _labelSize;
            }
        }

        public static Texture2D OverburdenedTex
        {
            get
            {
                if (_overburdenedTex == null)
                    _overburdenedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
                return _overburdenedTex;
            }
        }

        #endregion Properties

        #region Methods

        public static void DrawBar(Rect canvas, float current, float capacity, string label = "", string tooltip = "")
        {
            // rects
            Rect labelRect = new Rect(canvas);
            Rect barRect = new Rect(canvas);
            if (label != "")
                barRect.xMin += LabelSize;
            labelRect.width = LabelSize;

            // label
            if (label != "")
                Widgets.Label(labelRect, label);

            // bar
            bool overburdened = current > capacity;
            float fillPercentage = overburdened ? 1f : current / capacity;
            if (overburdened)
            {
                Widgets.FillableBar(barRect, fillPercentage, OverburdenedTex);
                DrawBarThreshold(barRect, capacity / current, 1f);
            }
            else
                Widgets.FillableBar(barRect, fillPercentage);

            // tooltip
            if (tooltip != "")
                TooltipHandler.TipRegion(canvas, tooltip);
        }

        public static void DrawBarThreshold(Rect barRect, float pct, float curLevel = 1f)
        {
            float thresholdBarWidth = (float)((barRect.width <= 60f) ? 1 : 2);

            Rect position = new Rect(barRect.x + barRect.width * pct - (thresholdBarWidth - 1f), barRect.y + barRect.height / 2f, thresholdBarWidth, barRect.height / 2f);
            Texture2D image;
            if (pct < curLevel)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public static string GetBulkTip(this Loadout loadout)
        {
            float workSpeedFactor = MassBulkUtility.WorkSpeedFactor(loadout.Bulk, medianBulkCapacity);

            return "CR.DetailedBaseBulkTip".Translate(
                CR_StatDefOf.CarryBulk.ValueToString(medianBulkCapacity, CR_StatDefOf.CarryBulk.toStringNumberSense),
                CR_StatDefOf.CarryBulk.ValueToString(loadout.Bulk, CR_StatDefOf.CarryBulk.toStringNumberSense),
                workSpeedFactor.ToStringPercent());
        }

        public static string GetBulkTip(this Pawn pawn)
        {
            CompInventory comp = pawn.TryGetComp<CompInventory>();
            if (comp != null)
                return "CR.DetailedBulkTip".Translate(CR_StatDefOf.CarryBulk.ValueToString(comp.capacityBulk, CR_StatDefOf.CarryBulk.toStringNumberSense), CR_StatDefOf.CarryBulk.ValueToString(comp.currentBulk, CR_StatDefOf.CarryBulk.toStringNumberSense),
                                                       comp.workSpeedFactor.ToStringPercent());
            else
                return String.Empty;
        }

        public static string GetBulkTip(this Thing thing, int count = 1)
        {
            return
                "CR.Bulk".Translate() + ": " + CR_StatDefOf.Bulk.ValueToString(thing.GetStatValue(CR_StatDefOf.Bulk) * count, CR_StatDefOf.Bulk.toStringNumberSense);
        }

        public static string GetBulkTip(this ThingDef def, int count = 1)
        {
            return
                "CR.Bulk".Translate() + ": " + CR_StatDefOf.Bulk.ValueToString(def.GetStatValueAbstract(CR_StatDefOf.Bulk) * count, CR_StatDefOf.Bulk.toStringNumberSense);
        }

        public static Loadout GetLoadout(this Pawn pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException("pawn");

            Loadout loadout;
            if (!LoadoutManager.AssignedLoadouts.TryGetValue(pawn, out loadout))
            {
                LoadoutManager.AssignedLoadouts.Add(pawn, LoadoutManager.DefaultLoadout);
                loadout = LoadoutManager.DefaultLoadout;
            }
            return loadout;
        }

        public static string GetWeightAndBulkTip(this Loadout loadout)
        {
            return loadout.GetWeightTip() + "\n\n" + loadout.GetBulkTip();
        }

        public static string GetWeightAndBulkTip(this Pawn pawn)
        {
            return pawn.GetWeightTip() + "\n\n" + pawn.GetBulkTip();
        }

        public static string GetWeightAndBulkTip(this Thing thing)
        {
            return thing.LabelCap + "\n" + thing.GetWeightTip(thing.stackCount) + "\n" + thing.GetBulkTip(thing.stackCount);
        }

        public static string GetWeightAndBulkTip(this ThingDef def, int count = 1)
        {
            return def.LabelCap +
                (count != 1 ? " x" + count : "") +
                "\n" + def.GetWeightTip(count) + "\n" + def.GetBulkTip(count);
        }

        public static string GetWeightTip(this ThingDef def, int count = 1)
        {
            return
                "CR.Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(def.GetStatValueAbstract(StatDefOf.Mass) * count, StatDefOf.Mass.toStringNumberSense);
            //old "CR.Weight".Translate() + ": " + CR_StatDefOf.Weight.ValueToString(def.GetStatValueAbstract(CR_StatDefOf.Weight) * count, CR_StatDefOf.Weight.toStringNumberSense);
        }

        public static string GetWeightTip(this Thing thing, int count = 1)
        {
            return
                "CR.Weight".Translate() + ": " + StatDefOf.Mass.ValueToString(thing.GetStatValue(StatDefOf.Mass) * count, StatDefOf.Mass.toStringNumberSense);
           //old "CR.Weight".Translate() + ": " + CR_StatDefOf.Weight.ValueToString(thing.GetStatValue(CR_StatDefOf.Weight) * count, CR_StatDefOf.Weight.toStringNumberSense);
        }

        public static string GetWeightTip(this Loadout loadout)
        {
            float moveSpeedFactor = MassBulkUtility.MoveSpeedFactor(loadout.Weight, medianWeightCapacity);
            float encumberPenalty = MassBulkUtility.EncumberPenalty(loadout.Weight, medianWeightCapacity);

            return "CR.DetailedBaseWeightTip".Translate(CR_StatDefOf.CarryWeight.ValueToString(medianWeightCapacity, CR_StatDefOf.CarryWeight.toStringNumberSense), CR_StatDefOf.CarryWeight.ValueToString(loadout.Weight, CR_StatDefOf.CarryWeight.toStringNumberSense),
                                                 moveSpeedFactor.ToStringPercent(),
                                                 encumberPenalty.ToStringPercent());
        }

        public static string GetWeightTip(this Pawn pawn)
        {
            CompInventory comp = pawn.TryGetComp<CompInventory>();
            if (comp != null)
                return "CR.DetailedWeightTip".Translate(CR_StatDefOf.CarryWeight.ValueToString(comp.capacityWeight, CR_StatDefOf.CarryWeight.toStringNumberSense), CR_StatDefOf.CarryWeight.ValueToString(comp.currentWeight, CR_StatDefOf.CarryWeight.toStringNumberSense),
                                                     comp.moveSpeedFactor.ToStringPercent(),
                                                     comp.encumberPenalty.ToStringPercent());
            else
                return "";
        }

        public static void SetLoadout(this Pawn pawn, Loadout loadout)
        {
            if (pawn == null)
                throw new ArgumentNullException("pawn");

            if (LoadoutManager.AssignedLoadouts.ContainsKey(pawn))
                LoadoutManager.AssignedLoadouts[pawn] = loadout;
            else
                LoadoutManager.AssignedLoadouts.Add(pawn, loadout);
        }

        public static void UpdateColonistCapacities()
        {
            Pawn[] colonists = PawnsFinder.AllMaps_FreeColonists.ToArray();

            if (colonists.Length > 0)
            {
                // The median calculation is O(n log n) but it could be made O(n) with a little effort
                // Due to the small number of colonists, and because the values only need to be updated occasionally,
                // this shouldn't be a problem.
                medianWeightCapacity = Median(colonists.Select(c => c.GetStatValue(CR_StatDefOf.CarryWeight)).ToArray());
                medianBulkCapacity = Median(colonists.Select(c => c.GetStatValue(CR_StatDefOf.CarryBulk)).ToArray());
            }
            else
            {
                // Use the base values
                medianWeightCapacity = ThingDefOf.Human.GetStatValueAbstract(CR_StatDefOf.CarryWeight);
                medianBulkCapacity = ThingDefOf.Human.GetStatValueAbstract(CR_StatDefOf.CarryBulk);
            }
        }

        private static float Median(float[] xs)
        {
            var ys = xs.OrderBy(x => x).ToArray();
            if (ys.Length == 0)
            {
                Log.Error("CR :: Utility_Loadouts :: Median: Nonzero-length array");
                return 0;
            }
            else if (ys.Length % 2 == 0)
                return (ys[(int)(ys.Length / 2) - 1] + ys[(int)(ys.Length / 2)])/2;
            else
                return ys[Mathf.FloorToInt(ys.Length / 2)];
        }


        #endregion Methods
    }
}
