using Achievements.Extensions;
using UnityEngine;

namespace Achievements.Utilities.UI
{
	/// <summary>
	/// Stripped back version of the M-ultiTool theme system.
	/// </summary>
	internal static class Styling
	{
		private static bool _hasInitialised = false;
		private static GUISkin _skin;

		private static Texture2D _black;
		private static Texture2D _blackHover;
		private static Texture2D _white;
		private static Texture2D _whiteHover;
		private static Texture2D _transparent;
		private static Texture2D _buttonPrimary;
		private static Texture2D _buttonPrimaryHover;
		private static Texture2D _buttonSecondary;
		private static Texture2D _buttonSecondaryHover;
		private static Texture2D _box;
		private static Texture2D _boxHover;
		private static Texture2D _boxDark;

		public static GUISkin GetSkin() => _skin;

		public static void Bootstrap()
		{
			if (!_hasInitialised)
			{
				CreateSkin(GUI.skin);

				// Create any required core textures.
				_black = GUIExtensions.ColorTexture(1, 1, new Color(0f, 0f, 0f));
				_blackHover = GUIExtensions.ColorTexture(1, 1, new Color(0.1f, 0.1f, 0.1f));
				_white = GUIExtensions.ColorTexture(1, 1, new Color(1f, 1f, 1f));
				_whiteHover = GUIExtensions.ColorTexture(1, 1, new Color(0.9f, 0.9f, 0.9f));
				_transparent = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0));

