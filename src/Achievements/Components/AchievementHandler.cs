using Achievements.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static mainscript;

namespace Achievements.Components
{
	/// <summary>
	/// Handles the unlocking of achievements.
	/// </summary>
	internal class AchievementHandler : MonoBehaviour
	{
		private const float TICK_RATE = 1f;
		private float _nextTick = 0;
		private int _tickCount = 0;

		public void OnLoad()
		{
			bool existingSave = mainscript.M.DFMS?.load ?? false;
			if (!existingSave)
			{
				Achievements.UnlockAchievement("getting_started");
				Achievements.AddProgress("new_game_100");
			}
		}

		private void Start()
		{
			// Check for any global distance not accounted for.
			float existingDistance = mainscript.DistanceRead();
			if (existingDistance <= 0f) return;

			int current5000 = Achievements.GetProgress(Achievements.I.ID, "drive_5000_global");
			int current10000 = Achievements.GetProgress(Achievements.I.ID, "drive_10000_global");
			int current25000 = Achievements.GetProgress(Achievements.I.ID, "drive_25000_global");
			int current50000 = Achievements.GetProgress(Achievements.I.ID, "drive_50000_global");

			// Only seed if stored progress is behind real distance, handles reinstalls etc.
			int distance = Mathf.FloorToInt(existingDistance);
			if (distance > current5000)
				Achievements.AddProgress(Achievements.I.ID, "drive_5000_global", distance - current5000);
			if (distance > current10000)
				Achievements.AddProgress(Achievements.I.ID, "drive_10000_global", distance - current10000);
			if (distance > current25000)
				Achievements.AddProgress(Achievements.I.ID, "drive_25000_global", distance - current25000);
			if (distance > current50000)
				Achievements.AddProgress(Achievements.I.ID, "drive_50000_global", distance - current50000);
		}

		private void Update()
		{
			_nextTick -= Time.unscaledDeltaTime;
			if (_nextTick <= 0)
			{
				_tickCount++;
				Tick();
				_nextTick = TICK_RATE;

				if (_tickCount > 900)
					_tickCount = 0;
			}
		}

		private void Tick()
		{
			if (!Achievements.IsOnMenu)
			{
				DistanceCheck();
				SpeedCheck();

				if (_tickCount % 5 == 0)
				{
					FlipCheck();
				}

				if (_tickCount % 10 == 0)
				{
					VehicleConditionCheck();
					FluidCheck();
				}
			}
		}

		private void DistanceCheck()
		{
			if (Achievements.IsUnlocked("drive_5000_save") && Achievements.IsUnlocked("drive_10000_save")) return;

			float distance = savedatascript.d.properties.driven;
			if (distance >= 5000f)
				Achievements.UnlockAchievement("drive_5000_save");
			if (distance >= 10000f)
				Achievements.UnlockAchievement("drive_10000_save");
		}

		private void SpeedCheck()
		{
			if (Achievements.IsUnlocked("speed_cap")) return;

			carscript car = mainscript.M?.player?.Car;
			if (car == null) return;

			if (car.speed >= 719f)
				Achievements.UnlockAchievement("speed_cap");
		}

		private void VehicleConditionCheck()
		{
			if (Achievements.IsUnlocked("vehicle_perfect") && Achievements.IsUnlocked("vehicle_rusty")) return;

			carscript car = mainscript.M?.player?.Car;
			if (car == null) return;
			partconditionscript root = car.gameObject.GetComponent<partconditionscript>();

			List<partconditionscript> parts = new List<partconditionscript>();
			GameUtilities.FindPartChildren(root, ref parts);

			bool allRusty = true;
			bool allPristine = true;
			foreach (var part in parts)
			{
				if (part.state != 0)
					allPristine = false;

				if (part.state != 4)
					allRusty = false;
			}

			if (allPristine)
				Achievements.UnlockAchievement("vehicle_perfect");

			if (allRusty)
				Achievements.UnlockAchievement("vehicle_rusty");
		}

		private void FluidCheck()
		{
			if (Achievements.IsUnlocked("full_tank")) return;

			carscript car = mainscript.M?.player?.Car;
			if (car?.Tank == null) return;
			var engine = car?.Engine;
			if (engine == null) return;

			List<mainscript.fluidenum> fluids = new List<mainscript.fluidenum>();
			foreach (fluid fluid in engine.FuelConsumption.fluids)
			{
				fluids.Add(fluid.type);
			}

			if (car.Tank.F.GetAmount() >= car.Tank.F.maxC)
			{
				bool fluidMatch = true;
				foreach (var fluid in car.Tank.F.fluids)
				{
					if (!fluids.Contains(fluid.type))
						fluidMatch = false;
				}

				if (fluidMatch)
					Achievements.UnlockAchievement("full_tank");
			}
		}

		private void FlipCheck()
		{
			if (Achievements.IsUnlocked("flip_vehicle")) return;

			carscript car = mainscript.M?.player?.Car;
			if (car?.RB == null) return;

			if (car.transform.up.y < -0.5f && car.RB.velocity.magnitude < 0.5f)
				Achievements.UnlockAchievement("flip_vehicle");
		}
	}
}
