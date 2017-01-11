using RimWorld;
using System;
using System.IO;
using System.Reflection;
using Verse;

namespace PermaDeathNameFix
{
    [StaticConstructorOnStartup]
    public class Injector_NameFix
    {
        static Injector_NameFix()
        {
            MethodInfo method = typeof(NamePlayerFactionDialogUtility).GetMethod("IsValidName", new Type[] { typeof(string) });
            MethodInfo method2 = typeof(ReplacementCode).GetMethod("_IsValidName", new Type[] { typeof(string) });
            Detour.TryDetourFromTo(method, method2);
            
            MethodInfo method3 = typeof(PermadeathModeUtility).GetMethod("CheckUpdatePermadeathModeUniqueNameOnGameLoad");
            MethodInfo method4 = typeof(ReplacementCode).GetMethod("_CheckUpdatePermadeathModeUniqueNameOnGameLoad");
            Detour.TryDetourFromTo(method3, method4);
            
        }
    }
}