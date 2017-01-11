using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;
using UnityEngine;
using System.Collections.Generic;

namespace Combat_Realism
{
    public class JobGiver_TakeAndEquip : ThinkNode_JobGiver
    {
        private CompInventory inventory;
        private CompAmmoUser ammouser;

        private enum WorkPriority
        {
            None,
            Unloading,
            LowAmmo,
            Weapon,
            Ammo,
            Apparel
        }

        private WorkPriority GetPriorityWork(Pawn pawn)
        {
            if (pawn.TryGetComp<CompInventory>() != null
                && ((pawn.TryGetComp<CompInventory>().capacityWeight / 2 <= pawn.TryGetComp<CompInventory>().currentWeight)
                || (pawn.TryGetComp<CompInventory>().capacityBulk / 2 <= pawn.TryGetComp<CompInventory>().currentBulk)))
            {
                return WorkPriority.Unloading;
            }

            if (!pawn.Faction.IsPlayer && pawn.equipment.Primary == null)
            {
                return WorkPriority.Weapon;
            }

            if (pawn.equipment.Primary != null && pawn.equipment.Primary.TryGetComp<CompAmmoUser>() != null)
            {
                int ammocount = 0;
                foreach (ThingDef ammoDef in pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
                {
                    Thing ammoThing;
                    ammoThing = pawn.TryGetComp<CompInventory>().ammoList.Find(thing => thing.def == ammoDef);
                    if (ammoThing != null)
                    {
                        ammocount += ammoThing.stackCount;
                    }
                }
                if (ammocount < pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.magazineSize)
                {
                    return WorkPriority.LowAmmo;
                }
                if (ammocount < pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.magazineSize * 3)
                {
                    return WorkPriority.Ammo;
                }
            }

            if (!pawn.Faction.IsPlayer && pawn.equipment.Primary != null
                && PawnUtility.EnemiesAreNearby(pawn, 30, true)
                || (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso)
                || !pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Legs)))
            {
                return WorkPriority.Apparel;
            }

            else return WorkPriority.None;
        }

        public override float GetPriority(Pawn pawn)
        {

            if (pawn.Faction.IsPlayer && pawn.equipment.Primary != null && pawn.equipment.Primary.TryGetComp<CompAmmoUser>() != null)
            {
                Loadout loadout = pawn.GetLoadout();
                if (loadout != null && loadout.SlotCount > 0)
                {
                    return 0f;
                }
            }

            var priority = GetPriorityWork(pawn);

            if (priority == WorkPriority.Unloading) return 9.2f;
            else if (priority == WorkPriority.LowAmmo) return 9f;
            else if (priority == WorkPriority.Weapon) return 8f;
            else if (priority == WorkPriority.Ammo) return 6f;
            else if (priority == WorkPriority.Apparel) return 5f;
            else if(priority == WorkPriority.None) return 0f;

            TimeAssignmentDef assignment = (pawn.timetable != null) ? pawn.timetable.CurrentAssignment : TimeAssignmentDefOf.Anything;
            if (assignment == TimeAssignmentDefOf.Sleep) return 0f;

            if (pawn.health == null || pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                return 0f;
            }
            else return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike)
            {
                return null;
            }

            /*
            Log.Message(pawn.ToString() +  " priority:" + (GetPriorityWork(pawn)).ToString());
            Log.Message(pawn.ToString() + " capacityWeight: " + pawn.TryGetComp<CompInventory>().capacityWeight.ToString());
            Log.Message(pawn.ToString() + " currentWeight: " + pawn.TryGetComp<CompInventory>().currentWeight.ToString());
            Log.Message(pawn.ToString() + "capacityBulk: " + pawn.TryGetComp<CompInventory>().capacityBulk.ToString());
            Log.Message(pawn.ToString() + "currentBulk: " + pawn.TryGetComp<CompInventory>().currentBulk.ToString());
            */

