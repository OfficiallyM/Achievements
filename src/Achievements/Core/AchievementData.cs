namespace Achievements.Core
{
	public class AchievementData
	{
		public string ModId { get; set; }
		public string AchievementId { get; set; }
		public Definition Definition { get; set; }
		public State State { get; set; }

		public AchievementData(string modId, string achievementId, Definition definition, State state)
		{
			ModId = modId;
			AchievementId = achievementId;
			Definition = definition;
			State = state;
		}
	}
}
