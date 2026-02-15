using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achievements.Harmony
{
	[HarmonyPatch(typeof(fpscontroller), nameof(fpscontroller.Death))]
	internal static class Patch_Player_Death
	{
		private static void Prefix()
		{
			Achievements.UnlockAchievement("die");
		}
	}
}