            inventory = pawn.TryGetComp<CompInventory>();
            ammouser = pawn.TryGetComp<CompAmmoUser>();
            if (inventory != null)
            {
                Room room = RoomQuery.RoomAtFast(pawn.Position, pawn.Map);

                // Drop excess weapon
                if (!pawn.Faction.IsPlayer && pawn.equipment.Primary != null && GetPriorityWork(pawn) == WorkPriority.Unloading && inventory.rangedWeaponList.Count >= 1)
                {
                    Thing ListGun = pawn.TryGetComp<CompInventory>().rangedWeaponList.Find(thing => pawn.equipment != null && pawn.equipment.Primary != null && thing.TryGetComp<CompAmmoUser>() != null && thing.def != pawn.equipment.Primary.def);
                    if (ListGun != null)
                    {
                        Thing ammoListGun = null;
                        foreach (ThingDef ListGunDef in ListGun.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
                        {
                            if (pawn.TryGetComp<CompInventory>().ammoList.Find(thing => thing.def == ListGunDef) == null)
                            {
                                ammoListGun = ListGun;
                                break;
                            }
                        }
                        if (ammoListGun != null)
                        {
                            Thing droppedWeapon;
                            if (inventory.container.TryDrop(ListGun, pawn.Position, pawn.Map, ThingPlaceMode.Near, ListGun.stackCount, out droppedWeapon))
                            {
                                pawn.jobs.EndCurrentJob(JobCondition.None, true);
                                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, droppedWeapon, 30, true));
                            }
                        }
                    }
                }

