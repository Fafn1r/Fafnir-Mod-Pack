﻿using System;
using Verse;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_RandomMoodBipolar : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            switch ((p.GetHashCode() ^ (GenLocalDate.DayOfYear(p) + GenLocalDate.Year(p) + (int)(GenLocalDate.DayPercent(p) * 10) * 60) * 391) % 12)
            {
                case 0:
                    return ThoughtState.ActiveAtStage(0);
                case 1:
                    return ThoughtState.ActiveAtStage(1);
                case 2:
                    return ThoughtState.ActiveAtStage(1);
                case 3:
                    return ThoughtState.ActiveAtStage(2);
                case 4:
                    return ThoughtState.ActiveAtStage(2);
                case 5:
                    return ThoughtState.Inactive;
                case 6:
                    return ThoughtState.Inactive;
                case 7:
                    return ThoughtState.ActiveAtStage(3);
                case 8:
                    return ThoughtState.ActiveAtStage(3);
                case 9:
                    return ThoughtState.ActiveAtStage(4);
                case 10:
                    return ThoughtState.ActiveAtStage(4);
                case 11:
                    return ThoughtState.ActiveAtStage(5);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
