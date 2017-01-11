using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PermaDeathNameFix
{
    static class ReplacementCode
    {
        public static bool _IsValidName(string s)
        {
            bool valid = s.Length != 0 && GenText.IsValidFilename(s);
            if (Current.Game.Info.permadeathMode)
            {
                if (valid)
                {
                    foreach (FileInfo current in GenFilePaths.AllSavedGameFiles)
                        if (current.Name.Substring(0, current.Name.LastIndexOf('.')).Equals(Current.Game.Info.permadeathModeUniqueName))
                            current.MoveTo(current.Directory.FullName + "\\" + s + " (Permadeath)" + current.Extension);

                    Current.Game.Info.permadeathModeUniqueName = s + " (Permadeath)";
                }
            }
            return valid;
        }

        public static void _CheckUpdatePermadeathModeUniqueNameOnGameLoad(string filename)
        {
            try
            {
                if (Current.Game.Info.permadeathMode)
                {
                    GameInfo info = Current.Game.Info;
                    if (Faction.OfPlayer.HasName)
                        if (info.permadeathModeUniqueName != (Faction.OfPlayer.Name) + " (Permadeath)")
                        {
                            Current.Game.Info.permadeathModeUniqueName = Faction.OfPlayer.Name + " (Permadeath)";
                            Log.Warning("Faction's name has changed and doesn't match save name. Fixing...");
                            foreach (FileInfo current in GenFilePaths.AllSavedGameFiles)
                                if (current.Name.Substring(0, current.Name.LastIndexOf('.')).Equals(filename))
                                    current.MoveTo(current.Directory.FullName + "\\" + info.permadeathModeUniqueName + current.Extension);
                        }
                }
            } catch(Exception ex)
            {
                Log.Message(ex.Message);
            }
        }
    }
}
