using System;
using ColossalFramework.UI;
using ICities;

namespace JoystickCamera {
	public class SettingsPanel {
		protected JoystickCamera parent;
		protected UIHelperBase helper;

		public SettingsPanel(JoystickCamera parent, UIHelperBase helper) {
			this.parent = parent;
			this.helper = helper;
		}

		public void Run() {
			UIHelperBase groupG = helper.AddGroup("General");
			//groupG.AddCheckbox("Move Relative to Screen", !this.worldRelative,
			//	(isChecked) => this.worldRelative = !isChecked);

			//UIComponent root = ((helper as UIHelper).self as UIComponent);
			((groupG as UIHelper).self as UIComponent).AddUIComponent<UILabel>()
				.text = "Note: Using mouse inputs may make the game very hard\n" +
				"to control, or might not do anything!";

			foreach(JoystickInputDef input in parent.GetInputs()) {
				AddInput(input);
			}
		}

		protected void AddInput(JoystickInputDef input) {
			UIHelperBase group = helper.AddGroup(input.Name);

			//HACK
			if(input.output == JoystickInputDef.Output.CAMERA_TURN_Y) {
				((group as UIHelper).self as UIComponent).AddUIComponent<UILabel>()
				.text = "Note: Rotating the camera up/down might confuse the game\n" +
				"about which way is forward! (You should probably use Zoom instead.)\n" +
				"To fix it, just rotate all the way back around.";
			}

			group.AddSlider("Movement Speed",
				min: input.minSpeed,
				max: input.maxSpeed,
				step: input.speedStep,
				defaultValue: input.speed,
				eventCallback: (val) => input.speed = val);

			group.AddDropdown("Input", JoystickInputDef.axisNames, (int)input.axis,
				(sel) => input.axis = (JoystickInputDef.Axis)sel);

			group.AddCheckbox("Invert", input.sign < 0 ? true : false,
				(isChecked) => input.sign = isChecked ? -1 : 1);

			group.AddSlider("Dead Zone",
				min: 0, max: 100, step: 1, defaultValue: input.deadZone * 100,
				eventCallback: (val) => input.deadZone = val / 100);
		}
	}
}
