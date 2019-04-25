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
		/// Available axes.
		/// </summary>
		/// <remarks>These names are defined by the game (except None).
		/// We can't use any others unfortunately.</remarks>
		public enum Axis {
			NONE,
			HORIZONTAL,
			VERTICAL,
			ROTATION_HORIZONTAL_CAMERA,
			ROTATION_VERTICAL_CAMERA,
			ZOOM_CAMERA,
			MOUSE_X,
			MOUSE_Y,
			MOUSE_SCROLLWHEEL,
		};
		public static readonly string[] axisNames = {
			"None",
			"Horizontal",
			"Vertical",
			"RotationHorizontalCamera",
			"RotationVerticalCamera",
			"ZoomCamera",
			"Mouse X",
			"Mouse Y",
			"Mouse ScrollWheel",
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

		public Axis axis; //the axis to use for input
		public float speed; //how fast the camera moves
		public float minSpeed = 1; //for settings UI, minimum value for speed slider
		public float maxSpeed = 1000; //max value for speed slider
		public float speedStep = 1; //step size for speed slider
		public float sign = 1; //-1 to invert input, 1 to not invert
		public float deadZone = 0; //ignore input less than this magnitude
		public float offset = 0; //add offset to input
		public bool smoothing = true; //use input smoothing
		public List<Modifier> modifiers = new List<Modifier>();
		public Output output; //the output variable to control

		public string Name => OutputName[(int)output];
		public string AxisName => axisNames[(int)axis];

		public JoystickInputDef() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:JoystickCamera.JoystickInputDef"/> class.
		/// </summary>
		/// <param name="axis">Axis to use as input.</param>
		/// <param name="output">Output variable to control.</param>
		/// <param name="speed">How fast the camera will move.</param>
		/// <param name="sign">Either -1 to invert the input, or 1 to not invert.</param>
		/// <param name="deadZone">Ignore inputs less than this magnitude.</param>
		/// <param name="modifiers">Modifier keys.</param>
		public JoystickInputDef(Axis axis, Output output, float speed = 100,
		float sign = 1, float deadZone = 0, float offset = 0, bool smoothing = true,
		Modifier[] modifiers = null) {
			this.axis = axis;
			this.output = output;
			this.speed = speed;
			this.sign = sign;
			this.deadZone = deadZone;
			this.offset = offset;
			this.smoothing = smoothing;
			if(modifiers != null) {
				foreach(var mod in modifiers) {
					this.modifiers.Add(mod);
				}
			}
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
			string axisName = axisNames[(int)this.axis];
			float val = smoothing ? Input.GetAxis(axisName) : Input.GetAxisRaw(axisName);
			val += offset;
			if(val > -deadZone && val < deadZone) val = 0;
			return val * speed * sign;
		}
	}
}
