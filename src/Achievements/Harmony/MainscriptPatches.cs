using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Achievements.Harmony
{
	[HarmonyPatch(typeof(mainscript), "DistanceWrite")]
	internal class Patch_mainscript_DistanceWrite
	{
		static void Postfix(float p)
		{
			Achievements.AddProgress(Achievements.I.ID, "drive_5000_global", Mathf.RoundToInt(p));
			Achievements.AddProgress(Achievements.I.ID, "drive_10000_global", Mathf.RoundToInt(p));
			Achievements.AddProgress(Achievements.I.ID, "drive_25000_global", Mathf.RoundToInt(p));
			Achievements.AddProgress(Achievements.I.ID, "drive_50000_global", Mathf.RoundToInt(p));
		}
	}
}
