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
			//UIComponent root = ((helper as UIHelper).self as UIComponent);
			((groupG as UIHelper).self as UIComponent).AddUIComponent<UILabel>()
				.text = "NOTE:\n" +
				"· Using mouse inputs may make the game very hard\n" +
				"   to control, or might not do anything!\n" +
				"· Up/Down movement is usually ignored by the game,\n" +
				"   or converted into forward/backward movement.\n" +
				"· Up/Down rotation can confuse the camera about which\n" +
				"   direction is forward; probably you want Zoom instead.\n" +
				"   to fix it, turn back the other way or do a full circle.";

			foreach(JoystickInputDef input in parent.GetInputs()) {
				AddInput(input);
			}
		}

		protected void AddInput(JoystickInputDef input) {
			UIHelperBase group = helper.AddGroup("Input");

			group.AddDropdown("Input Axis", JoystickInputDef.axisNames,
				(int)input.axis,
				(sel) => input.axis = (JoystickInputDef.Axis)sel);

			group.AddDropdown("Output Axis", JoystickInputDef.OutputName,
				(int)input.output,
				(sel) => input.output = (JoystickInputDef.Output)sel);

			group.AddSlider("Movement Speed",
				min: input.minSpeed,
				max: input.maxSpeed,
				step: input.speedStep,
				defaultValue: input.speed,
				eventCallback: (val) => input.speed = val);

			group.AddCheckbox("Invert", input.sign < 0 ? true : false,
				(isChecked) => input.sign = isChecked ? -1 : 1);

			group.AddSlider("Dead Zone",
				min: 0, max: 100, step: 1, defaultValue: input.deadZone * 100,
				eventCallback: (val) => input.deadZone = val / 100);
		}
	}
}
