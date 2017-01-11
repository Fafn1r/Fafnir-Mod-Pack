using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using HugsLib;

namespace Hygiene
{
    public class HygieneBase : ModBase
    {
        public override string ModIdentifier
        {
            get
            {
                return "Hygiene";
            }
        }

        public override void DefsLoaded()
        {
            HygieneUtility.Reset();
            //Saddle up, we're going in dry
            List<ThinkNode> nodes = DefDatabase<ThinkTreeDef>.GetNamed("Humanlike").thinkRoot.subNodes.SelectMany(n => n.subNodes.SelectMany(n2 => (from n3 in n2.subNodes
                                                                                                                                                    where n3 is ThinkNode_PrioritySorter
                                                                                                                                                    select n3))).ToList();
            foreach (ThinkNode n in nodes)
            {
                n.subNodes.Add(new JobGiver_GetHygiene());
                n.ResolveSubnodesAndRecur();
            }
        }
    }
}
