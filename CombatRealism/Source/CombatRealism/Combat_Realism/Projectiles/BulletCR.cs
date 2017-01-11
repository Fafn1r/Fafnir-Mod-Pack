using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace Combat_Realism
{
    public class BulletCR : ProjectileCR
    {
        private const float StunChance = 0.1f;

        protected override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            if (hitThing != null)
            {
                int damageAmountBase = def.projectile.damageAmountBase;
                ThingDef equipmentDef = this.equipmentDef;
                DamageDef_CR damDefCR = def.projectile.damageDef as DamageDef_CR;

                DamageInfo dinfo = new DamageInfo(
                    def.projectile.damageDef,
                    damageAmountBase,
                    ExactRotation.eulerAngles.y,
                    launcher,
                    null,
                    equipmentDef);

              //  if (damDefCR != null && damDefCR.harmOnlyOutsideLayers) dinfo.ForceHitPart.depth = BodyPartDepth.Outside;

                ProjectilePropertiesCR propsCR = def.projectile as ProjectilePropertiesCR;
                if (propsCR != null && !propsCR.secondaryDamage.NullOrEmpty())
                {
                   // Log.Message("propsCR: " + propsCR.ToString());
                    // Get the correct body part
                    Pawn pawn = hitThing as Pawn;
                    if (pawn != null && def.projectile.damageDef.workerClass == typeof(DamageWorker_AddInjuryCR))
                    {
                        BodyPartRecord exactPartFromDamageInfo = DamageWorker_AddInjuryCR.GetExactPartFromDamageInfo(dinfo, pawn);
                        dinfo = new DamageInfo(
                            dinfo.Def,
                            dinfo.Amount,
                            dinfo.Angle,
                            dinfo.Instigator,
                            exactPartFromDamageInfo = (DamageWorker_AddInjuryCR.GetExactPartFromDamageInfo(dinfo, pawn)),
                            dinfo.WeaponGear);
                    }
                    List<DamageInfo> dinfoList = new List<DamageInfo>() { dinfo };
                    foreach (SecondaryDamage secDamage in propsCR.secondaryDamage)
                    {
                        dinfoList.Add(new DamageInfo(
                            secDamage.def,
                            secDamage.amount,
                            dinfo.Angle,
                            dinfo.Instigator,
                            dinfo.ForceHitPart,
                            dinfo.WeaponGear));
                    }
                    foreach (DamageInfo curDinfo in dinfoList)
                    {
                        hitThing.TakeDamage(curDinfo);
                    }
                }
                else
                {
                    hitThing.TakeDamage(dinfo);
                }
            }
            else
            {
                SoundDefOf.BulletImpactGround.PlayOneShot(new TargetInfo(Position, map, false));
                MoteMaker.MakeStaticMote(ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
            }
        }
    }
}