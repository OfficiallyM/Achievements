using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achievements.Utilities
{
	internal static class GameUtilities
	{
		/// <summary>
		/// Find all child parts recursively with tosaveitemscript.
		/// </summary>
		/// <param name="root">Root part</param>
		/// <param name="children">Child partconditionscript passed by reference</param>
		public static void FindPartChildren(partconditionscript root, ref List<partconditionscript> children)
		{
			if (!children.Contains(root)) children.Add(root);

			tosaveitemscript tosave = root.GetComponent<tosaveitemscript>();
			if (tosave == null || tosave.partslotscripts == null) return;

			foreach (partslotscript slot in tosave.partslotscripts)
			{
				if (slot.part == null || slot.part.condition == null)
					continue;

				children.Add(slot.part.condition);
				FindPartChildren(slot.part.condition, ref children);
			}
		}
	}
}
