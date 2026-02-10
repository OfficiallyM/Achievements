using System;

namespace Achievements.Core
{
	public class State
	{
		public string ModId { get; set; }
		public string AchievementId { get; set; }
		public bool IsUnlocked { get; set; } = false;
		public DateTime? UnlockedAt { get; set; }
		public string UnlockedSaveName { get; set; }
		public int? Progress { get; set; } = null;
	}
}
