using Achievements.Components;
using Achievements.Core;
using Achievements.Extensions;
using Achievements.Utilities;
using Achievements.Utilities.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TLDLoader;
using UnityEngine;

namespace Achievements
{
	public class Achievements : Mod
	{
		// Mod meta stuff.
		private string _version = "1.0.0";
		public override string ID => "M_Achievements";
		public override string Name => "Achievements";
		public override string Author => "M-";
		public override string Version => _version;
		public override bool UseLogger => true;
		public override bool LoadInMenu => true;
		public override bool UseHarmony => true;

		internal static Achievements I;
		internal static AchievementHandler Handler;
		internal static List<Definition> Definitions = new List<Definition>();
		internal static List<AchievementData> Data = new List<AchievementData>();
		internal static Preferences Preferences = new Preferences();
		internal static bool Debug = false;
		internal static bool IsOnMenu => mainscript.M == null;
		internal static bool IsPaused
		{
			get
			{
				var menu = mainscript.M?.menu?.Menu;
				return menu != null && menu.activeSelf;
			}
		}

		/// <summary>
		/// Occurs when an achievement state is updated.
		/// </summary>
		public static event Action<State> OnStateChange;
		/// <summary>
		/// Occurs when the progress of an achievement is updated.
		/// </summary>
		public static event Action<State> OnAchievementProgress;
		/// <summary>
		/// Occurs when an achievement is unlocked, providing the current state information.
		/// </summary>
		public static event Action<State> OnAchievementUnlock;

		private const float NOTIFICATION_WIDTH = 300f;
		private const float NOTIFICATION_HEIGHT = 100f;
		private const float DISPLAY_DURATION = 7f;

		private bool _showUI = false;
		private string[] _tabs = { "Achievements", "Preferences" };
		private int _activeTab = 0;
		private AudioSource _audioSource;
		private static int _achievementsMissingDefinition = 0;
		private Vector2 _scrollPosition = Vector2.zero;

		private Queue<Notification> _notificationQueue = new Queue<Notification>();
		private Notification _renderingNotification;

		private List<AchievementData> _filteredData;
		private string[] _sortOptions = { "Default", "Unlocked", "A-Z", "Progress" };
		private string _searchQuery;
		private string _lastSearchQuery;
		private int _sortIndex;
		private int _lastSortIndex;

		private AudioClip _notificationSound;

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
			Preferences = SaveUtilities.GetPreferences();

			OnAchievementUnlock += AchievementUnlock;

			GameObject handler = new GameObject("AchievementHandler");
			Handler = handler.AddComponent<AchievementHandler>();
			GameObject.DontDestroyOnLoad(handler);

			_audioSource = handler.AddComponent<AudioSource>();
			_audioSource.spatialBlend = 0f;
			_audioSource.playOnAwake = false;

			// Oneshot achievements.
			RegisterAchievement("getting_started", "Getting Started", "Start a new game");
			RegisterAchievement("drive_5000_save", "The Long Drive", "Drive 5000 kilometers in a single save");
			RegisterAchievement("vehicle_perfect", "Perfection", "Fully restore every part of a vehicle");
			RegisterAchievement("vehicle_rusty", "Rust Bucket", "Drive a vehicle with every part at the worst condition");
			RegisterAchievement("full_tank", "Full Tank", "Get a full tank of fuel");
			RegisterAchievement("die", "The Bell Tolls", "Get killed");
			RegisterAchievement("flip_vehicle", "Rubber Side Down", "Flip a vehicle");

			// Progress achievements.
			RegisterAchievement("drive_5000_global", "Well Travelled", "Drive 5000 kilometers in total", maxProgress: 5000);
			RegisterAchievement("drive_10000_global", "Long Way From Home", "Drive 10000 kilometers in total", maxProgress: 10000);
			RegisterAchievement("drive_25000_global", "Is Anyone Out There?", "Drive 25000 kilometers in total", maxProgress: 25000);
			RegisterAchievement("munkas_100", "No Mercy", "Kill 100 munkas", maxProgress: 100);
			RegisterAchievement("bunnies_100", "Rabbit Season", "Kill 100 bunnies", maxProgress: 100);

