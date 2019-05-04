using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace JoystickCamera {
	/// <summary>
	/// Manages the settings UI.
	/// </summary>
	public class SettingsPanel {
		protected JoystickCamera parent;
		protected UIHelperBase helper;
		protected UIComponent root;

		public SettingsPanel(JoystickCamera parent, UIHelperBase helper) {
			this.parent = parent;
			this.helper = helper;
		}

		/// <summary>
		/// Create and run the panel.
		/// </summary>
		public void Run() {
			this.root = ((helper as UIHelper).self as UIComponent);

			//Add notes
			UIHelperBase groupG = helper.AddGroup(" ");
			var groupRoot = ((groupG as UIHelper).self as UIComponent);
			var panel = this.AddPanel(groupRoot, "general", 0, 0, 600, 2);

			UICustomTabStrip tabStrip = panel.AddTabStrip("tabstrip",
				out UITabContainer tabContainer);
			tabStrip.relativePosition = new Vector3(0, -20, 0);
			//tabContainer.backgroundSprite = "SubBarButtonBase";
			/*
			panel.AddLabel(
				"· Up/Down movement is usually ignored by the game,\n" +
				"   or converted into forward/backward movement.", 0, 0);

			*/
			//JoystickCamera.Log($"Panel size is {root.width} x {root.height}");
			tabContainer.width = this.root.width;
			tabContainer.height = this.root.height - 32;
			tabContainer.relativePosition = new Vector3(0, 32, 0);
			AddGeneralTab(tabStrip, tabContainer);
			AddInputsTab(tabStrip, tabContainer);
			AddCurrentInputsTab(tabStrip, tabContainer);


			tabStrip.selectedIndex = 0;
		}

		protected void AddGeneralTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			UIButton tabButton = tabStrip.AddTab("General",
				out UIPanelWrapper tabPanel, "General settings");

			//Add Reset Camera button
			UIButton btnReset = tabPanel.AddButton("Reset Camera", 0, 0, 130, 30,
				"Reset camera to a sane state.");
			btnReset.eventClicked += (component, eventParam) => {
				GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
				if(gameObject == null) return;
				CameraController cameraController = gameObject.GetComponent<CameraController>();
				cameraController.Reset(Vector3.zero);
			};

			//Add debug toggle
			tabPanel.AddCheckbox("debug", 0, 35, parent.enableDebugDisplay,
			"Show debug info in-game.")
			.eventClick += (component, eventParam) => {
				//wtf
				parent.enableDebugDisplay = !((UICustomCheckbox)component).isChecked;
				//parent.SaveConfig();
			};
			tabPanel.AddLabel("Show Debug Info", 20, 35);
		}

		protected void AddInputsTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			UIButton tabButton = tabStrip.AddTab("Inputs",
				out UIPanelWrapper tabPanel, "Input definitions");

			//Add New Input button
			UIButton btnAdd = tabPanel.AddButton("Add New Input", 0, 0, 130, 30,
				"Add an input.");
			btnAdd.eventClicked += (component, eventParam) => {
				AddInput(parent.AddInput(), tabPanel);
			};

			//why -50 here? no idea
			var scrollablePanel = tabPanel.AddScrollablePanel("inputs_scrollable",
				0, 30, (int)tabContainer.width - 50, (int)tabContainer.height - 100,
				out UIScrollbar scrollbar);
			//scrollablePanel.Panel.backgroundSprite = "SubBarButtonBase";

			//Add inputs
			foreach(JoystickInputDef input in parent.GetInputs()) {
				AddInput(input, scrollablePanel);
			}
			scrollbar.maxValue = scrollablePanel.height + scrollbar.scrollSize + 500;
			scrollablePanel.Panel.parent.height = scrollablePanel.height;
			//scrollbar.autoSize = true;
			scrollbar.autoHide = true;
			scrollbar.incrementAmount = 120; //about the height of one entry
		}

		protected void AddCurrentInputsTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			//Add current value display
			UIButton tabButton = tabStrip.AddTab("Devices",
				out UIPanelWrapper tabPanel, "Current input values");

			int y = 0;
			foreach(var src in parent.GetInputSources()) {
				y = AddCurrentValues(src, tabPanel, y) + 20;
			}
		}

		/// <summary>
		/// Adds the display of current joystick inputs.
		/// </summary>
		/// <remarks>This is helpful to find which axis maps to which physical
		/// input on the joystick.</remarks>
		protected int AddCurrentValues(InputSource source, UIPanelWrapper parent, int y) {
			var name = source.Name;
			var axisNames = source.GetAxisNames();
			var axes = source.GetAxes();

			//groupRoot.relativePosition = new Vector3(0, 0, 0);
			var groupRoot = parent.Panel;
			int x = 0;
			UIPanelWrapper panel = null;
			bool isFirst = true;
			parent.AddLabel(source.Name, 100, y).textScale = 1.5f;
			y += 30;

			foreach(string axis in axisNames) {
				if(axis == "None") continue;

				if(x == 0) {
					//Panels take up full width and auto align height,
					//so to do two columns we need to pack them both
					//into a single panel.
					panel = this.AddPanel(groupRoot, "display_" + axis, 0, y, 600, 30);
				}
				panel.AddLabel(axis, x, 0);
				var slider = panel.AddSlider(axis + "_curval",
					x: x, y: 17, value: 0, min: -255, max: 255, step: 1);
				x += 300;
				if(x >= 600) {
					x = 0;
					y += 30;
				}

				slider.isInteractive = false;
				if(isFirst) {
					slider.OnUpdate += () => {
						source.Update();
						slider.value = axes[axis].GetValue();
					};
					isFirst = false;
				}
				else slider.OnUpdate += () => {
					slider.value = axes[axis].GetValue();
				};
			}
			if(x > 0) y += 30;
			return y;
		}

		/// <summary>
		/// Add a panel.
		/// </summary>
		/// <returns>The panel.</returns>
		/// <param name="parent">Parent.</param>
		/// <param name="name">Name.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <remarks>XXX is this used/needed?</remarks>
		protected UIPanelWrapper AddPanel(UIComponent parent, string name, int x, int y, int width, int height) {
			UIPanel panel = parent.AddUIComponent<UIPanel>();
			return new UIPanelWrapper(panel, name, x, y, width, height);
		}

		/// <summary>
		/// Add the widgets for an input.
		/// </summary>
		/// <param name="input">Input.</param>
		protected void AddInput(JoystickInputDef input, UIPanelWrapper container) {
			Vector2 bounds = container.GetBounds();
			UIPanelWrapper panel = container.AddPanel(input.Name, 0, (int)bounds.y + 30, 600, 100);

			var sources = parent.GetInputSources();
			InputSource source = null;
			var devNames = new List<string>(sources.Count);
			int devIdx = -1;
			for(int i = 0; i < sources.Count; i++) {
				devNames.Add(sources[i].Name);
				if(sources[i] == input.inputSource) {
					devIdx = i;
					source = sources[i];
					//keep going, we need the names
				}
			}

			var axisNames = input.inputSource.GetAxisNames();
			UIDropDown ddInput = null;

			//Add device dropdown.
			panel.AddLabel("Device:", 0, 5);
			UIDropDown ddDevice = panel.AddDropdown(
				name: "device", x: 70, y: 0, items: devNames.ToArray(),
				tooltip: "Which input device to use.");
			ddDevice.selectedIndex = devIdx;
			ddDevice.eventSelectedIndexChanged += (component, value) => {
				input.inputSource = sources[value];
				ddInput.selectedIndex = 0;
				ddInput.items = input.inputSource.GetAxisNames();
				parent.SaveConfig();
			};

			//Add input dropdown.
			panel.AddLabel("Input:", 300, 5);
			ddInput = panel.AddDropdown(
				name: "input", x: 350, y: 0, items: axisNames,
				tooltip: "Which input axis to use.");
			ddInput.selectedIndex = Array.IndexOf(axisNames, input.axis);
			ddInput.eventSelectedIndexChanged += (component, value) => {
				input.axis = input.inputSource.GetAxisNames()[value];
				parent.SaveConfig();
			};

			//Add output dropdown.
			panel.AddLabel("Action:", 0, 30);
			UIDropDown ddOutput = panel.AddDropdown(
				name: "output", x: 70, y: 30, items: JoystickInputDef.OutputName,
				tooltip: "What this axis should do.");
			ddOutput.selectedIndex = (int)input.output;
			ddOutput.eventSelectedIndexChanged += (component, value) => {
				input.output = (JoystickInputDef.Output)value;
				parent.SaveConfig();
			};

			//Add speed slider.
			panel.AddLabel("Movement Speed:", 300, 30);
			panel.AddSlider(name: "speed", x: 450, y: 30,
				value: input.speed, min: input.minSpeed,
				max: input.maxSpeed, step: input.speedStep,
				tooltip: "How fast the camera should move.")
				.eventValueChanged += (component, value) => {
					input.speed = value;
					parent.SaveConfig();
				};

			//Add offset slider.
			panel.AddLabel("Offset:", 385, 60);
			panel.AddSlider(name: "offset", x: 450, y: 60, value: input.offset * 100,
				min: -100, max: 100, step: 1, tooltip: "Offset added to input.")
				.eventValueChanged += (component, value) => {
					input.offset = value / 100;
					parent.SaveConfig();
				};

			//Add deadzone slider.
			panel.AddLabel("Dead Zone:", 350, 90);
			panel.AddSlider(name: "deadzone", x: 450, y: 90,
				value: input.deadZone * 100, min: 0, max: 100, step: 1,
				tooltip: "Ignore movements less than this magnitude.")
				.eventValueChanged += (component, value) => {
					input.deadZone = value / 100;
					parent.SaveConfig();
				};

			//Add invert checkbox.
			panel.AddCheckbox("invert", 0, 60, input.sign < 0,
			"Move in opposite direction.")
			.eventClick += (component, eventParam) => {
				//invert checkbox logic because isChecked updates AFTER
				//this callback, even though the callback that handles
				//that is added before this one. WTF?
				input.sign = !((UICustomCheckbox)component).isChecked ? -1 : 1;
				parent.SaveConfig();
			};
			panel.AddLabel("Invert", 20, 60);

			//Add smoothing checkbox.
			panel.AddCheckbox("smoothing", 85, 60, input.smoothing,
			"Use Unity's input smoothing.")
			.eventClick += (component, eventParam) => {
				input.smoothing = !((UICustomCheckbox)component).isChecked;
				parent.SaveConfig();
			};
			panel.AddLabel("Smoothing", 105, 60);

			//Add relative checkbox.
			panel.AddCheckbox("relative", 200, 60, input.relative,
			"Use relative input values.")
			.eventClick += (component, eventParam) => {
				input.relative = !((UICustomCheckbox)component).isChecked;
				parent.SaveConfig();
			};
			panel.AddLabel("Relative", 220, 60);

			//Add delete button.
			UIButton btnDelete = panel.AddButton("Delete Input", 575, 0, 110, 20,
				"Delete this input.");
			btnDelete.eventClicked += (component, eventParam) => {
				parent.RemoveInput(input);
				parent.SaveConfig();
				root.parent.RemoveUIComponent(root);
				UnityEngine.Object.Destroy(root);
			};

			//Add modifiers header.
			panel.AddLabel("Modifiers:", 0, 85);
			UIButton btnAddMod = panel.AddButton("Add Modifier",
				90, 85, 110, 20, "Add a modifier");
			btnAddMod.eventClicked += (component, eventParam) => {
				var mod = new JoystickInputDef.Modifier(
					JoystickInputDef.ModifierButton.SHIFT_ANY,
					JoystickInputDef.ModifierCondition.HELD);
				input.modifiers.Add(mod);
				parent.SaveConfig();
				panel.height += 25;
				AddModifierWidgets(input, mod, panel, (int)panel.height - 25);
			};

			//Add modifiers.
			int y = 120;
			foreach(var mod in input.modifiers) {
				AddModifierWidgets(input, mod, panel, y);
				y += 25;
			}
			panel.height = y;
		}

		/// <summary>
		/// Add the widgets for a modifier key.
		/// </summary>
		/// <param name="input">Input this modifier belongs to.</param>
		/// <param name="mod">Modifier.</param>
		/// <param name="panel">Panel to add to.</param>
		/// <param name="y">Y coordinate to add widgets at.</param>
		protected void AddModifierWidgets(JoystickInputDef input,
		JoystickInputDef.Modifier mod, UIPanelWrapper panel, int y) {
			string name = $"modifier{y}";
			var SubPanel = panel.AddPanel(name, 0, y, 600, 25);

			//Add button dropdown.
			var drop = SubPanel.AddDropdown(name, 0, 0,
				JoystickInputDef.modifierButtonName, "Which button to use");
			drop.selectedIndex = (int)mod.button;
			drop.eventSelectedIndexChanged += (component, value) => {
				mod.button = (JoystickInputDef.ModifierButton)value;
				parent.SaveConfig();
			};

			//Add Held/Not Held checkboxes.
			var lblHeld = SubPanel.AddLabel("Held", 230, 2);
			var chkHeld = SubPanel.AddCheckbox(name + "_held", 210, 2,
				mod.condition == JoystickInputDef.ModifierCondition.HELD,
				"Button must be held");

			var lblNotHeld = SubPanel.AddLabel("Not Held", 290, 2);
			var chkNotHeld = SubPanel.AddCheckbox(name + "_notheld", 270, 2,
				mod.condition == JoystickInputDef.ModifierCondition.NOT_HELD,
				"Button must not be held");

			//Add checkbox event handlers.
			chkHeld.eventClicked += (component, eventParam) => {
				mod.condition = chkHeld.isChecked ?
					JoystickInputDef.ModifierCondition.HELD
					: JoystickInputDef.ModifierCondition.NOT_HELD;
				chkNotHeld.isChecked = !chkHeld.isChecked;
				parent.SaveConfig();
			};
			chkNotHeld.eventClicked += (component, eventParam) => {
				mod.condition = chkNotHeld.isChecked ?
					JoystickInputDef.ModifierCondition.NOT_HELD
					: JoystickInputDef.ModifierCondition.HELD;
				chkHeld.isChecked = !chkNotHeld.isChecked;
				parent.SaveConfig();
			};

			//Add delete button.
			UIButton btnDeleteMod = SubPanel.AddButton("Delete", 370, 0, 70, 20,
				"Delete this modifier");
			btnDeleteMod.eventClicked += (component, eventParam) => {
				input.modifiers.Remove(mod);
				parent.SaveConfig();
				panel.Remove(SubPanel.Panel);
			};
		}
	}
}
