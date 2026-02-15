using Achievements.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TLDLoader;

namespace Achievements.Utilities
{
	internal static class SaveUtilities
	{
		private static SaveData _data;

		private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		/// <summary>
		/// Unserialize existing save data.
		/// </summary>
		public static void Load()
		{
			_data = new SaveData();
			try
			{
				string file = Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Achievements.json");
				if (File.Exists(file))
				{
					string data = File.ReadAllText(file);
					if (string.IsNullOrEmpty(data)) return;
					_data = JsonConvert.DeserializeObject<SaveData>(data, _jsonSettings);
				}
			}
			catch (Exception ex)
			{
				Logging.LogError($"Save Load() error. Details: {ex}");
			}
		}

		/// <summary>
		/// Serialize save data and write to save.
		/// </summary>
		private static void Save()
		{
			try
			{
				_data.Version = Achievements.I.Version;
				string json = JsonConvert.SerializeObject(_data, Formatting.None, _jsonSettings);
				File.WriteAllText(Path.Combine(ModLoader.GetModConfigFolder(Achievements.I), "Achievements.json"), json);
			}
			catch (Exception ex)
			{
				Logging.LogError($"Save Commit() error. Details: {ex}");
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
			bool doesExist = _data.Achievements.IndexOf(state) != -1;
			if (!doesExist)
				_data.Achievements.Add(state);
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
			return _data.Achievements;
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
			foreach (var achievement in _data.Achievements)
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

		public static int GetUnlockedCount()
		{
			int count = 0;
			foreach (var achievement in _data.Achievements)
			{
				if (achievement.IsUnlocked)
					count++;
			}
			return count;
		}
	}
}
