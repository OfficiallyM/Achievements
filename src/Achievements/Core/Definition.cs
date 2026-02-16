namespace Achievements.Core
{
	public class Definition
	{
		public string ModId { get; set; }
		public string AchievementId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int? MaxProgress { get; set; } = null;
		public bool IsSecret { get; set; } = false;
	}
}
