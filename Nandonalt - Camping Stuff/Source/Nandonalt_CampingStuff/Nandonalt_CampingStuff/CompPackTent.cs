using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace Nandonalt_CampingStuff
{
    public class CompPackTent : CompUsable
    {

        public String tentName = "DeployableTent";
        public Rot4 placingRot = Rot4.South;

        protected CompPropTent Props
        {
            get
            {
                return (CompPropTent)this.props;
            }
        }

        protected virtual string FloatMenuOptionLabel
        {
            get
            {
                return this.Props.useLabel;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (!myPawn.CanReserve(this.parent, 1))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(this.FloatMenuOptionLabel, delegate
                {
                    if (myPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1))
                    {
                        foreach (CompUseEffect current in this.parent.GetComps<CompUseEffect>())
                        {
                            if (current.SelectedUseOption(myPawn))
                            {
                                return;
                            }
                        }
                        this.TryStartUseJob(myPawn);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<String>(ref this.tentName, "tentName", "DeployableTent");
            Scribe_Values.LookValue<Rot4>(ref this.placingRot, "placingRot", Rot4.South);
        }

        public void TryStartUseJob(Pawn user)
        {
            if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1))
            {
                return;
            }
            Job job = new Job(this.Props.useJob, this.parent);
            user.jobs.TryTakeOrderedJob(job);
        }

        public void UsedBy(Pawn p)
        {
            if(this.parent.Stuff == null)
            {
                this.parent.SetStuffDirect(ThingDefOf.Cloth);
            }
            IntVec3 pos = this.parent.Position;
            CellRect cellRect = new CellRect(pos.x - 2, pos.z - 2, 5, 4);
            int damage = 0;

            this.parent.Destroy(DestroyMode.Vanish);
            Thing tent = ThingMaker.MakeThing(ThingDef.Named(tentName), this.parent.Stuff);
            CompTargetable_Tent tentComp;
            tentComp = tent.TryGetComp<CompTargetable_Tent>();
            tentComp.placingRot = this.placingRot;
            tentComp.getPlacements(pos);
          
        
            foreach (IntVec3 current in tentComp.wallCells)
            {
                if (p.Map.roofGrid.RoofAt(current) == RoofDefOf.RoofConstructed)
                {
                    p.Map.roofGrid.SetRoof(current, null);
                }
            }
            foreach (IntVec3 current in tentComp.doorCells)
            {
                if (p.Map.roofGrid.RoofAt(current) == RoofDefOf.RoofConstructed)
                {
                    p.Map.roofGrid.SetRoof(current, null);
                }
            }
            foreach (IntVec3 current in tentComp.roofCells)
            {
                if (p.Map.roofGrid.RoofAt(current) == RoofDefOf.RoofConstructed)
                {
                    p.Map.roofGrid.SetRoof(current, null);
                }
            }
               if (p.Map.roofGrid.RoofAt(tentComp.supportCell) == RoofDefOf.RoofConstructed)
            {
                p.Map.roofGrid.SetRoof(tentComp.supportCell, null);
            }

            foreach (IntVec3 current in tentComp.wallCells)
            {
                List<Thing> thingList = current.GetThingList(p.Map);
                bool nowall = true;
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (thingList[i].def == ThingDef.Named("TentWall"))
                    {
                        thingList[i].Destroy(DestroyMode.Vanish);
                        nowall = false;
                    }
                
                }
                if (nowall)
                {
                    damage = damage + 10;
                }
            }

            
       
            foreach (IntVec3 current in tentComp.doorCells)
            {
                IntVec3 DoorPos = current;
                List<Thing> thingList2 = DoorPos.GetThingList(p.Map);
                bool nodoor = true;
                for (int i = 0; i < thingList2.Count; i++)
                {

                    if (thingList2[i].def == ThingDef.Named("TentDoor"))
                    {
                        CellRect rect = thingList2[i].OccupiedRect();
                        bool regionBarrier = thingList2[i].def.regionBarrier;
                        if (regionBarrier)
                        {
                            this.SpawnTemporaryRegionBarriers(rect, p);
                        }
                        thingList2[i].Destroy(DestroyMode.Vanish);
                        nodoor = false;
                        if (regionBarrier)
                        {
                            this.DestroyTemporaryRegionBarriers(rect, p.Map);
                        }
                    }
                }
                if (nodoor)
                {
                    damage = damage + 10;
                }
            }

            tent.HitPoints = tent.HitPoints - damage;
            GenSpawn.Spawn(tent, pos, p.Map);

            return;
        }

        private void SpawnTemporaryRegionBarriers(CellRect rect, Pawn pawn)
        {
            CellRect.CellRectIterator iterator = rect.GetIterator();
            while (!iterator.Done())
            {
                GenSpawn.Spawn(ThingDefOf.TemporaryRegionBarrier, iterator.Current, pawn.Map);
                iterator.MoveNext();
            }
        }
        private void DestroyTemporaryRegionBarriers(CellRect rect, Map map)
        {
            CellRect.CellRectIterator iterator = rect.GetIterator();
            while (!iterator.Done())
            {
                Thing thing = iterator.Current.GetThingList(map).Find((Thing x) => x.def.temporaryRegionBarrier);
                if (thing == null)
                {
                    Log.Warning("Could not destroy temporary region barrier at " + iterator.Current + " because it's not here.");
                }
                else
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
                iterator.MoveNext();
            }
        }


    }
}
