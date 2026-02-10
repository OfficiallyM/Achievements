using Achievements.Core;
using Achievements.Utilities;
using Achievements.Utilities.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TLDLoader;
using UnityEngine;

namespace Achievements
{
	public class Achievements : Mod
	{
		// Mod meta stuff.
		private string _version = "0.0.1";
		public override string ID => "M_Achievements";
		public override string Name => "Achievements";
		public override string Author => "M-";
		public override string Version => _version;
		public override bool UseLogger => true;
		public override bool LoadInMenu => true;

		internal static Achievements I;
		internal static List<Definition> Definitions = new List<Definition>();
		internal static List<AchievementData> Data = new List<AchievementData>();
		internal static bool Debug = false;
		internal static bool IsOnMenu => mainscript.M == null;
		internal static bool IsPaused => mainscript.M?.menu?.Menu?.activeSelf ?? false;

		private bool _showUI = false;
		private static int _achievementsMissingDefinition = 0;
		private Vector2 _scrollPosition = Vector2.zero;
		private bool _hasPostLoaded = false;

		public Achievements()
		{
			I = this;
#if DEBUG
			_version += "-DEV";
			Debug = true;
#endif
		}

		public override void OnMenuLoad()
		{
			SaveUtilities.Load();

			// Test achievements.
			RegisterAchievement(ID, "first_start", "Hello World", "Start the game for the first time");
			RegisterAchievement(ID, "secret_test", "Secret Achievement", "Find the hidden thing", isSecret: true);
			RegisterAchievement(ID, "drive_1k", "Starting the drive", "Drive 1 kilometer", maxProgress: 1);

			UnlockAchievement(ID, "first_start");

			BuildData();
		}

		/// <summary>
		/// Registers a new achievement definition for the specified mod.
		/// </summary>
		/// <remarks>Use this method to define achievements before they are awarded or tracked. Attempting to register
		/// the same achievement more than once will result in an exception.</remarks>
		/// <param name="modId">The unique identifier of the mod to which the achievement belongs. Cannot be null or empty.</param>
		/// <param name="achievementId">The unique identifier for the achievement within the specified mod. Cannot be null or empty.</param>
		/// <param name="name">The display name of the achievement. Cannot be null or empty.</param>
		/// <param name="description">A description of the achievement's purpose or how it is earned. Cannot be null or empty.</param>
		/// <param name="maxProgress">The maximum progress value required to complete the achievement, or null if the achievement does not track
		/// progress.</param>
		/// <param name="isSecret">true if the achievement should be hidden from users until unlocked; otherwise, false.</param>
		/// <exception cref="InvalidOperationException">Thrown if an achievement with the same mod ID and achievement ID has already been registered.</exception>
		public void RegisterAchievement(string modId, string achievementId, string name, string description, int? maxProgress = null, bool isSecret = false)
		{
			if (GetDefinition(modId, achievementId) != null)
				throw new InvalidOperationException($"Achievement '{achievementId}' already registered for mod '{modId}'");

			Definitions.Add(new Definition()
			{
				ModId = modId,
				AchievementId = achievementId,
				Name = name,
				Description = description,
				MaxProgress = maxProgress,
				IsSecret = isSecret
			});
		}

