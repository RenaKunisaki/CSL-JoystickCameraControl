using System;
using UnityEngine;

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
		public Axis axis;
		public float speed;
		public float minSpeed = 1; //for settings UI
		public float maxSpeed = 1000;
		public float speedStep = 10;
		public float sign = 1;
		public float deadZone = 0;
		public Output output;

		public string Name => OutputName[(int)output];

		public float Read() {
			float val = Input.GetAxis(axisNames[(int)this.axis]);
			if(val > -deadZone && val < deadZone) val = 0;
			return val * speed * sign;
		}
	}
}
