using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Hygiene
{
    public class Building_Washer : Building, IThoughtGiver
    {
        public override void DeSpawn()
        {
            Room room = this.GetRoom();
            base.DeSpawn();
            if (room != null)
            {
                room.RoomChanged();
            }
        }

        private void FacilityChanged()
        {
            CompFacility compFacility = this.TryGetComp<CompFacility>();
            CompAffectedByFacilities compAffectedByFacilities = this.TryGetComp<CompAffectedByFacilities>();
            if (compFacility != null)
            {
                compFacility.Notify_ThingChanged();
            }
            if (compAffectedByFacilities != null)
            {
                compAffectedByFacilities.Notify_ThingChanged();
            }
        }

        public Pawn GetCurOccupant()
        {
            if (!base.Spawned)
            {
                return null;
            }
            IntVec3 washingSlotPos = this.GetWashingSlotPos();
            List<Thing> list = base.Map.thingGrid.ThingsListAt(washingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i] as Pawn;
                if (pawn != null)
                {
                    if (pawn.CurJob != null)
                    {
                        if (pawn.jobs.curDriver.layingDown && !pawn.jobs.curDriver.asleep && this.pawn == pawn)
                        {
                            return pawn;
                        }
                    }
                }
            }
            return null;
        }

        public Pawn GetCurOccupantAt(IntVec3 pos)
        {
            if (this.GetWashingSlotPos() == pos)
            {
                return this.GetCurOccupant();
            }
            return null;
        }

        public IntVec3 GetWashingSlotPos()
        {
            CellRect cellRect = this.OccupiedRect();
            if (base.Rotation == Rot4.North)
            {
                return new IntVec3(cellRect.minX, base.Position.y, cellRect.minZ);
            }
            if (base.Rotation == Rot4.East)
            {
                return new IntVec3(cellRect.minX, base.Position.y, cellRect.maxZ);
            }
            if (base.Rotation == Rot4.South)
            {
                return new IntVec3(cellRect.minX, base.Position.y, cellRect.maxZ);
            }
            return new IntVec3(cellRect.maxX, base.Position.y, cellRect.maxZ);
        }

        public Thought_Memory GiveObservedThought()
        {
            if(GetCurOccupant() != null && GetCurOccupant().health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                GetCurOccupant().needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfHygiene.SeenBathing, null);
            }
            return null;
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }

        public Pawn pawn;
        public CompPowerTrader powerComp;
    }
}
