using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib;
using System.Reflection;
using HugsLib.Utils;
using HugsLib.Settings;
using HugsLib.Source.Detour;
using Psychology.Detour;

namespace Psychology
{
    public class PsychologyBase : HugsLib.ModBase
    {
        private static bool kinsey = true;
        private static KinseyMode mode = KinseyMode.Realistic;
        private static bool sabotabby = false;
        public static bool detoursMedical = true;
        public static bool detoursSexual = true;
        private SettingHandle<bool> toggleKinsey;
        private SettingHandle<bool> toggleEmpathy;
        private SettingHandle<KinseyMode> kinseyMode;
        private SettingHandle<bool> toggleSabotage;

        public enum KinseyMode
        {
            Realistic,
            Invisible,
            Gaypocalypse
        };

        static public bool ActivateKinsey()
        {
            return kinsey;
        }

        static public KinseyMode KinseyFormula()
        {
            return mode;
        }

        static public bool SabotageOn()
        {
            return sabotabby;
        }

        public override string ModIdentifier
        {
            get
            {
                return "Psychology";
            }
        }

        private static TraitDef AddConflictingTraits(String name, TraitDef[] traits)
        {
            TraitDef trait = TraitDef.Named(name);
            if (trait != null)
            {
                if (trait.conflictingTraits == null)
                {
                    trait.conflictingTraits = new List<TraitDef>();
                }
                foreach (TraitDef conflict in traits)
                {
                    trait.conflictingTraits.Add(conflict);
                }
            }
            return trait;
        }

        private static ThoughtDef AddNullifyingTraits(String name, TraitDef[] traits)
        {
            ThoughtDef thought = ThoughtDef.Named(name);
            if (thought != null)
            {
                if (thought.nullifyingTraits == null)
                {
                    thought.nullifyingTraits = new List<TraitDef>();
                }
                foreach (TraitDef conflict in traits)
                {
                    thought.nullifyingTraits.Add(conflict);
                }
            }
            return thought;
        }

        private static ThoughtDef ReplaceThoughtWorker(String name, Type newWorker)
        {
            ThoughtDef thought = ThoughtDef.Named(name);
            if (thought != null && thought.workerClass != null)
            {
                thought.workerClass = newWorker;
            }
            return thought;
        }

        private static IncidentDef ReplaceIncidentWorker(String name, Type newWorker)
        {
            IncidentDef incident = IncidentDef.Named(name);
            if (incident != null && incident.workerClass != null)
            {
                incident.workerClass = newWorker;
            }
            return incident;
        }

        private static void RemoveTrait(Pawn pawn, TraitDef trait)
        {
            pawn.story.traits.allTraits.RemoveAll(t => t.def == trait);
        }

        public override void SettingsChanged()
        {
            kinsey = toggleKinsey.Value;
            mode = kinseyMode.Value;
            sabotabby = toggleSabotage.Value;
        }

