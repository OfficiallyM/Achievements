using Achievements.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TLDLoader;

namespace Achievements.Utilities
{
	internal static class SaveUtilities
	{
		private static AchievementSaveData _achievementData;
		private static Preferences _preferences;

		private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		/// <summary>
		/// Unserialize existing saved data.
		/// </summary>
		public static void Load()
		{
			_achievementData = new AchievementSaveData();
			_preferences = new Preferences();
			try
			{
				string file = Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Achievements.json");
				if (File.Exists(file))
				{
					string data = File.ReadAllText(file);
					if (string.IsNullOrEmpty(data)) return;
					_achievementData = JsonConvert.DeserializeObject<AchievementSaveData>(data, _jsonSettings);
				}
			}
			catch (Exception ex)
			{
				Logging.LogError($"Achievements load error. Details: {ex}");
			}

			try
			{
				string file = Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Preferences.json");
				if (File.Exists(file))
				{
					string data = File.ReadAllText(file);
					if (string.IsNullOrEmpty(data)) return;
					_preferences = JsonConvert.DeserializeObject<Preferences>(data, _jsonSettings);
				}
			}
			catch (Exception ex)
			{
				Logging.LogError($"Preferences load error. Details: {ex}");
			}
		}

		/// <summary>
		/// Serialize and write achievement data.
		/// </summary>
		private static void Save()
		{
			try
			{
				_achievementData.Version = Achievements.I.Version;
				string json = JsonConvert.SerializeObject(_achievementData, Formatting.None, _jsonSettings);
				File.WriteAllText(Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Achievements.json"), json);
			}
			catch (Exception ex)
			{
				Logging.LogError($"Achievement save error. Details: {ex}");
			}
		}

		/// <summary>
		/// Inserts a new achievement state or updates an existing one in the data store based on the specified identifiers.
		/// </summary>
		/// <remarks>If an achievement with the same ModId and AchievementId already exists, its state is updated;
		/// otherwise, a new entry is added. Changes are saved immediately after the operation.</remarks>
		/// <param name="state">The achievement state to insert or update. Must not be null. The state is identified by its ModId and
		/// AchievementId properties.</param>
		public static void Upsert(State state)
		{
			bool doesExist = _achievementData.Achievements.IndexOf(state) != -1;
			if (!doesExist)
				_achievementData.Achievements.Add(state);
			else
				Achievements.RaiseOnStateChange(state);
			Save();
		}

		/// <summary>
		/// Retrieves a list of achievement states.
		/// </summary>
		/// <returns>A list of <see cref="State"/> objects representing the achievements. The list will be empty if no achievements are
		/// available.</returns>
		public static List<State> GetAchievements()
		{
			return _achievementData.Achievements;
		}

		/// <summary>
		/// Retrieves the state of the specified achievement for a given mod, creating a new state if none exists and the
		/// achievement is defined.
		/// </summary>
		/// <remarks>If the achievement state does not exist but the achievement is defined, a new state is created
		/// and returned. If the achievement is not defined for the specified mod, the method returns null.</remarks>
		/// <param name="modId">The unique identifier of the mod that contains the achievement.</param>
		/// <param name="achievementId">The unique identifier of the achievement to retrieve.</param>
		/// <returns>A State object representing the achievement's current state if found or created; otherwise, null if the
		/// achievement is not defined.</returns>
		public static State GetAchievement(string modId, string achievementId)
		{
			foreach (var achievement in _achievementData.Achievements)
			{
				if (achievement.ModId == modId && achievement.AchievementId == achievementId) 
					return achievement;
			}

			var definition = Achievements.GetDefinition(modId, achievementId);
			if (definition != null)
			{
				var newState = new State()
				{
					ModId = modId,
					AchievementId = achievementId,
				};
				Upsert(newState);
				return newState;
			}

			return null;
		}

		/// <summary>
		/// Gets the number of achievements that have been unlocked.
		/// </summary>
		/// <returns>The total number of unlocked achievements. Returns 0 if no achievements are unlocked.</returns>
		public static int GetUnlockedCount()
		{
			int count = 0;
			foreach (var achievement in _achievementData.Achievements)
			{
				if (achievement.IsUnlocked)
					count++;
			}
			return count;
		}

		/// <summary>
		/// Saves user preferences.
		/// </summary>
		public static void SavePreferences()
		{
			try
			{
				string json = JsonConvert.SerializeObject(_preferences, Formatting.None, _jsonSettings);
				File.WriteAllText(Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Preferences.json"), json);
			}
			catch (Exception ex)
			{
				Logging.LogError($"Preferences save error. Details: {ex}");
			}
		}

		/// <summary>
		/// Gets the current user preferences.
		/// </summary>
		/// <returns>A <see cref="Preferences"/> object representing the current user preferences.</returns>
		public static Preferences GetPreferences() => _preferences;
	}
}
