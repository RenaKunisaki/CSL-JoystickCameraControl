using System;
using UnityEngine;
using System.Collections.Generic;

namespace JoystickCamera {
	/// <summary>
	/// An input definition for the joystick.
	/// </summary>
	public class JoystickInputDef {
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
		//These names are defined by the game (except None)
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
		public enum ModifierCondition {
			IGNORE,
			HELD,
			NOT_HELD,
			Length
		};
		public static readonly string[] modifierConditionName = {
			"Don't Care", "Held", "Not Held",
		};
		public enum ModifierButton {
			SHIFT_LEFT, SHIFT_RIGHT, SHIFT_ANY,
			CTRL_LEFT, CTRL_RIGHT, CTRL_ANY,
			ALT_LEFT, ALT_RIGHT, ALT_ANY,
			CMD_LEFT, CMD_RIGHT, CMD_ANY,
			WIN_LEFT, WIN_RIGHT, WIN_ANY,
			Length
		};
		public static readonly string[] modifierButtonName = {
			"Left Shift", "Right Shift", "Either Shift",
			"Left Control", "Right Control", "Either Control",
			"Left Alt", "Right Alt", "Either Alt",
			"Left Command", "Right Command", "Either Command",
			"Left Windows", "Right Windows", "Either Windows",
		};
		public class Modifier {
			public ModifierButton button;
			public ModifierCondition condition;
			public Modifier(ModifierButton b, ModifierCondition c) {
				button = b;
				condition = c;
			}
		};
		public Axis axis;
		public float speed;
		public float minSpeed = 1; //for settings UI
		public float maxSpeed = 1000;
		public float speedStep = 1;
		public float sign = 1;
		public float deadZone = 0;
		public List<Modifier> modifiers = new List<Modifier>();
		public Output output;

		public string Name => OutputName[(int)output];
		public string AxisName => axisNames[(int)axis];

		public JoystickInputDef() { }

		public JoystickInputDef(Axis axis, Output output, float speed = 100,
		float sign = 1, float deadZone = 0,
		Modifier[] modifiers = null) {
			this.axis = axis;
			this.output = output;
			this.speed = speed;
			this.sign = sign;
			this.deadZone = deadZone;
			if(modifiers != null) {
				foreach(var mod in modifiers) {
					this.modifiers.Add(mod);
				}
			}
		}

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

		public float Read(Dictionary<ModifierButton, bool> modifiers) {
			if(!CheckModifiers(modifiers)) return 0;
			float val = Input.GetAxis(axisNames[(int)this.axis]);
			if(val > -deadZone && val < deadZone) val = 0;
			return val * speed * sign;
		}
	}
}
