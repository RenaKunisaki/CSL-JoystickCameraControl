using System;

namespace JoystickCamera {
	/// <summary>
	/// Input source that uses a raw HID device.
	/// </summary>
	public class HidInputSource: InputSource {
		protected HidDeviceHandler device;

		public HidInputSource(HidDeviceHandler device) {
			this.device = device;
			this.name = device.Name;
			foreach(var axis in device.GetAxisNames()) {
				this.axes.Add(axis, new Axis());
			}
			for(int i = 0; i < device.GetButtonCount(); i++) {
				this.buttons.Add((i + 1).ToString(), new Button());
			}
		}

		public override void Update() {
			this.device.Update();
			foreach(var axis in this.device.GetAxes()) {
				this.axes[axis.Key].SetValue(axis.Value);
			}
			var buttons = this.device.GetButtons();
			for(int i = 0; i < buttons.Length; i++) {
				this.buttons[(i + 1).ToString()].SetState(buttons[i]);
			}
		}
	}
}
