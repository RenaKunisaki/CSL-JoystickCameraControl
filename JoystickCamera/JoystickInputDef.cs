using System;
using UnityEngine;
using System.Collections.Generic;

namespace JoystickCamera {
	/// <summary>
	/// An input definition for the joystick.
	/// </summary>
	public class JoystickInputDef {
		/// <summary>
		/// Available outputs an axis can map to.
		/// </summary>
		public enum Output {
			CAMERA_MOVE_X,
			CAMERA_MOVE_Y,
			CAMERA_MOVE_Z,
			CAMERA_MOVE_NS, //north/south
			CAMERA_MOVE_EW, //east/west
			CAMERA_ZOOM,
			CAMERA_TURN_X,
			CAMERA_TURN_Y,
		};
		public static readonly string[] OutputName = {
			"Move Left/Right",
			"Move Up/Down",
			"Move Forward/Backward",
			"Move North/South",
			"Move East/West",
			"Zoom In/Out",
			"Turn Left/Right",
			"Turn Up/Down",
		};

		/// <summary>
		/// Conditions for modifier keys.
		/// </summary>
		public enum ModifierCondition {
			IGNORE, //XXX necessary?
			HELD,
			NOT_HELD,
			Length
		};
		public static readonly string[] modifierConditionName = {
			"Don't Care", "Held", "Not Held",
		};
		/// <summary>
		/// Buttons that can be used as modifiers.
		/// </summary>
		public enum ModifierButton {
			SHIFT_LEFT, SHIFT_RIGHT, SHIFT_ANY,
			CTRL_LEFT, CTRL_RIGHT, CTRL_ANY,
			ALT_LEFT, ALT_RIGHT, ALT_ANY,
			CMD_LEFT, CMD_RIGHT, CMD_ANY,
			WIN_LEFT, WIN_RIGHT, WIN_ANY,
			MOUSE1, MOUSE2, MOUSE3, MOUSE4, MOUSE5, MOUSE6, MOUSE7,
			BUTTON1, BUTTON2, BUTTON3, BUTTON4, BUTTON5,
			BUTTON6, BUTTON7, BUTTON8, BUTTON9, BUTTON10,
			BUTTON11, BUTTON12, BUTTON13, BUTTON14, BUTTON15,
			BUTTON16, BUTTON17, BUTTON18, BUTTON19, BUTTON20,
			Length
		};
		public static readonly string[] modifierButtonName = {
			"Left Shift", "Right Shift", "Either Shift",
			"Left Control", "Right Control", "Either Control",
			"Left Alt", "Right Alt", "Either Alt",
			"Left Command", "Right Command", "Either Command",
			"Left Windows", "Right Windows", "Either Windows",
			"Mouse Button 1", "Mouse Button 2","Mouse Button 3", "Mouse Button 4",
			"Mouse Button 5", "Mouse Button 6", "Mouse Button 7",
			"Button 1", "Button 2", "Button 3", "Button 4", "Button 5",
			"Button 6", "Button 7", "Button 8", "Button 9", "Button 10",
			"Button 11", "Button 12", "Button 13", "Button 14", "Button 15",
			"Button 16", "Button 17", "Button 18", "Button 19", "Button 20",
		};
		public class Modifier {
			public ModifierButton button;
			public ModifierCondition condition;
			public Modifier(ModifierButton b, ModifierCondition c) {
				button = b;
				condition = c;
			}
		};

		public InputSource inputSource;
		public string axis; //input axis name
		public float speed; //how fast the camera moves
		public float minSpeed = 1; //for settings UI, minimum value for speed slider
		public float maxSpeed = 1000; //max value for speed slider
		public float speedStep = 1; //step size for speed slider
		public float sign = 1; //-1 to invert input, 1 to not invert
		public float deadZone = 0; //ignore input less than this magnitude
		public float offset = 0; //add offset to input
		public bool smoothing = true; //use input smoothing
		public bool relative = false; //use relative input
		public float prevValue = 0;
		public float lastValue = 0; //for debug
		public List<Modifier> modifiers = new List<Modifier>();
		public Output output; //the output variable to control

		public string Name => OutputName[(int)output];

		public JoystickInputDef() {
			modifiers = new List<Modifier>();
		}

		/// <summary>
		/// Check if the current modifier key states are satisfied for this input.
		/// </summary>
		/// <returns><c>true</c>, if we should process this input,
		/// <c>false</c> if we should ignore it.</returns>
		/// <param name="modifiers">Current modifier key states.</param>
		protected bool CheckModifiers(Dictionary<ModifierButton, bool> modifiers) {
			if(this.modifiers == null) return true;
			foreach(var mod in this.modifiers) {
				bool req = false;
				switch(mod.condition) {
					case ModifierCondition.IGNORE: continue;
					case ModifierCondition.HELD: req = true; break;
					case ModifierCondition.NOT_HELD: req = false; break;
				}
				if(modifiers[mod.button] != req) return false;
			}
			return true;
		}

		/// <summary>
		/// Read the input value.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="modifiers">Current modifier key states.</param>
		/// <remarks>If the modifier conditions aren't satisfied, returns 0.</remarks>
		public float Read(Dictionary<ModifierButton, bool> modifiers) {
			if(!CheckModifiers(modifiers)) return 0;
			float val = this.inputSource.GetAxis(this.axis).GetValue(this.smoothing);

			float retval = val;
			if(relative) retval -= prevValue;
			prevValue = val;

			retval += offset;
			if(retval > -deadZone && retval < deadZone) retval = 0;
			lastValue = retval;
			return retval * speed * sign;
		}
	}
}
