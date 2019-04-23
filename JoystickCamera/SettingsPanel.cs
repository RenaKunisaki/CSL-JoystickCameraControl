using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace JoystickCamera {
	public class SettingsPanel {
		protected JoystickCamera parent;
		protected UIHelperBase helper;
		protected UIComponent root;

		public SettingsPanel(JoystickCamera parent, UIHelperBase helper) {
			this.parent = parent;
			this.helper = helper;
		}

		public void Run() {
			this.root = ((helper as UIHelper).self as UIComponent);

			UIHelperBase groupG = helper.AddGroup("Note:");
			((groupG as UIHelper).self as UIComponent).AddUIComponent<UILabel>()
				.text =
				"· Using mouse inputs may make the game very hard\n" +
				"   to control, or might not do anything!\n" +
				"· Up/Down movement is usually ignored by the game,\n" +
				"   or converted into forward/backward movement.\n" +
				"· Up/Down rotation can confuse the camera about which\n" +
				"   direction is forward; probably you want Zoom instead.\n" +
				"   To fix it, turn back the other way or do a full circle.";

			groupG.AddButton("Add New Input", () => {
				AddInput(parent.AddInput());
			});

			//Add displays of current input values
			UIHelperBase groupV = helper.AddGroup("Current Input Values");
			var groupRoot = ((groupV as UIHelper).self as UIComponent);
			int y = 0;
			foreach(string axis in JoystickInputDef.axisNames) {
				if(axis == "None") continue;

				var panel = this.AddPanel(groupRoot,
					"display_" + axis, 0, y, 400, 30);
				panel.AddLabel(axis, 0, 0);
				var slider = panel.AddSlider(axis + "_curval",
					x: 150, y: 0, value: 0, min: -100, max: 100, step: 1);
				y += 25;

				slider.isInteractive = false;
				slider.OnUpdate += () => {
					slider.value = Input.GetAxis(axis) * 100;
				};
			}

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
			UIButton btnDelete = panel.AddButton("Delete Input", 380, 60, 110);
			btnDelete.eventClicked += (component, eventParam) => {
				parent.RemoveInput(input);
				root.parent.RemoveUIComponent(root);
				UnityEngine.Object.Destroy(root);
			};

			//Add modifiers header
			panel.AddLabel("Modifiers:", 0, 85);
			UIButton btnAddMod = panel.AddButton("Add Modifier",
				90, 85, 110, 20, "Add a modifier");
			btnAddMod.eventClicked += (component, eventParam) => {
				var mod = new JoystickInputDef.Modifier(
					JoystickInputDef.ModifierButton.SHIFT_ANY,
					JoystickInputDef.ModifierCondition.HELD);
				input.modifiers.Add(mod);
				panel.height += 25;
				AddModifierWidgets(input, mod, panel, (int)panel.height - 25);
			};

			//Add modifiers
			int y = 105;
			foreach(var mod in input.modifiers) {
				AddModifierWidgets(input, mod, panel, y);
				y += 25;
			}
			panel.height = y;
		}

		protected void AddModifierWidgets(JoystickInputDef input,
		JoystickInputDef.Modifier mod, UIPanelWrapper panel, int y) {
			string name = $"modifier{y}";
			var SubPanel = panel.AddPanel(name, 0, y, 600, 25);

			var drop = SubPanel.AddDropdown(name, 0, 0,
				JoystickInputDef.modifierButtonName, "Which button to use");
			drop.eventSelectedIndexChanged += (component, value) => {
				mod.button = (JoystickInputDef.ModifierButton)value;
			};

			var lblHeld = SubPanel.AddLabel("Held", 230, 2);
			var chkHeld = SubPanel.AddCheckbox(name + "_held", 210, 2,
				mod.condition == JoystickInputDef.ModifierCondition.HELD,
				"Button must be held");

			var lblNotHeld = SubPanel.AddLabel("Not Held", 290, 2);
			var chkNotHeld = SubPanel.AddCheckbox(name + "_notheld", 270, 2,
				mod.condition == JoystickInputDef.ModifierCondition.NOT_HELD,
				"Button must not be held");

			chkHeld.eventClicked += (component, eventParam) => {
				mod.condition = chkHeld.isChecked ?
					JoystickInputDef.ModifierCondition.HELD
					: JoystickInputDef.ModifierCondition.NOT_HELD;
				chkNotHeld.isChecked = !chkHeld.isChecked;
			};
			chkNotHeld.eventClicked += (component, eventParam) => {
				mod.condition = chkNotHeld.isChecked ?
					JoystickInputDef.ModifierCondition.NOT_HELD
					: JoystickInputDef.ModifierCondition.HELD;
				chkHeld.isChecked = !chkNotHeld.isChecked;
			};

			UIButton btnDeleteMod = SubPanel.AddButton("Delete", 370, 0, 70, 20,
				"Delete this modifier");
			btnDeleteMod.eventClicked += (component, eventParam) => {
				input.modifiers.Remove(mod);
				panel.Remove(SubPanel.Panel);
			};
		}
	}
}
