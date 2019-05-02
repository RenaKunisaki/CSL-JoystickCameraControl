using System;
using UnityEngine;
using System.Collections.Generic;

namespace JoystickCamera {
	/// <summary>
	/// Input source that uses Unity's input manager.
	/// </summary>
	public class UnityInputSource: InputSource {
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

		public static readonly string[] buttonNames = {
			"Mouse Button 1", "Mouse Button 2","Mouse Button 3", "Mouse Button 4",
			"Mouse Button 5", "Mouse Button 6", "Mouse Button 7",
			"Button 1", "Button 2", "Button 3", "Button 4", "Button 5",
			"Button 6", "Button 7", "Button 8", "Button 9", "Button 10",
			"Button 11", "Button 12", "Button 13", "Button 14", "Button 15",
			"Button 16", "Button 17", "Button 18", "Button 19", "Button 20",
		};

		protected static readonly Dictionary<string, float> AxisScales = new Dictionary<string, float> {
			{ "None", 0 },
			{ "Horizontal", 100},
			{ "Vertical", 100},
			{ "RotationHorizontalCamera", 100},
			{ "RotationVerticalCamera", 100},
			{ "ZoomCamera", 100},
			{ "Mouse X", 1},
			{ "Mouse Y", 1},
			{ "Mouse ScrollWheel", 1},
		};

		public UnityInputSource() {
			this.name = "Unity Input Manager";
			foreach(var axis in axisNames) {
				this.axes.Add(axis, new Axis());
			}
		}

		public override void Update() {
			//Read axes
			foreach(var axis in this.axes) {
				float val = Input.GetAxisRaw(axis.Key) * AxisScales[axis.Key];
				axis.Value.SetValue(val);
			}
		}
	}
}