        public override void DefsLoaded()
        {
            if (ModIsActive)
            {
                toggleEmpathy = Settings.GetHandle<bool>("EnableEmpathy", "EmpathyChangesTitle".Translate(), "EmpathyChangesTooltip".Translate(), true);
                toggleKinsey = Settings.GetHandle<bool>("EnableSexuality", "SexualityChangesTitle".Translate(), "SexualityChangesTooltip".Translate(), true);
                kinseyMode = Settings.GetHandle<KinseyMode>("KinseyMode", "KinseyModeTitle".Translate(), "KinseyModeTooltip".Translate(), KinseyMode.Realistic, null, "KinseyMode_");
                toggleSabotage = Settings.GetHandle<bool>("EnableSabotage", "SabotageIncidentTitle".Translate(), "SabotageIncidentTooltip".Translate(), false);

                sabotabby = toggleSabotage;

                if (!detoursMedical)
                {
                    Logger.Warning("MedicalDetourDisable".Translate());
                }

                //Vanilla Defs will be edited at load to improve compatibility with other mods.
                AddConflictingTraits("Nudist", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddConflictingTraits("Bloodlust", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddConflictingTraits("Psychopath", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized, TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("Cannibal", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Gourmet });
                AddConflictingTraits("Ascetic", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddConflictingTraits("Neurotic", new TraitDef[] { TraitDefOfPsychology.HeavySleeper });
                AddConflictingTraits("DislikesMen", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("DislikesWomen", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddConflictingTraits("Prosthophobe", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                
                TraitDef bisexual = DefDatabase<TraitDef>.GetNamedSilentFail("Bisexual");
                TraitDef asexual = DefDatabase<TraitDef>.GetNamedSilentFail("Asexual");
                if (bisexual != null || asexual != null || !toggleKinsey || !detoursSexual)
                {
                    if (toggleKinsey)
                    {
                        Logger.Message("KinseyDisable".Translate());
                        if(!detoursSexual)
                        {
                            Logger.Warning("KinseyDetourDisable".Translate());
                            TraitDefOfPsychology.Codependent.SetCommonality(0f);
                            TraitDefOfPsychology.Lecher.SetCommonality(0f);
                            TraitDefOfPsychology.OpenMinded.SetCommonality(0f);
                            TraitDefOfPsychology.Polygamous.SetCommonality(0f);
                        }
                    }
                    kinsey = false;
                }

                if (PsychologyBase.ActivateKinsey())
                {
                    mode = kinseyMode.Value;
                    TraitDef gay = TraitDef.Named("Gay");
                    if (gay != null)
                    {
                        gay.SetCommonality(0f);
                    }
                    foreach (ThingDef t in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (t.thingClass == typeof(Pawn))
                        {
                            t.thingClass = typeof(PsychologyPawn);
                        }
                    }
                }
                
                AddNullifyingTraits("AteLavishMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteFineMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteAwfulMeal", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteRawFood", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteInsectMeatAsIngredient", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteInsectMeatDirect", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("AteRottenFood", new TraitDef[] { TraitDefOfPsychology.Gourmet });
                AddNullifyingTraits("SleepDisturbed", new TraitDef[] { TraitDefOfPsychology.HeavySleeper });
                AddNullifyingTraits("ObservedLayingCorpse", new TraitDef[] { TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("WitnessedDeathAlly", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("WitnessedDeathNonAlly", new TraitDef[] { TraitDefOfPsychology.BleedingHeart, TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("FeelingRandom", new TraitDef[] { TraitDefOfPsychology.Unstable });
                AddNullifyingTraits("ApparelDamaged", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddNullifyingTraits("EnvironmentDark", new TraitDef[] { TraitDefOfPsychology.Photosensitive });
                AddNullifyingTraits("DeadMansApparel", new TraitDef[] { TraitDefOfPsychology.Desensitized });
                AddNullifyingTraits("Naked", new TraitDef[] { TraitDefOfPsychology.Prude });
                AddNullifyingTraits("ColonistLeftUnburied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                AddNullifyingTraits("CheatedOnMe", new TraitDef[] { TraitDefOfPsychology.Polygamous });
                AddNullifyingTraits("Affair", new TraitDef[] { TraitDefOfPsychology.Polygamous });
                AddNullifyingTraits("Disfigured", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("Pretty", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("Ugly", new TraitDef[] { TraitDefOfPsychology.OpenMinded });
                AddNullifyingTraits("SleptOutside", new TraitDef[] { TraitDefOfPsychology.Outdoorsy });

                ThoughtDef knowGuestExecuted = AddNullifyingTraits("KnowGuestExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowGuestExecuted != null && toggleEmpathy)
                {
                    if (knowGuestExecuted.stages.Count >= 1) //We can't assume that other mods won't remove stages from this thought, so we'll try to avoid OutOfBounds exceptions.
                    {
                        knowGuestExecuted.stages[0].baseMoodEffect = -1;
                    }
                    if (knowGuestExecuted.stages.Count >= 2)
                    {
                        knowGuestExecuted.stages[1].baseMoodEffect = -2;
                    }
                    if (knowGuestExecuted.stages.Count >= 3)
                    {
                        knowGuestExecuted.stages[2].baseMoodEffect = -4;
                    }
                    if (knowGuestExecuted.stages.Count >= 4)
                    {
                        knowGuestExecuted.stages[3].baseMoodEffect = -5;
                    }
                }

                ThoughtDef knowColonistExecuted = AddNullifyingTraits("KnowColonistExecuted", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistExecuted != null && toggleEmpathy)
                {
                    if (knowColonistExecuted.stages.Count >= 1)
                    {
                        knowColonistExecuted.stages[0].baseMoodEffect = -1;
                    }
                    if (knowColonistExecuted.stages.Count >= 2)
                    {
                        knowColonistExecuted.stages[1].baseMoodEffect = -2;
                    }
                    if (knowColonistExecuted.stages.Count >= 3)
                    {
                        knowColonistExecuted.stages[2].baseMoodEffect = -4;
                    }
                    if (knowColonistExecuted.stages.Count >= 4)
                    {
                        knowColonistExecuted.stages[3].baseMoodEffect = -5;
                    }
                }

                ThoughtDef knowPrisonerDiedInnocent = AddNullifyingTraits("KnowPrisonerDiedInnocent", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowPrisonerDiedInnocent != null && toggleEmpathy)
                {
                    if (knowPrisonerDiedInnocent.stages.Count >= 1)
                    {
                        knowPrisonerDiedInnocent.stages[0].baseMoodEffect = -4;
                    }
                }

                ThoughtDef knowColonistDied = AddNullifyingTraits("KnowColonistDied", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistDied != null && toggleEmpathy)
                {
                    if (knowColonistDied.stages.Count >= 1)
                    {
                        knowColonistDied.stages[0].baseMoodEffect = -2;
                    }
                }

                ThoughtDef colonistAbandoned = AddNullifyingTraits("ColonistAbandoned", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandoned != null && toggleEmpathy)
                {
                    if (colonistAbandoned.stages.Count >= 1)
                    {
                        colonistAbandoned.stages[0].baseMoodEffect = -2;
                    }
                }

                ThoughtDef colonistAbandonedToDie = AddNullifyingTraits("ColonistAbandonedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (colonistAbandonedToDie != null && toggleEmpathy)
                {
                    if (colonistAbandonedToDie.stages.Count >= 1)
                    {
                        colonistAbandonedToDie.stages[0].baseMoodEffect = -4;
                    }
                }

                ThoughtDef prisonerAbandonedToDie = AddNullifyingTraits("PrisonerAbandonedToDie", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (prisonerAbandonedToDie != null && toggleEmpathy)
                {
                    if (prisonerAbandonedToDie.stages.Count >= 1)
                    {
                        prisonerAbandonedToDie.stages[0].baseMoodEffect = -3;
                    }
                }

                ThoughtDef knowPrisonerSold = AddNullifyingTraits("KnowPrisonerSold", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowPrisonerSold != null && toggleEmpathy)
                {
                    if (knowPrisonerSold.stages.Count >= 1)
                    {
                        knowPrisonerSold.stages[0].baseMoodEffect = -4;
                    }
                }

                ThoughtDef knowGuestOrganHarvested = AddNullifyingTraits("KnowGuestOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowGuestOrganHarvested != null && toggleEmpathy)
                {
                    if (knowGuestOrganHarvested.stages.Count >= 1)
                    {
                        knowGuestOrganHarvested.stages[0].baseMoodEffect = -4;
                    }
                }

                ThoughtDef knowColonistOrganHarvested = AddNullifyingTraits("KnowColonistOrganHarvested", new TraitDef[] { TraitDefOfPsychology.BleedingHeart });
                if (knowColonistOrganHarvested != null && toggleEmpathy)
                {
                    if (knowColonistOrganHarvested.stages.Count >= 1)
                    {
                        knowColonistOrganHarvested.stages[0].baseMoodEffect = -4;
                    }
                }

                ReplaceThoughtWorker("CabinFever", typeof(ThoughtWorker_CabinFever));
                ReplaceThoughtWorker("Disfigured", typeof(ThoughtWorker_Disfigured));
                ReplaceThoughtWorker("Ugly", typeof(ThoughtWorker_Ugly));
                ReplaceThoughtWorker("AnnoyingVoice", typeof(ThoughtWorker_AnnoyingVoice));
                ReplaceThoughtWorker("CreepyBreathing", typeof(ThoughtWorker_CreepyBreathing));
                ReplaceThoughtWorker("Pretty", typeof(ThoughtWorker_Pretty));
                ReplaceIncidentWorker("RaidEnemy", typeof(IncidentWorker_RaidEnemy));

                // Credit to FluffierThanThou for the code.
                var livingRaces = DefDatabase<ThingDef>
                    .AllDefsListForReading
                    .Where(t => !t.race?.hediffGiverSets?.NullOrEmpty() ?? false);

                foreach (ThingDef alive in livingRaces)
                {
                    if (alive.race.hediffGiverSets.Contains(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicStandard")))
                    {
                        alive.race.hediffGiverSets.Add(DefDatabase<HediffGiverSetDef>.GetNamed("OrganicPsychology"));
                        alive.recipes.Add(RecipeDefOfPsychology.TreatPyromania);
                    }
                }

                MentalBreakDef berserk = DefDatabase<MentalBreakDef>.GetNamed("Berserk");
                berserk.baseCommonality = 0f;
                MentalStateDef fireStartingSpree = DefDatabase<MentalStateDef>.GetNamed("FireStartingSpree");
                fireStartingSpree.workerClass = typeof(MentalStateWorker_FireStartingSpree);
            }
        }

        public override void MapLoaded(Map map)
        {
            if (ModIsActive && PsychologyBase.ActivateKinsey())
            {
                List<Pawn> gayPawns = (from p in map.mapPawns.AllPawns
                                       where p.RaceProps.Humanlike && p.story.traits.HasTrait(TraitDefOf.Gay)
                                       select p).ToList();
                foreach (Pawn pawn in gayPawns)
                {
                    PsychologyBase.RemoveTrait(pawn, TraitDefOf.Gay);
                    PsychologyPawn realPawn = pawn as PsychologyPawn;
                    if (realPawn != null && realPawn.sexuality.kinseyRating < 5)
                    {
                        realPawn.sexuality.kinseyRating = Rand.RangeInclusive(5, 6);
                    }
                }
            }
        }
    }
}
