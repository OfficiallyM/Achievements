using System;
using System.Reflection;
using TLDLoader;

namespace Achievements
{
	public static class AchievementManager
	{
		private static Mod I;
		private static Mod _achievementsMod;
		private static Type _achievementsType;
		private static object _achievementsInstance;

		private static MethodInfo _registerMethod;
		private static MethodInfo _addProgressMethod;
		private static MethodInfo _unlockMethod;
		private static MethodInfo _isUnlockedMethod;
		private static MethodInfo _getProgressMethod;

		/// <summary>
		/// Initialises achievements system for the specified <see cref="Mod"/> object.
		/// </summary>
		/// <param name="mod">The <see cref="Mod"/> instance to use for initialization. Cannot be null.</param>
		public static void Init(Mod mod) => I = mod;

		/// <summary>
		/// Registers a new achievement definition for the specified mod.
		/// </summary>
		/// <remarks>Use this method to define achievements before they are awarded or tracked. Attempting to register
		/// the same achievement more than once will result in an exception.</remarks>
		/// <param name="achievementId">The unique identifier for the achievement. Cannot be null or empty.</param>
		/// <param name="name">The display name of the achievement. Cannot be null or empty.</param>
		/// <param name="description">A description of the achievement, shown to users when viewing achievement details.</param>
		/// <param name="maxProgress">The maximum progress value required to complete the achievement. If null, the achievement is considered binary
		/// (unlocked or not). Must be positive if specified.</param>
		/// <param name="isSecret">A value indicating whether the achievement is hidden from users until it is unlocked. Set to <see
		/// langword="true"/> to make the achievement secret; otherwise, <see langword="false"/>.</param>
		/// <exception cref="InvalidOperationException">Thrown if the achievement system has not been initialized, or if an achievement with the same achievement ID has already been registered.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found.</exception>
		public static void RegisterAchievement(string achievementId, string name, string description, int? maxProgress = null, bool isSecret = false)
		{
			if (I == null)
				throw new InvalidOperationException("Init method needs to be called first");
			if (!ResolveAchievements()) return;
			try
			{
				_registerMethod?.Invoke(_achievementsInstance, new object[] { I.ID, achievementId, name, description, maxProgress, isSecret });
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		/// <summary>
		/// Adds progress toward unlocking the specified achievement.
		/// </summary>
		/// <remarks>If the achievement's progress reaches or exceeds its maximum, the achievement is marked as
		/// unlocked. If the achievement does not have a maximum progress value, it is unlocked immediately.</remarks>
		/// <param name="achievementId">The unique identifier of the achievement to which progress will be added. Cannot be null or empty.</param>
		/// <param name="amount">The amount of progress to add. Defaults to 1. Must be a positive integer.</param>
		/// <exception cref="InvalidOperationException">Thrown if the achievement system has not been initialized by calling the Init method.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found.</exception>
		public static void AddProgress(string achievementId, int amount = 1)
		{
			if (I == null)
				throw new InvalidOperationException("Init method needs to be called first");
			if (!ResolveAchievements()) return;
			try
			{
				_addProgressMethod?.Invoke(_achievementsInstance, new object[] { I.ID, achievementId, amount });
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		/// <summary>
		/// Unlocks the specified achievement for the current user.
		/// </summary>
		/// <param name="achievementId">The unique identifier of the achievement to unlock. Cannot be null or empty.</param>
		/// <exception cref="InvalidOperationException">Thrown if the achievement system has not been initialized by calling the Init method, or if the specified achievement is a progress-based achievement. Use AddProgress to increment progress
		/// instead.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found.</exception>
		public static void UnlockAchievement(string achievementId)
		{
			if (I == null)
				throw new InvalidOperationException("Init method needs to be called first");
			if (!ResolveAchievements()) return;
			try
			{
				_unlockMethod?.Invoke(_achievementsInstance, new object[] { I.ID, achievementId });
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		/// <summary>
		/// Determines whether the specified achievement has been unlocked.
		/// </summary>
		/// <param name="achievementId">The unique identifier of the achievement to check. Cannot be null.</param>
		/// <returns>true if the achievement is unlocked for the current user; otherwise, false.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the initialization method has not been called before invoking this method.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found.</exception>
		public static bool IsUnlocked(string achievementId)
		{
			if (I == null)
				throw new InvalidOperationException("Init method needs to be called first");
			if (!ResolveAchievements()) return false;
			try
			{
				return (bool)(_isUnlockedMethod?.Invoke(_achievementsInstance, new object[] { I.ID, achievementId }) ?? false);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		/// <summary>
		/// Gets the current progress value for the specified achievement.
		/// </summary>
		/// <param name="achievementId">The unique identifier of the achievement for which to retrieve progress. Cannot be null or empty.</param>
		/// <returns>An integer representing the current progress of the specified achievement. Returns 0 if the achievement is not
		/// found or achievements are not available.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the initialization method has not been called before invoking this method.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found.</exception>
		public static int GetProgress(string achievementId)
		{
			if (I == null)
				throw new InvalidOperationException("Init method needs to be called first");
			if (!ResolveAchievements()) return 0;
			try
			{
				return (int)(_getProgressMethod?.Invoke(_achievementsInstance, new object[] { I.ID, achievementId }) ?? 0);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		private static bool AchievementsLoaded()
		{
			if (_achievementsMod == null)
			{
				foreach (var mod in ModLoader.LoadedMods)
				{
					if (mod.ID == "M_Achievements")
					{
						_achievementsMod = mod;
						break;
					}
				}
			}

			return _achievementsMod != null;
		}

		private static bool ResolveAchievements()
		{
			if (_achievementsType != null) return true;
			if (!AchievementsLoaded()) return false;

			_achievementsType = _achievementsMod.GetType().Assembly.GetType("Achievements.Achievements");
			if (_achievementsType == null) return false;

			_achievementsInstance = _achievementsType.GetField("I", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);

			_registerMethod = _achievementsType.GetMethod("RegisterAchievement", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(int?), typeof(bool) });
			_addProgressMethod = _achievementsType.GetMethod("AddProgress", new Type[] { typeof(string), typeof(string), typeof(int) });
			_unlockMethod = _achievementsType.GetMethod("UnlockAchievement", new Type[] { typeof(string), typeof(string) });
			_isUnlockedMethod = _achievementsType.GetMethod("IsUnlocked", new Type[] { typeof(string), typeof(string) });
			_getProgressMethod = _achievementsType.GetMethod("GetProgress", new Type[] { typeof(string), typeof(string) });

			return _achievementsInstance != null;
		}
	}
}
