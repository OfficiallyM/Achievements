using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achievements.Harmony
{
	[HarmonyPatch(typeof(newAiScript), nameof(newAiScript.Die))]
	internal static class Patch_newAiScript_Die
	{
		private static void Prefix()
		{
			Achievements.AddProgress("munkas_100");
		}
	}

	[HarmonyPatch(typeof(aiscript), nameof(aiscript.Die))]
	internal static class Patch_aiscript_Die
	{
		private static void Prefix()
		{
			Achievements.AddProgress("bunnies_100");
		}
	}
}