				_buttonPrimary = GUIExtensions.ColorTexture(1, 1, new Color(0.4f, 0.4f, 0.4f));
				_buttonPrimaryHover = GUIExtensions.ColorTexture(1, 1, new Color(0.5f, 0.5f, 0.5f));
				_buttonSecondary = GUIExtensions.ColorTexture(1, 1, new Color(0.15f, 0.15f, 0.15f));
				_buttonSecondaryHover = GUIExtensions.ColorTexture(1, 1, new Color(0.25f, 0.25f, 0.25f));
				_box = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0.4f));
				_boxHover = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0.5f));
				_boxDark = GUIExtensions.ColorTexture(1, 1, new Color(0, 0, 0, 0.6f));

				Color buttonPrimaryTextColour = Color.white;
				Color buttonSecondaryTextColour = Color.white;
				Color textColour = Color.white;

				// Override scrollbar width and height.
				_skin.verticalScrollbar.fixedWidth = _skin.verticalScrollbarThumb.fixedWidth = _skin.horizontalScrollbar.fixedHeight = _skin.horizontalScrollbarThumb.fixedHeight = 8f;

				// Button styling.
				GUIStyle buttonStyle = new GUIStyle(_skin.button);
				buttonStyle.padding = new RectOffset(10, 10, 5, 5);

				GUIStyle buttonPrimary = new GUIStyle(buttonStyle);
				buttonPrimary.name = "ButtonPrimary";
				buttonPrimary.normal.background = _buttonPrimary;
				buttonPrimary.hover.background = _buttonPrimaryHover;
				buttonPrimary.active.background = _buttonPrimaryHover;
				buttonPrimary.focused.background = _buttonPrimaryHover;
				buttonPrimary.normal.textColor = buttonPrimaryTextColour;
				buttonPrimary.hover.textColor = buttonPrimaryTextColour;
				buttonPrimary.active.textColor = buttonPrimaryTextColour;
				buttonPrimary.focused.textColor = buttonPrimaryTextColour;

				// Default to use primary button.
				_skin.button = buttonPrimary;

				GUIStyle buttonPrimaryWrap = new GUIStyle(buttonPrimary);
				buttonPrimaryWrap.name = "ButtonPrimaryWrap";
				buttonPrimaryWrap.wordWrap = true;

				GUIStyle buttonPrimaryTextLeft = new GUIStyle(buttonPrimary);
				buttonPrimaryTextLeft.name = "ButtonPrimaryTextLeft";
				buttonPrimaryTextLeft.alignment = TextAnchor.MiddleLeft;

				GUIStyle buttonSecondary = new GUIStyle(buttonStyle);
				buttonSecondary.name = "ButtonSecondary";
				buttonSecondary.normal.background = _buttonSecondary;
				buttonSecondary.hover.background = _buttonSecondaryHover;
				buttonSecondary.active.background = _buttonSecondaryHover;
				buttonSecondary.focused.background = _buttonSecondaryHover;
				buttonSecondary.normal.textColor = buttonSecondaryTextColour;
				buttonSecondary.hover.textColor = buttonSecondaryTextColour;
				buttonSecondary.active.textColor = buttonSecondaryTextColour;
				buttonSecondary.focused.textColor = buttonSecondaryTextColour;

				GUIStyle buttonSecondaryTextLeft = new GUIStyle(buttonSecondary);
				buttonSecondaryTextLeft.name = "ButtonSecondaryTextLeft";
				buttonSecondaryTextLeft.alignment = TextAnchor.MiddleLeft;

				GUIStyle buttonBlack = new GUIStyle(buttonStyle);
				buttonBlack.name = "ButtonBlack";
				buttonBlack.normal.background = _black;
				buttonBlack.hover.background = _blackHover;
				buttonBlack.active.background = _blackHover;
				buttonBlack.focused.background = _blackHover;

				GUIStyle buttonWhite = new GUIStyle(buttonStyle);
				buttonWhite.name = "ButtonWhite";
				buttonWhite.normal.background = _white;
				buttonWhite.hover.background = _whiteHover;
				buttonWhite.active.background = _whiteHover;
				buttonWhite.focused.background = _whiteHover;

				GUIStyle buttonBlackTranslucent = new GUIStyle(buttonStyle);
				buttonBlackTranslucent.name = "ButtonBlackTranslucent";
				buttonBlackTranslucent.normal.background = _box;
				buttonBlackTranslucent.hover.background = _boxHover;
				buttonBlackTranslucent.active.background = _boxHover;
				buttonBlackTranslucent.focused.background = _boxHover;
				buttonBlackTranslucent.normal.textColor = textColour;
				buttonBlackTranslucent.hover.textColor = textColour;
				buttonBlackTranslucent.active.textColor = textColour;
				buttonBlackTranslucent.focused.textColor = textColour;

				GUIStyle buttonTransparent = new GUIStyle(buttonStyle);
				buttonTransparent.name = "ButtonTransparent";
				buttonTransparent.normal.background = null;
				buttonTransparent.hover.background = _transparent;
				buttonTransparent.active.background = _transparent;
				buttonTransparent.focused.background = _transparent;
				buttonTransparent.normal.textColor = textColour;
				buttonTransparent.hover.textColor = textColour;
				buttonTransparent.active.textColor = textColour;
				buttonTransparent.focused.textColor = textColour;
				buttonTransparent.wordWrap = true;
				buttonTransparent.alignment = TextAnchor.LowerCenter;

				// Box styling.
				_skin.box.normal.background = _box;

				GUIStyle boxDark = new GUIStyle(_skin.box);
				boxDark.name = "BoxDark";
				boxDark.normal.background = _boxDark;

				// Label styling.
				GUIStyle labelHeader = new GUIStyle(_skin.label);
				labelHeader.name = "LabelHeader";
				labelHeader.alignment = TextAnchor.MiddleLeft;
				labelHeader.fontSize = 24;
				labelHeader.fontStyle = FontStyle.Bold;
				labelHeader.normal.textColor = textColour;
				labelHeader.hover.textColor = textColour;
				labelHeader.active.textColor = textColour;
				labelHeader.focused.textColor = textColour;
				labelHeader.wordWrap = true;

				GUIStyle labelHeaderCenter = new GUIStyle(labelHeader);
				labelHeaderCenter.name = "LabelHeaderCenter";
				labelHeaderCenter.alignment = TextAnchor.MiddleCenter;

				GUIStyle labelSubHeader = new GUIStyle(labelHeader);
				labelSubHeader.name = "LabelSubHeader";
				labelSubHeader.fontSize = 18;

				GUIStyle labelSubHeaderCenter = new GUIStyle(labelSubHeader);
				labelSubHeaderCenter.name = "LabelSubHeaderCenter";
				labelSubHeaderCenter.alignment = TextAnchor.MiddleCenter;

				GUIStyle labelMessage = new GUIStyle(_skin.label);
				labelMessage.name = "LabelMessage";
				labelMessage.alignment = TextAnchor.MiddleCenter;
				labelMessage.fontSize = 40;
				labelMessage.fontStyle = FontStyle.Bold;
				labelMessage.normal.textColor = textColour;
				labelMessage.hover.textColor = textColour;
				labelMessage.active.textColor = textColour;
				labelMessage.focused.textColor = textColour;
				labelMessage.wordWrap = true;

				GUIStyle labelCenter = new GUIStyle(_skin.label);
				labelCenter.name = "LabelCenter";
				labelCenter.alignment = TextAnchor.MiddleCenter;
				labelCenter.normal.textColor = textColour;
				labelCenter.hover.textColor = textColour;
				labelCenter.active.textColor = textColour;
				labelCenter.focused.textColor = textColour;
				labelCenter.wordWrap = true;

				GUIStyle labelLeft = new GUIStyle(_skin.label);
				labelLeft.name = "LabelLeft";
				labelLeft.alignment = TextAnchor.MiddleLeft;
				labelLeft.normal.textColor = textColour;
				labelLeft.hover.textColor = textColour;
				labelLeft.active.textColor = textColour;
				labelLeft.focused.textColor = textColour;
				labelLeft.wordWrap = true;

				// Add custom styles.
				_skin.customStyles = new GUIStyle[]
				{
					// Buttons.
					buttonPrimary,
					buttonPrimaryWrap,
					buttonPrimaryTextLeft,
					buttonSecondary,
					buttonSecondaryTextLeft,
					buttonBlack,
					buttonWhite,
					buttonBlackTranslucent,
					buttonTransparent,

					// Boxes.
					boxDark,

					// Labels.
					labelHeader,
					labelHeaderCenter,
					labelSubHeader,
					labelSubHeaderCenter,
					labelMessage,
					labelCenter,
					labelLeft,

					// These are just here to prevent log errors, idk where they're coming from.
					new GUIStyle() { name = "thumb" },
					new GUIStyle() { name = "upbutton" },
					new GUIStyle() { name = "downbutton" },
				};
				_hasInitialised = true;
			}
		}

		private static void CreateSkin(GUISkin original)
		{
			// Create skin off default to save setting all GUIStyles individually.
			// Unity doesn't offer a way of doing this so build it manually.
			_skin = ScriptableObject.CreateInstance<GUISkin>();
			_skin.name = $"Achievements";
			_skin.box = new GUIStyle(original.box);
			_skin.button = new GUIStyle(original.button);
			_skin.horizontalScrollbar = new GUIStyle(original.horizontalScrollbar);
			_skin.horizontalScrollbarLeftButton = new GUIStyle(original.horizontalScrollbarLeftButton);
			_skin.horizontalScrollbarRightButton = new GUIStyle(original.horizontalScrollbarRightButton);
			_skin.horizontalScrollbarThumb = new GUIStyle(original.horizontalScrollbarThumb);
			_skin.horizontalSlider = new GUIStyle(original.horizontalSlider);
			_skin.horizontalSliderThumb = new GUIStyle(original.horizontalSliderThumb);
			_skin.label = new GUIStyle(original.label);
			_skin.scrollView = new GUIStyle(original.scrollView);
			_skin.textArea = new GUIStyle(original.textArea);
			_skin.textField = new GUIStyle(original.textField);
			_skin.toggle = new GUIStyle(original.toggle);
			_skin.verticalScrollbar = new GUIStyle(original.verticalScrollbar);
			_skin.verticalScrollbarDownButton = new GUIStyle(original.verticalScrollbarDownButton);
			_skin.verticalScrollbarThumb = new GUIStyle(original.verticalScrollbarThumb);
			_skin.verticalScrollbarUpButton = new GUIStyle(original.verticalScrollbarUpButton);
			_skin.verticalSlider = new GUIStyle(original.verticalSlider);
			_skin.verticalSliderThumb = new GUIStyle(original.verticalSliderThumb);
			_skin.window = new GUIStyle(original.window);
			_skin.font = original.font;
		}
	}
}