		/// <summary>
		/// Increases the progress of the specified achievement by the given amount, unlocking it if the progress meets or
		/// exceeds the maximum required.
		/// </summary>
		/// <remarks>If the achievement's progress reaches or exceeds its maximum, the achievement is marked as
		/// unlocked. If the achievement does not have a maximum progress value, it is unlocked immediately.</remarks>
		/// <param name="modId">The unique identifier of the mod that contains the achievement.</param>
		/// <param name="achievementId">The unique identifier of the achievement to update.</param>
		/// <param name="amount">The amount by which to increase the achievement's progress. Defaults to 1. Must be a positive integer.</param>
		/// <exception cref="KeyNotFoundException">Thrown if the specified achievement or its definition cannot be found for the given mod.</exception>
		public void AddProgress(string modId, string achievementId, int amount = 1)
		{
			var achievement = GetAchievement(modId, achievementId) ?? 
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			var definition = GetDefinition(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement definition '{achievementId}' not found for mod '{modId}'");

			int progress = achievement.Progress ?? 0;
			if (definition.MaxProgress != null)
				achievement.Progress = Math.Min(progress + amount, definition.MaxProgress.Value);

			if ((definition.MaxProgress == null || achievement.Progress > definition.MaxProgress) && !achievement.IsUnlocked)
			{
				achievement.IsUnlocked = true;
				achievement.UnlockedAt = DateTime.Now;
				achievement.UnlockedSaveName = mainscript.M?.DFMS?.savefilename ?? "";
			}
			SaveUtilities.Upsert(achievement);
		}

		/// <summary>
		/// Unlocks the specified achievement for the given mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement to unlock. Cannot be null or empty.</param>
		/// <param name="achievementId">The unique identifier of the achievement to unlock. Cannot be null or empty.</param>
		/// <exception cref="KeyNotFoundException">Thrown if the specified achievement or its definition does not exist for the given mod.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the specified achievement is a progress-based achievement. Use AddProgress to increment progress
		/// instead.</exception>
		public void UnlockAchievement(string modId, string achievementId)
		{
			var achievement = GetAchievement(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			var definition = GetDefinition(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement definition '{achievementId}' not found for mod '{modId}'");

			if (definition.MaxProgress != null)
				throw new InvalidOperationException(
					$"Cannot unlock progress achievement '{achievementId}'. Use AddProgress instead.");

			AddProgress(modId, achievementId, 1);
		}

		/// <summary>
		/// Determines whether the specified achievement is unlocked for the given mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement.</param>
		/// <param name="achievementId">The unique identifier of the achievement to check.</param>
		/// <returns>true if the specified achievement is unlocked; otherwise, false.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found for the given modId.</exception>
		public bool IsUnlocked(string modId, string achievementId)
		{
			var achievement = GetAchievement(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			return achievement.IsUnlocked;
		}

		/// <summary>
		/// Retrieves the achievement state for the specified achievement within the given mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement. Cannot be null or empty.</param>
		/// <param name="achievementId">The unique identifier of the achievement to retrieve. Cannot be null or empty.</param>
		/// <returns>A value indicating the current state of the specified achievement.</returns>
		internal static State GetAchievement(string modId, string achievementId)
		{
			return SaveUtilities.GetAchievement(modId, achievementId);
		}

		/// <summary>
		/// Retrieves the achievement definition that matches the specified mod and achievement identifiers.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod to search for. Cannot be null.</param>
		/// <param name="achievementId">The unique identifier of the achievement to search for. Cannot be null.</param>
		/// <returns>The matching achievement definition if found; otherwise, null.</returns>
		internal static Definition GetDefinition(string modId, string achievementId)
		{
			foreach (var achievement in Definitions)
			{
				if (achievement.ModId == modId && achievement.AchievementId == achievementId)
					return achievement;
			}
			return null;
		}

		private static void BuildData()
		{
			_achievementsMissingDefinition = 0;

			foreach (var definition in Definitions)
			{
				bool exists = false;
				foreach (var data in Data)
				{
					if (definition.ModId == data.ModId && definition.AchievementId == data.AchievementId)
					{
						exists = true;
						break;
					}
				}
				
				if (!exists)
				{
					Data.Add(new AchievementData(
						definition.ModId,
						definition.AchievementId,
						definition,
						GetAchievement(definition.ModId, definition.AchievementId)
					));
				}
			}

			// Check states for any achievements that haven't been defined yet.
			var states = SaveUtilities.GetAchievements();
			foreach (var state in states)
			{
				bool exists = false;
				foreach (var data in Data)
				{
					if (state.ModId == data.ModId && state.AchievementId == data.AchievementId)
					{
						exists = true;
						break;
					}
				}

				if (!exists)
				{
					_achievementsMissingDefinition++;
				}
			}
		}

		private void ToggleUI(bool? force = null)
		{
			if (force.HasValue)
				_showUI = force.Value;
			else
				_showUI = !_showUI;
		}

		public override void Update()
		{
			if (IsOnMenu && _hasPostLoaded)
				_hasPostLoaded = false;

			// Build achievement data once all mods are fully loaded to avoid missing any.
			if (!IsOnMenu && !_hasPostLoaded)
			{
				BuildData();
				_hasPostLoaded = true;
			}

			if (_showUI && !(mainscript.M?.menu?.Menu?.activeSelf ?? false) && Input.GetButtonDown("Cancel"))
				ToggleUI(false);
		}

		public override void OnGUI()
		{
			Styling.Bootstrap();
			GUI.skin = Styling.GetSkin();

			// TODO: Proper way of opening the UI.
			if ((IsOnMenu || IsPaused) && GUI.Button(new Rect(0, 0, 200, 25), "Achievements manager"))
				ToggleUI();

			if (_showUI)
				RenderUI();

			// Reset back to default Unity skin to avoid styling bleeding to other UI mods.
			GUI.skin = null;
		}

		private void RenderUI()
		{
			// Don't show UI when on main menu if settings or save screens are active.
			if (IsOnMenu && (mainmenuscript.mainmenu.SettingsScreenObj.activeSelf || mainmenuscript.mainmenu.SaveScreenObj.activeSelf))
				return;
			// Don't show UI when in game if pause menu is not active.
			else if (!IsOnMenu && !IsPaused)
			{
				ToggleUI(false);
				return;
			}

			float width = Screen.width * 0.25f;
			float height = Screen.height * 0.75f;
			float x = Screen.width / 2 - width / 2;
			float y = Screen.height / 2 - height / 2;

			GUILayout.BeginArea(new Rect(x, y, width, height), $"<color=#f87ffa><size=18><b>{Name}</b></size>\n<size=16>Made with ❤️ by {Author}</size></color>", "box");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("X"))
			{
				ToggleUI(false);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginVertical();

			GUILayout.BeginScrollView(_scrollPosition);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label($"{SaveUtilities.GetUnlockedCount()} / {Definitions.Count} unlocked", "LabelSubHeader");
			GUILayout.EndHorizontal();
			foreach (var achievement in Data)
			{
				GUILayout.BeginVertical(achievement.State.IsUnlocked ? "box" : "BoxDark");
				if (achievement.Definition.IsSecret && !achievement.State.IsUnlocked)
				{
					GUILayout.Label("Secret achievement", "LabelSubHeader");
					GUILayout.Label("Details will reveal when unlocked");
				}
				else
				{
					GUILayout.Label(achievement.Definition.Name, "LabelSubHeader");
					GUILayout.Label(achievement.Definition.Description);
				}
				GUILayout.BeginHorizontal("box");
				GUILayout.Label($"Provided by: {achievement.ModId}");
				GUILayout.FlexibleSpace();
				GUILayout.Label(achievement.State.IsUnlocked ? "<color=#0F0>Unlocked</color>" : "<color=#F00>Locked</color>");
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
			}

			GUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			if (_achievementsMissingDefinition > 0)
				GUILayout.Label($"{_achievementsMissingDefinition} undefined stored achievements", "LabelCenter");
			GUILayout.Label($"<color=#f87ffa><size=16>v{Version}</size></color>", "LabelCenter");
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}
}
