namespace Achievements.Utilities
{
	internal static class Logging
	{
		public static void Log(string message, TLDLoader.Logger.LogLevel logLevel = TLDLoader.Logger.LogLevel.Info) =>
			Achievements.I.Logger.Log(message, logLevel);

		public static void LogDebug(string message)
		{
#if DEBUG
			Achievements.I.Logger.LogDebug(message);
#endif
		}

		public static void LogInfo(string message) =>
			Achievements.I.Logger.LogInfo(message);

		public static void LogWarning(string message) =>
			Achievements.I.Logger.LogWarning(message);

		public static void LogError(string message) =>
			Achievements.I.Logger.LogError(message);

		public static void LogCritical(string message) =>
			Achievements.I.Logger.LogCritical(message);
	}
}
