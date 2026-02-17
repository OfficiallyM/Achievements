using System.Collections.Generic;

namespace Achievements.Core
{
	internal sealed class AchievementSaveData
	{
		public List<State> Achievements { get; set; } = new List<State>();
		public string Version { get; set; }
	}
}