                // Find and drop not need ammo from inventory
                if (!pawn.Faction.IsPlayer && inventory.ammoList.Count > 1 && GetPriorityWork(pawn) == WorkPriority.Unloading)
                {
                    Thing WrongammoThing = null;
                    foreach (ThingDef WrongammoDef in pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
                    {
                        WrongammoThing = pawn.TryGetComp<CompInventory>().ammoList.Find(thing => thing.def != WrongammoDef);
                        break;
                    }
                    if (WrongammoThing != null)
                    {
                        Thing InvListGun = pawn.TryGetComp<CompInventory>().rangedWeaponList.Find(thing => pawn.equipment != null && pawn.equipment.Primary != null && thing.TryGetComp<CompAmmoUser>() != null && thing.def != pawn.equipment.Primary.def);
                        if (InvListGun != null)
                        {
                            Thing ammoInvListGun = null;
                            foreach (ThingDef InvListGunDef in InvListGun.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
                            {
                                ammoInvListGun = pawn.TryGetComp<CompInventory>().ammoList.Find(thing => thing.def == InvListGunDef);
                                break;
                            }
                            if (ammoInvListGun != null && ammoInvListGun != WrongammoThing)
                            {
                                Thing droppedThingAmmo;
                                if (inventory.container.TryDrop(ammoInvListGun, pawn.Position, pawn.Map, ThingPlaceMode.Near, ammoInvListGun.stackCount, out droppedThingAmmo))
                                {
                                    pawn.jobs.EndCurrentJob(JobCondition.None, true);
                                    pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, 30, true));
                                }
                            }
                        }
                        else
                        {
                            Thing droppedThing;
                            if (inventory.container.TryDrop(WrongammoThing, pawn.Position, pawn.Map, ThingPlaceMode.Near, WrongammoThing.stackCount, out droppedThing))
                            {
                                pawn.jobs.EndCurrentJob(JobCondition.None, true);
                                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.DropEquipment, 30, true));
                            }
                        }
                    }
                }

                if (!pawn.Faction.IsPlayer && GetPriorityWork(pawn) == WorkPriority.Weapon && pawn.equipment.Primary == null)
                {
                    Predicate<Thing> validatorWS = (Thing w) => w.def.IsWeapon
                    && w.MarketValue > 500 && pawn.CanReserve(w, 1)
                    && pawn.CanReach(w, PathEndMode.Touch, Danger.Deadly, true)
                    && (w.Position.DistanceToSquared(pawn.Position) < 15f || room == RoomQuery.RoomAtFast(w.Position, pawn.Map));
                    List<Thing> weapon = (
                        from w in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                        where validatorWS(w)
                        orderby w.MarketValue - w.Position.DistanceToSquared(pawn.Position) * 2f descending
                        select w
                        ).ToList();

                    foreach (Thing thing in weapon)
                    {
                        List<ThingDef> thingDefAmmoList = (from ThingDef g in thing.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes
                                                           select g).ToList<ThingDef>();
                        Predicate<Thing> validatorA = (Thing t) => t.def.category == ThingCategory.Item
                        && t is AmmoThing && pawn.CanReserve(t, 1)
                        && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true)
                        && (t.Position.DistanceToSquared(pawn.Position) < 20f || room == RoomQuery.RoomAtFast(t.Position, pawn.Map));
                        List<Thing> thingAmmoList = (
                            from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                            where validatorA(t)
                            select t
                            ).ToList();

                        if (thingAmmoList.Count > 0)
                        {
                            foreach (Thing th in thingAmmoList)
                            {
                                foreach (ThingDef thd in thingDefAmmoList)
                                {
                                    if (thd == th.def)
                                    {
                                        int ammothingcount = 0;
                                        foreach (ThingDef ammoDef in thing.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes)
                                        {
                                            ammothingcount += th.stackCount;
                                        }
                                        if (ammothingcount > thing.TryGetComp<CompAmmoUser>().Props.magazineSize * 2)
                                        {
                                            int numToThing = 0;
                                            if (inventory.CanFitInInventory(thing, out numToThing))
                                            {
                                                if (thing.Position == pawn.Position || thing.Position.AdjacentToCardinal(pawn.Position))
                                                {
                                                    return new Job(JobDefOf.Equip, thing)
                                                    {
                                                        checkOverrideOnExpire = true,
                                                        expiryInterval = 100,
                                                        canBash = true,
                                                        locomotionUrgency = LocomotionUrgency.Sprint
                                                    };
                                                }
                                                return GotoForce(pawn, thing, PathEndMode.Touch);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if ((GetPriorityWork(pawn) == WorkPriority.Ammo || GetPriorityWork(pawn) == WorkPriority.LowAmmo) 
                    && pawn.equipment != null && pawn.equipment.Primary != null 
                    && pawn.equipment.Primary.TryGetComp<CompAmmoUser>() != null)
                {
                    List<ThingDef> curAmmoList = (from ThingDef g in pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.ammoSet.ammoTypes
                                                  select g).ToList<ThingDef>();

                    if (curAmmoList.Count > 0 && (GetPriorityWork(pawn) == WorkPriority.Ammo || GetPriorityWork(pawn) == WorkPriority.LowAmmo))
                    {
                        Predicate<Thing> validator = (Thing t) => t is AmmoThing && pawn.CanReserve(t, 1) 
                        && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true) 
                        && ((pawn.Faction.IsPlayer && !ForbidUtility.IsForbidden(t, pawn)) || !pawn.Faction.IsPlayer) 
                        && (t.Position.DistanceToSquared(pawn.Position) < 20f || room == RoomQuery.RoomAtFast(t.Position, pawn.Map));
                        List<Thing> curThingList = (
                            from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                            where validator(t)
                            select t
                            ).ToList();

                        foreach (Thing th in curThingList)
                        {
                            foreach (ThingDef thd in curAmmoList)
                            {
                                if (thd == th.def)
                                {
                                    //Defence from low count loot spam
                                    if (th.stackCount > pawn.equipment.Primary.TryGetComp<CompAmmoUser>().Props.magazineSize)
                                    {
                                        if (th.Position == pawn.Position || th.Position.AdjacentToCardinal(pawn.Position))
                                        {
                                            int numToCarry = 0;
                                            if (inventory.CanFitInInventory(th, out numToCarry))
                                            {
                                                return new Job(JobDefOf.TakeInventory, th)
                                                {
                                                    count = numToCarry,
                                                    expiryInterval = 150,
                                                    checkOverrideOnExpire = true,
                                                    canBash = true,
                                                    locomotionUrgency = LocomotionUrgency.Sprint
                                                };
                                            }
                                        }
                                        return GotoForce(pawn, th, PathEndMode.Touch);
                                    }
                                }
                            }
                        }
                    }
                }

                if (!pawn.Faction.IsPlayer && GetPriorityWork(pawn) == WorkPriority.Apparel &&  pawn.equipment.Primary != null)
                {
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso))
                    {
                        Apparel apparel = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Torso);
                        Log.Message(apparel.ToString());
                        if (apparel != null)
                        {
                            int numToapparel = 0;
                            if (inventory.CanFitInInventory(apparel, out numToapparel))
                            {
                                return new Job(JobDefOf.Wear, apparel)
                                {
                                    ignoreForbidden = true,
                                    locomotionUrgency = LocomotionUrgency.Sprint
                                };
                            }
                        }
                    }
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Legs))
                    {
                        Apparel apparel2 = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.Legs);
                        Log.Message(apparel2.ToString());
                        if (apparel2 != null)
                        {
                            int numToapparel2 = 0;
                            if (inventory.CanFitInInventory(apparel2, out numToapparel2))
                            {
                                return new Job(JobDefOf.Wear, apparel2)
                                {
                                    ignoreForbidden = true,
                                    locomotionUrgency = LocomotionUrgency.Sprint
                                };
                            }
                        }
                    }
                    /*
                    if (!pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead))
                    {
                        Apparel apparel3 = this.FindGarmentCoveringPart(pawn, BodyPartGroupDefOf.FullHead);
                        if (apparel3 != null)
                        {
                            int numToapparel3 = 0;
                            if (inventory.CanFitInInventory(apparel3, out numToapparel3))
                            {
                                return new Job(JobDefOf.Wear, apparel3)
                                {
                                    ignoreForbidden = true,
                                    locomotionUrgency = LocomotionUrgency.Sprint
                                };
                            }
                        }
                    }
                    */
                }
                return null;
            }
            return null;
        }

        private static Job GotoForce(Pawn pawn, LocalTargetInfo target, PathEndMode pathEndMode)
        {
            using (PawnPath pawnPath = pawn.Map.pathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassAnything, false), pathEndMode))
            {
                IntVec3 cellBeforeBlocker;
                Thing thing = pawnPath.FirstBlockingBuilding(out cellBeforeBlocker, pawn);
                if (thing != null)
                {
                    Job job = DigUtility.PassBlockerJob(pawn, thing, cellBeforeBlocker, true);
                    if (job != null)
                    {
                        return job;
                    }
                }
                if (thing == null)
                {
                    return new Job(JobDefOf.Goto, target, 100, true);
                }
                if (pawn.equipment.Primary != null)
                {
                    Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
                    if (primaryVerb.verbProps.ai_IsBuildingDestroyer && (!primaryVerb.verbProps.ai_IsIncendiary || thing.FlammableNow))
                    {
                        return new Job(JobDefOf.UseVerbOnThing)
                        {
                            targetA = thing,
                            verbToUse = primaryVerb,
                            expiryInterval = 100
                        };
                    }
                }
                return MeleeOrWaitJob(pawn, thing, cellBeforeBlocker);
            }
        }

        private static Job MeleeOrWaitJob(Pawn pawn, Thing blocker, IntVec3 cellBeforeBlocker)
        {
            if (!pawn.CanReserve(blocker, 1))
            {
                return new Job(JobDefOf.Goto, CellFinder.RandomClosewalkCellNear(cellBeforeBlocker, pawn.Map, 10), 100, true);
            }
            return new Job(JobDefOf.AttackMelee, blocker)
            {
                ignoreDesignations = true,
                expiryInterval = 100,
                checkOverrideOnExpire = true
            };
        }

        private Apparel FindGarmentCoveringPart(Pawn pawn, BodyPartGroupDef bodyPartGroupDef)
        {
            Room room = pawn.GetRoom();
            Predicate<Thing> validator = (Thing t) => pawn.CanReserve(t, 1) 
            && pawn.CanReach(t, PathEndMode.Touch, Danger.Deadly, true) 
            && (t.Position.DistanceToSquared(pawn.Position) < 12f || room == RoomQuery.RoomAtFast(t.Position, t.Map));
            List<Thing> aList = (
                from t in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
                orderby t.MarketValue - t.Position.DistanceToSquared(pawn.Position) * 2f descending
                where validator(t)
                select t
                ).ToList();

            foreach (Thing current in aList)
            {
                Apparel ap = current as Apparel;
                if (ap != null && ap.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) && pawn.CanReserve(ap, 1) && ApparelUtility.HasPartsToWear(pawn, ap.def))
                {
                    return ap;
                }
            }
            return null;
        }
    }
}
