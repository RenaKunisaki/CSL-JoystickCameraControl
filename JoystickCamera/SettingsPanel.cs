using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

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

			groupG.AddButton("Add New Input", () => {
				AddInput(parent.AddInput());
			});

			foreach(JoystickInputDef input in parent.GetInputs()) {
				AddInput(input);
			}
		}

		protected UIPanelWrapper AddPanel(UIComponent parent, string name, int x, int y, int width, int height) {
			UIPanel panel = parent.AddUIComponent<UIPanel>();
			return new UIPanelWrapper(panel, name, x, y, width, height);
		}

		protected void AddInput(JoystickInputDef input) {
			UIHelperBase group = helper.AddGroup(" ");
			UIHelper groupAsHelper = (UIHelper)group;
			UIComponent root = (UIComponent)groupAsHelper.self;
			UIPanelWrapper panel = AddPanel(root, "InputPanel", 0, 0, 600, 100);

			//Add input dropdown
			panel.AddLabel("Input Axis:", 0, 5);
			UIDropDown ddInput = panel.AddDropdown(
				name: "input", x: 90, y: 0, items: JoystickInputDef.axisNames);
			ddInput.selectedIndex = (int)input.axis;
			ddInput.eventSelectedIndexChanged += (component, value) => {
				input.axis = (JoystickInputDef.Axis)value;
			};

			//Add output dropdown
			panel.AddLabel("Action:", 300, 5);
			UIDropDown ddOutput = panel.AddDropdown(
				name: "output", x: 370, y: 0, items: JoystickInputDef.OutputName);
			ddOutput.selectedIndex = (int)input.output;
			ddOutput.eventSelectedIndexChanged += (component, value) => {
				input.output = (JoystickInputDef.Output)value;
			};

			//Add speed slider
			panel.AddLabel("Movement Speed:", 0, 30);
			panel.AddSlider(name: "speed", x: 150, y: 30,
				value: input.speed, min: input.minSpeed,
				max: input.maxSpeed, step: input.speedStep,
				tooltip: "How fast the camera should move")
				.eventValueChanged += (component, value) => {
					input.speed = value;
				};

			//Add invert checkbox
			panel.AddCheckbox("invert", 380, 30, input.sign < 0,
			"Move in opposite direction")
			.eventClick += (component, eventParam) => {
				input.sign = ((UICustomCheckbox)component).isChecked ? -1 : 1;
			};
			panel.AddLabel("Invert", 400, 30);

			//Add deadzone slider
			panel.AddLabel("Dead Zone:", 0, 60);
			panel.AddSlider(name: "deadzone", x: 150, y: 60,
				value: input.deadZone * 100, min: 0, max: 100, step: 1,
				tooltip: "Ignore movements less than this magnitude")
				.eventValueChanged += (component, value) => {
					input.deadZone = value / 100;
				};

			//Add delete button
			UIButton btnDelete = panel.AddButton("Delete", 380, 60, 70);
			btnDelete.eventClicked += (component, eventParam) => {
				parent.RemoveInput(input);
				JoystickCamera.Log("Hiding the input group");
				(group as UIPanel).Hide();
				JoystickCamera.Log("Removing the input group");
				((helper as UIHelper).self as UIPanel).RemoveUIComponent(
					group as UIComponent);
				JoystickCamera.Log("Removed the input group");
			};
		}
	}
}