			// Secret achievements.
			RegisterAchievement("drive_10000_save", "The Very Long Drive", "Drive 10000 kilometers in a single save", isSecret: true);
			RegisterAchievement("drive_50000_global", "Are You Lost?", "Drive 50000 kilometers in total", maxProgress: 50000, isSecret: true);
			RegisterAchievement("speed_cap", "Hittin' the limit", "Reach the vehicle speed cap of 719km/h", isSecret: true);
			RegisterAchievement("new_game_100", "Indecisive", "Start a new game 100 times", maxProgress: 100, isSecret: true);

			// Assets.
			AssetBundle bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Achievements)}.achievements"));
			_notificationSound = bundle.LoadAsset<AudioClip>("notification.wav");
			bundle.Unload(false);
		}

		public override void OnLoad()
		{
			Handler.OnLoad();
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
		public static void RegisterAchievement(string modId, string achievementId, string name, string description, int? maxProgress = null, bool isSecret = false)
		{
			if (GetDefinition(modId, achievementId) != null)
				throw new InvalidOperationException($"Achievement '{achievementId}' already registered for mod '{modId}'");

			var definition = new Definition()
			{
				ModId = modId,
				AchievementId = achievementId,
				Name = name,
				Description = description,
				MaxProgress = maxProgress,
				IsSecret = isSecret
			};
			Definitions.Add(definition);

			Data.Add(new AchievementData(
				definition.ModId,
				definition.AchievementId,
				definition,
				GetAchievement(definition.ModId, definition.AchievementId)
			));
		}

		internal void RegisterAchievement(string achievementId, string name, string description, int? maxProgress = null, bool isSecret = false)
		{
			RegisterAchievement(ID, achievementId, name, description, maxProgress, isSecret);
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
		public static void AddProgress(string modId, string achievementId, int amount = 1)
		{
			if (IsUnlocked(modId, achievementId))
				return;

			var achievement = GetAchievement(modId, achievementId) ?? 
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			var definition = GetDefinition(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement definition '{achievementId}' not found for mod '{modId}'");

			int progress = achievement.Progress ?? 0;
			if (definition.MaxProgress != null)
			{
				achievement.Progress = Math.Min(progress + amount, definition.MaxProgress.Value);
				OnAchievementProgress?.Invoke(achievement);
			}

			if ((definition.MaxProgress == null || achievement.Progress >= definition.MaxProgress) && !achievement.IsUnlocked)
			{
				achievement.IsUnlocked = true;
				achievement.UnlockedAt = DateTime.Now;
				OnAchievementUnlock?.Invoke(achievement);
			}
			SaveUtilities.Upsert(achievement);
		}

		internal static void AddProgress(string achievementId, int amount = 1)
		{
			AddProgress(I.ID, achievementId, amount);
		}

		/// <summary>
		/// Unlocks the specified achievement for the given mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement to unlock. Cannot be null or empty.</param>
		/// <param name="achievementId">The unique identifier of the achievement to unlock. Cannot be null or empty.</param>
		/// <exception cref="KeyNotFoundException">Thrown if the specified achievement or its definition does not exist for the given mod.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the specified achievement is a progress-based achievement. Use AddProgress to increment progress
		/// instead.</exception>
		public static void UnlockAchievement(string modId, string achievementId)
		{
			if (IsUnlocked(modId, achievementId))
				return;

			var achievement = GetAchievement(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			var definition = GetDefinition(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement definition '{achievementId}' not found for mod '{modId}'");

			if (definition.MaxProgress != null)
				throw new InvalidOperationException(
					$"Cannot unlock progress achievement '{achievementId}'. Use AddProgress instead.");

			AddProgress(modId, achievementId, 1);
		}

		internal static void UnlockAchievement(string achievementId)
		{
			UnlockAchievement(I.ID, achievementId);
		}

		/// <summary>
		/// Determines whether the specified achievement is unlocked for the given mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement.</param>
		/// <param name="achievementId">The unique identifier of the achievement to check.</param>
		/// <returns>true if the specified achievement is unlocked; otherwise, false.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found for the given modId.</exception>
		public static bool IsUnlocked(string modId, string achievementId)
		{
			var achievement = GetAchievement(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");

			return achievement.IsUnlocked;
		}

		internal static bool IsUnlocked(string achievementId)
		{
			return IsUnlocked(I.ID, achievementId);
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

		/// <summary>
		/// Gets the current progress value for the specified achievement within a mod.
		/// </summary>
		/// <param name="modId">The unique identifier of the mod containing the achievement.</param>
		/// <param name="achievementId">The unique identifier of the achievement whose progress is to be retrieved.</param>
		/// <returns>The progress value of the specified achievement. Returns 0 if the achievement has no recorded progress.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if an achievement with the specified achievementId is not found for the given modId.</exception>
		public static int GetProgress(string modId, string achievementId)
		{
			var achievement = GetAchievement(modId, achievementId) ??
				throw new KeyNotFoundException($"Achievement '{achievementId}' not found for mod '{modId}'");
			return achievement?.Progress ?? 0;
		}

		internal static int GetProgress(string achievementId)
		{
			return GetProgress(I.ID, achievementId);
		}

		internal static void RaiseOnStateChange(State state) 
			=> OnStateChange?.Invoke(state);

		private AchievementData GetData(string modId, string achievementId)
		{
			foreach (var data in Data)
			{
				if (data.ModId == modId && data.AchievementId == achievementId)
					return data;
			}
			return null;
		}

		private AchievementData GetData(State state)
		{
			return GetData(state.ModId, state.AchievementId);
		}

		public void AchievementUnlock(State state)
		{
			_notificationQueue.Enqueue(new Notification()
			{
				Achievement = GetData(state),
			});
		}

		private void RefreshFilteredData()
		{
			var filtered = string.IsNullOrEmpty(_searchQuery)
				? Data
				: Data.Where(a =>
					a.Definition.Name.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
					a.Definition.Description.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
					a.ModId.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
					a.AchievementId.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0);

			switch (_sortIndex)
			{
				// Unlocked - unlocked first, then locked.
				case 1: 
					filtered = filtered.OrderByDescending(a => a.State.IsUnlocked);
					break;
				// A-Z.
				case 2: 
					filtered = filtered.OrderBy(a => a.Definition.Name);
					break;
				// Progress - highest percentage first.
				case 3: 
					filtered = filtered.OrderByDescending(a =>
						a.Definition.MaxProgress.HasValue
							? (float)(a.State.Progress ?? 0) / a.Definition.MaxProgress.Value
							: a.State.IsUnlocked ? 1f : 0f);
					break;
			}

			_filteredData = filtered.ToList();
		}

		private void ToggleUI(bool? force = null)
		{
			_showUI = force ?? !_showUI;

			if (!IsOnMenu)
			{
				if (_showUI && IsPaused)
					mainscript.M.PressedEscape();

				mainscript.M.crsrLocked = !_showUI;
				mainscript.M.SetCursorVisible(_showUI);
				mainscript.M.menu.gameObject.SetActive(!_showUI);
			}

			if (_showUI)
			{
				// Check states for any achievements that haven't been defined yet.
				_achievementsMissingDefinition = 0;
				var states = SaveUtilities.GetAchievements();
				foreach (var state in states)
				{
					if (GetData(state)?.Definition == null)
					{
						_achievementsMissingDefinition++;
					}
				}

				RefreshFilteredData();
				Animator.Play("mainUI", Animator.AnimationState.SlideIn);
			}
			else
			{
				Animator.Play("mainUI", Animator.AnimationState.SlideOut);
			}
		}

		public override void Update()
		{
			if (_showUI && Input.GetButtonDown("Cancel"))
				ToggleUI(false);
		}

		public override void OnGUI()
		{
			Styling.Bootstrap();
			GUI.skin = Styling.GetSkin();

			bool menuCondition = IsOnMenu && !mainmenuscript.mainmenu.SettingsScreenObj.activeSelf && !mainmenuscript.mainmenu.SaveScreenObj.activeSelf;
			if ((menuCondition || IsPaused) && GUI.Button(new Rect(Screen.width * 0.70f, 10f, 200, 50), "Achievements Manager"))
				ToggleUI();

			if (_showUI || !Animator.IsIdle("mainUI"))
				RenderUI();

			RenderNotifications();

			// Reset back to default Unity skin to avoid styling bleeding to other UI mods.
			GUI.skin = null;
		}

		private void RenderUI()
		{
			// Don't show UI when on main menu if settings or save screens are active.
			if (IsOnMenu && (mainmenuscript.mainmenu.SettingsScreenObj.activeSelf || mainmenuscript.mainmenu.SaveScreenObj.activeSelf))
				return;

			float width = Screen.width * 0.25f;
			float height = Screen.height * 0.75f;
			float x = Screen.width / 2 - width / 2;
			float y = Screen.height / 2 - height / 2;
			Rect targetRect = new Rect(x, y, width, height);
			Rect animatedRect = Animator.Slide("mainUI", targetRect, Animator.SlideDirection.Bottom);

			if (!_showUI && Animator.IsIdle("mainUI"))
				return;

			GUILayout.BeginArea(animatedRect, $"<color=#f87ffa><size=18><b>{Name}</b></size>\n<size=16>Made with ❤️ by {Author}</size></color>", "box");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("X"))
			{
				ToggleUI(false);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Space(5);
			for (int i = 0; i < _tabs.Length; i++)
			{
				if (GUILayout.Button(_tabs[i], _activeTab == i ? "ButtonSecondary" : "button"))
					_activeTab = i;
			}
			GUILayout.Space(5);
			GUILayout.EndHorizontal();

			switch (_activeTab)
			{
				case 0: RenderAchievementsTab(); break;
				case 1: RenderPreferencesTab(); break;
			}

			GUILayout.FlexibleSpace();
			if (_achievementsMissingDefinition > 0 && _activeTab == 0)
				GUILayout.Label($"{_achievementsMissingDefinition} unregistered achievements", "LabelCenter");
			GUILayout.Label($"<color=#f87ffa><size=16>v{Version}</size></color>", "LabelCenter");
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void RenderAchievementsTab()
		{
			GUILayout.Label($"{SaveUtilities.GetUnlockedCount()} / {Data.Count + _achievementsMissingDefinition} unlocked", "LabelSubHeader");

			GUILayout.BeginHorizontal();
			_searchQuery = GUILayout.TextField(_searchQuery);
			if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
				_searchQuery = string.Empty;
			GUILayout.Space(10);

			if (GUILayout.Button($"Sort: {_sortOptions[_sortIndex]}", GUILayout.MaxWidth(125)))
				_sortIndex = (_sortIndex + 1) % _sortOptions.Length;

			// Check for search/sorting change.
			if (_searchQuery != _lastSearchQuery || _sortIndex != _lastSortIndex)
			{
				RefreshFilteredData();
				_lastSearchQuery = _searchQuery;
				_lastSortIndex = _sortIndex;
			}
			GUILayout.EndHorizontal();
			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			foreach (var achievement in _filteredData)
			{
				GUILayout.BeginVertical(achievement.State.IsUnlocked ? "box" : "BoxDark");
				if (achievement.Definition.IsSecret && !achievement.State.IsUnlocked)
				{
					GUILayout.Label("Secret Achievement", "LabelSubHeader");
					GUILayout.Label("Details will reveal when unlocked");
				}
				else
				{
					GUILayout.Label(achievement.Definition.Name, "LabelSubHeader");
					GUILayout.Label(achievement.Definition.Description);
				}

				// Render progress bar.
				if (achievement.Definition.MaxProgress != null)
				{
					float progress = (float)(achievement.State.Progress ?? 0) / achievement.Definition.MaxProgress.Value;

					GUILayout.BeginHorizontal();
					Rect progressRect = GUILayoutUtility.GetRect(200, 20, GUILayout.ExpandWidth(true));

					// Background.
					GUI.DrawTexture(progressRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.2f, 0.2f, 0.2f, 1f), 0, 0);

					// Fill.
					Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
					GUI.DrawTexture(fillRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0f, 0.8f, 0f, 1f), 0, 0);

					// Text overlay.
					if (!achievement.Definition.IsSecret || (achievement.Definition.IsSecret && achievement.State.IsUnlocked))
						GUI.Label(progressRect, $"{achievement.State.Progress ?? 0} / {achievement.Definition.MaxProgress}", "LabelCenter");

					GUILayout.EndHorizontal();
				}

				GUILayout.BeginHorizontal("box");
				GUILayout.Label($"Provided by: {achievement.ModId}");
				GUILayout.FlexibleSpace();
				if (achievement.State.IsUnlocked && achievement.State.UnlockedAt.HasValue)
				{
					GUILayout.Label(achievement.State.UnlockedAt.Value.ToString("dd/MM/yyyy HH:mm"));
					GUILayout.Space(10);
				}
				GUILayout.Label(achievement.State.IsUnlocked ? "<color=#0F0>Unlocked</color>" : "<color=#F00>Locked</color>");
				GUILayout.EndHorizontal();

				if (Debug)
				{
					GUILayout.BeginVertical("BoxDark");
					GUILayout.Label("Debug settings", "LabelCenter");
					GUILayout.BeginHorizontal();
					if (achievement.Definition.MaxProgress != null)
					{
						int progress = achievement.State.Progress ?? 0;
						if (GUILayout.Button("-", GUILayout.MaxWidth(30)))
						{
							achievement.State.Progress = Mathf.Clamp(progress - 1, 0, achievement.Definition.MaxProgress.Value);
							SaveUtilities.Upsert(achievement.State);
						}

						if (GUILayout.Button("+", GUILayout.MaxWidth(30)))
						{
							AddProgress(achievement.ModId, achievement.AchievementId);
						}
					}
					GUILayout.FlexibleSpace();

					if (GUILayout.Button(achievement.State.IsUnlocked ? "Re-lock" : "Unlock", GUILayout.ExpandWidth(false)))
					{
						achievement.State.IsUnlocked = !achievement.State.IsUnlocked;
						if (!achievement.State.IsUnlocked)
						{
							achievement.State.UnlockedAt = null;
							achievement.State.Progress = null;
						}
						else
						{
							OnAchievementUnlock?.Invoke(achievement.State);
							achievement.State.Progress = achievement.Definition.MaxProgress;
							achievement.State.UnlockedAt = DateTime.Now;
						}
						SaveUtilities.Upsert(achievement.State);
					}
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
				GUILayout.EndVertical();
			}

			GUILayout.EndScrollView();
		}

		private void RenderPreferencesTab()
		{
			GUILayout.Space(10);
			Preferences.NotificationSound = GUILayout.Toggle(Preferences.NotificationSound, "Play achievement unlock sound");
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save preferences", GUILayout.ExpandWidth(false)))
				SaveUtilities.SavePreferences();
			GUILayout.Space(5);
			GUILayout.EndHorizontal();
		}

		private void RenderNotifications()
		{
			if (_renderingNotification == null)
			{
				if (_notificationQueue.Count == 0) return;

				_renderingNotification = _notificationQueue.Dequeue();
				_renderingNotification.StartDisplayTime = Time.unscaledTime;
				Animator.Play("notification", Animator.AnimationState.SlideIn);
				if (Preferences.NotificationSound)
					_audioSource.PlayOneShot(_notificationSound);
				return;
			}

			float elapsed = Time.unscaledTime - _renderingNotification.StartDisplayTime.Value;

			// Remove once animation finishes.
			if (elapsed >= DISPLAY_DURATION && Animator.IsIdle("notification"))
			{
				_renderingNotification = null;
				Animator.Reset("notification");
				return;
			}
			// Trigger slide out in last second.
			else if (elapsed >= DISPLAY_DURATION - 1f && Animator.IsIdle("notification"))
				Animator.Play("notification", Animator.AnimationState.SlideOut);

			try
			{
				Rect targetRect = new Rect(Screen.width - NOTIFICATION_WIDTH - 10f, Screen.height - NOTIFICATION_HEIGHT - 10f, NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
				Rect animatedRect = Animator.Slide("notification", targetRect, Animator.SlideDirection.Right);
				GUILayout.BeginArea(animatedRect, "", "BoxDark");
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal("BoxDark");
				GUILayout.Label("Achievement Unlocked", "LabelHeaderCenter");
				GUILayout.EndHorizontal();
				GUILayout.Label(_renderingNotification.Achievement.Definition.Name, "LabelSubHeaderCenter");
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			catch { }
		}
	}
}
