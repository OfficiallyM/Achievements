using System.Collections.Generic;
using UnityEngine;

namespace Achievements.Utilities.UI
{
	public static class Animator
	{
		private static Dictionary<string, float> _offsets = new Dictionary<string, float>();
		private static Dictionary<string, float> _elapsed = new Dictionary<string, float>();
		private static Dictionary<string, AnimationState> _states = new Dictionary<string, AnimationState>();

		public enum SlideDirection { Left, Right, Top, Bottom }
		public enum AnimationState { Idle, SlideIn, SlideOut }

		/// <summary>
		/// Play the specified animation for a given key.
		/// </summary>
		/// <param name="key">The unique identifier for the animation. Cannot be null.</param>
		/// <param name="state">Animation state.</param>
		public static void Play(string key, AnimationState state)
		{
			_states[key] = state;
			_elapsed[key] = 0f;
		}

		/// <summary>
		/// Resets all stored state associated with the specified key.
		/// </summary>
		/// <param name="key">The unique identifier for the animation state to reset. Cannot be null.</param>
		public static void Reset(string key)
		{
			_offsets.Remove(key);
			_elapsed.Remove(key);
			_states.Remove(key);
		}

		/// <summary>
		/// Determines whether the animation has finished.
		/// </summary>
		/// <param name="key">The unique identifier for the animation whose state is to be checked. Cannot be null.</param>
		/// <returns>true if the animation state for the specified key is idle or the key does not exist; otherwise, false.</returns>
		public static bool IsIdle(string key)
		{
			return !_states.ContainsKey(key) || _states[key] == AnimationState.Idle;
		}

		/// <summary>
		/// Calculates the current position of a rectangle as it slides in or out of view in the specified direction, using a
		/// unique animation key.
		/// </summary>
		/// <remarks>Call this method each frame to animate the rectangle's sliding motion. The animation state is
		/// managed internally using the provided key, allowing multiple rectangles to be animated independently. The returned
		/// rectangle should be used for rendering or layout during the animation.</remarks>
		/// <param name="key">A unique identifier for the sliding animation instance. Used to track the animation state and progress for this
		/// rectangle.</param>
		/// <param name="targetRect">The target rectangle to animate. Specifies the position and size of the rectangle when fully visible.</param>
		/// <param name="direction">The direction in which the rectangle should slide. Determines the axis and direction of the animation.</param>
		/// <param name="speed">The speed at which the rectangle slides, where higher values result in faster animations. The default is 1.5.</param>
		/// <returns>A Rect representing the current position of the animated rectangle, adjusted according to the sliding animation's
		/// progress.</returns>
		public static Rect Slide(string key, Rect targetRect, SlideDirection direction, float speed = 1.5f)
		{
			float slideDistance;
			switch (direction)
			{
				case SlideDirection.Left: slideDistance = targetRect.x + targetRect.width; break;
				case SlideDirection.Right: slideDistance = Screen.width - targetRect.x; break;
				case SlideDirection.Top: slideDistance = targetRect.y + targetRect.height; break;
				case SlideDirection.Bottom: slideDistance = Screen.height - targetRect.y; break;
				default: slideDistance = 0f; break;
			}

			if (!_offsets.ContainsKey(key))
				_offsets[key] = slideDistance;

			if (!_elapsed.ContainsKey(key))
				_elapsed[key] = 0f;

			if (!_states.ContainsKey(key))
				_states[key] = AnimationState.Idle;

			// Tick elapsed.
			_elapsed[key] += Time.unscaledDeltaTime;

			float target;
			switch (_states[key])
			{
				case AnimationState.SlideOut:
					target = slideDistance;
					break;
				// SlideIn + Idle.
				default:
					target = 0f;
					break;
			}

			_offsets[key] = Mathf.Lerp(_offsets[key], target, 1f - Mathf.Pow(0.01f, Time.unscaledDeltaTime * speed));

			float offset = _offsets[key];
			Rect result;
			switch (direction)
			{
				case SlideDirection.Left: result = new Rect(targetRect.x - offset, targetRect.y, targetRect.width, targetRect.height); break;
				case SlideDirection.Right: result = new Rect(targetRect.x + offset, targetRect.y, targetRect.width, targetRect.height); break;
				case SlideDirection.Top: result = new Rect(targetRect.x, targetRect.y - offset, targetRect.width, targetRect.height); break;
				case SlideDirection.Bottom: result = new Rect(targetRect.x, targetRect.y + offset, targetRect.width, targetRect.height); break;
				default: result = targetRect; break;
			}

			// Transition to idle.
			if (_states[key] != AnimationState.Idle && Mathf.RoundToInt(offset) == Mathf.RoundToInt(target))
				_states[key] = AnimationState.Idle;

			return result;
		}
	}
}
