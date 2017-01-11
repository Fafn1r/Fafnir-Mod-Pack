using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace Combat_Realism
{
    public class CompExplosiveCR : ThingComp
    {
        public CompProperties_ExplosiveCR Props
        {
            get
            {
                return (CompProperties_ExplosiveCR)props;
            }
        }

        /// <summary>
        /// Produces a secondary explosion on impact using the explosion values from the projectile's projectile def. Requires the projectile's launcher to be passed on due to protection level, 
        /// only works when parent can be cast as ProjectileCR. Intended use is for HEAT and similar weapons that spawn secondary explosions while also penetrating, NOT explosive ammo of 
        /// anti-materiel rifles as the explosion just spawns on top of the pawn, not inside the hit body part.
        /// 
        /// Additionally handles fragmentation effects if defined.
        /// </summary>
        /// <param name="instigator">Launcher of the projectile calling the method</param>
		public virtual void Explode(Thing instigator, IntVec3 pos, Map map)
        {
            // Regular explosion stuff
            if (Props.explosionRadius > 0 && Props.explosionDamage > 0)
            {
                GenExplosion.DoExplosion
                    (pos,
                    map,
                    Props.explosionRadius,
                    Props.explosionDamageDef,
                    instigator,
                    Props.soundExplode == null ? Props.explosionDamageDef.soundExplosion : Props.soundExplode,
                    parent.def, 
                    null,
                    Props.postExplosionSpawnThingDef,
                    Props.explosionSpawnChance, 
                    1, 
                    Props.applyDamageToExplosionCellsNeighbors, 
                    Props.preExplosionSpawnThingDef, 
                    Props.explosionSpawnChance,
                    1);
            }
            // Fragmentation stuff
            if (!Props.fragments.NullOrEmpty())
            {
                if (Props.fragRange <= 0)
                {
                    Log.Error(parent.LabelCap + " has fragments but no fragRange");
                }

                else
                {
                    Vector3 exactOrigin = new Vector3(0, 0, 0);
                    exactOrigin.x = parent.DrawPos.x;
                    exactOrigin.z = parent.DrawPos.z;
                    foreach (ThingCountClass fragment in Props.fragments)
                    {
                        for (int i = 0; i < fragment.count; i++)
                        {
                            ProjectileCR projectile = (ProjectileCR)ThingMaker.MakeThing(fragment.thingDef, null);
                            projectile.canFreeIntercept = true;
                            Vector3 exactTarget = exactOrigin + (new Vector3(1, 0, 1) * UnityEngine.Random.Range(0, Props.fragRange)).RotatedBy(UnityEngine.Random.Range(0, 360));
                            LocalTargetInfo targetCell = exactTarget.ToIntVec3();
                            GenSpawn.Spawn(projectile, parent.Position, map);
                            projectile.Launch(instigator, exactOrigin, targetCell, exactTarget, null);
                        }
                    }
                }
            }
        }
    }
}
