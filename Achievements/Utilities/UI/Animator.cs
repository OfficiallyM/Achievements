using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Achievements.Utilities.UI
{
	public static class Animator
	{
		private static Dictionary<string, float> _offsets = new Dictionary<string, float>();

		public enum SlideDirection { Left, Right, Top, Bottom }
		public enum SlideMode { In, Out, InOut }

		/// <summary>
		/// Calculates and returns a rectangle that animates sliding in or out of view based on the specified direction, mode,
		/// and timing parameters.
		/// </summary>
		/// <remarks>This method is typically called each frame to animate a rectangle sliding in or out of view. The
		/// returned rectangle should be used for rendering during the animation. The sliding state is tracked using the
		/// provided key, allowing multiple independent slide animations. The animation uses unscaled delta time, so it is not
		/// affected by changes to the global time scale.</remarks>
		/// <param name="key">A unique identifier used to track the sliding state of the rectangle across frames. Must not be null or empty.</param>
		/// <param name="targetRect">The target rectangle to be animated. Defines the final position and size of the sliding rectangle.</param>
		/// <param name="duration">The total duration, in seconds, of the slide animation. Must be greater than zero.</param>
		/// <param name="elapsed">The elapsed time, in seconds, since the start of the slide animation.</param>
		/// <param name="direction">The direction in which the rectangle should slide. Determines the axis and direction of movement.</param>
		/// <param name="mode">The slide mode, specifying whether the rectangle slides in, out, or both (in and out).</param>
		/// <param name="speed">The speed factor controlling how quickly the rectangle interpolates toward its target position. Higher values
		/// result in faster sliding. Defaults to 8 if not specified.</param>
		/// <returns>A new rectangle representing the current position of the animated slide, based on the elapsed time and specified
		/// parameters.</returns>
		public static Rect Slide(string key, Rect targetRect, float duration, float elapsed, SlideDirection direction, SlideMode mode, float speed = 8f)
		{
			float slideDistance;
			switch (direction)
			{
				case SlideDirection.Left: 
					slideDistance = targetRect.x + targetRect.width; 
					break;
				case SlideDirection.Right: 
					slideDistance = Screen.width - targetRect.x; 
					break;
				case SlideDirection.Top: 
					slideDistance = targetRect.y + targetRect.height; 
					break;
				case SlideDirection.Bottom: 
					slideDistance = Screen.height - targetRect.y; 
					break;
				default: 
					slideDistance = 0f; 
					break;
			}

			float target;
			switch (mode)
			{
				case SlideMode.Out: 
					target = slideDistance; 
					break;
				case SlideMode.InOut: 
					target = elapsed >= duration - 1f ? slideDistance : 0f; 
					break;
				// SlideMode.In.
				default: 
					target = 0f; 
					break; 
			}

			if (!_offsets.ContainsKey(key))
				_offsets[key] = slideDistance;

			_offsets[key] = Mathf.Lerp(_offsets[key], target, Time.unscaledDeltaTime * speed);

			float offset = _offsets[key];
			switch (direction)
			{
				case SlideDirection.Left: 
					return new Rect(targetRect.x - offset, targetRect.y, targetRect.width, targetRect.height);
				case SlideDirection.Right: 
					return new Rect(targetRect.x + offset, targetRect.y, targetRect.width, targetRect.height);
				case SlideDirection.Top: 
					return new Rect(targetRect.x, targetRect.y - offset, targetRect.width, targetRect.height);
				case SlideDirection.Bottom: 
					return new Rect(targetRect.x, targetRect.y + offset, targetRect.width, targetRect.height);
				default: 
					return targetRect;
			}
		}

		/// <summary>
		/// Removes the offset associated with the specified key, resetting it to its default state.
		/// </summary>
		/// <param name="key">The key whose offset should be reset. Cannot be null.</param>
		public static void Reset(string key)
		{
			if (_offsets.ContainsKey(key))
				_offsets.Remove(key);
		}
	}
}
