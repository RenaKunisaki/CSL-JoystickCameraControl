using System;
namespace JoystickCamera {
	/// <summary>
	/// Represents a device that's missing or inaccessible.
	/// Used so we can still have inputs defined in config that refer to
	/// devices which aren't currently present.
	/// </summary>
	public class PlaceholderInputSource: InputSource {
		public PlaceholderInputSource(string name, string[] axes, string[] buttons) {
			this.name = name;
			foreach(var axis in axes) {
				this.axes.Add(axis, new Axis());
			}
			foreach(var button in buttons) {
				this.buttons.Add(button, new Button());
			}

			//set all to a default state
			foreach(var axis in this.axes) {
				axis.Value.SetValue(0);
			}
			foreach(var button in this.buttons) {
				button.Value.SetState(false);
			}
		}

		public override void Update() {
			//do nothing
		}
	}
}
