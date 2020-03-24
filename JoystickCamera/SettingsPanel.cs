using System;
using System.Linq;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace JoystickCamera {
	/// <summary>
	/// Manages the settings UI.
	/// </summary>
	public class SettingsPanel {
		protected JoystickCamera parent;
		protected UIHelperBase helper, mainGroup;
		protected UIComponent root;
		protected UICustomTabStrip tabStrip;
		protected UITabContainer tabContainer;
		protected UIPanelWrapper mainPanel;

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
			mainGroup = helper.AddGroup(" ");
			var groupRoot = ((mainGroup as UIHelper).self as UIComponent);
			mainPanel = this.AddPanel(groupRoot, "settings", 0, 0, 600, 2);

			tabStrip = mainPanel.AddTabStrip("tabstrip", out tabContainer);
			tabStrip.relativePosition = new Vector3(0, -20, 0);
			//tabContainer.backgroundSprite = "SubBarButtonBase"; //debug
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
			AddInstructionsTab(tabStrip, tabContainer);
			AddInputsTab(tabStrip, tabContainer);
			AddCurrentInputsTab(tabStrip, tabContainer);
			AddAboutTab(tabStrip, tabContainer);

			tabStrip.selectedIndex = 0;
		}

		/// <summary>
		/// Update all widgets.
		/// </summary>
		public void Refresh() {
			var groupRoot = ((mainGroup as UIHelper).self as UIComponent);
			this.root.RemoveUIComponent(this.tabStrip);
			this.root.RemoveUIComponent(this.tabContainer);
			this.root.RemoveUIComponent(this.mainPanel.Panel);
			this.root.RemoveUIComponent(groupRoot);
			UnityEngine.Object.Destroy(this.tabStrip);
			UnityEngine.Object.Destroy(this.tabContainer);
			UnityEngine.Object.Destroy(this.mainPanel.Panel);
			UnityEngine.Object.Destroy(groupRoot);
			this.Run();
			//XXX why does this add a gap at the top?
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

			//Add USB toggle
			tabPanel.AddCheckbox("usb", 0, 35, parent.enableUsbDevices,
			"Use USB mice, joysticks, etc as inputs.")
			.OnChange += (isChecked) => {
				parent.enableUsbDevices = isChecked;
				if(parent.enableUsbDevices) {
					parent.EnumerateDevices();
					parent.LoadConfig(); //re-read inputs
					parent.enableUsbDevices = true; //loading config will reset it
					this.Refresh();
				}
				parent.SaveConfig();
			};
			tabPanel.AddLabel("Scan for USB input devices", 20, 35);

			//Add restrict rotation toggle
			tabPanel.AddCheckbox("restrictRotation", 0, 55, parent.restrictRotation,
			"Restrict camera rotation to range normally allowed by game.")
			.OnChange += (isChecked) => {
				parent.restrictRotation = isChecked;
				parent.SaveConfig();
			};
			tabPanel.AddLabel("Restrict Camera Rotation", 20, 55);
			tabPanel.AddLabel("Unchecking this lets you rotate the camera freely.\n" +
				"This can cause some glitches. To fix it, rotate back again\n" +
				"or do a complete circle.", 20, 75);

			//Add debug toggle
			tabPanel.AddCheckbox("debug", 0, 135, parent.enableDebugDisplay,
			"Show debug info in-game.")
			.OnChange += (isChecked) => {
				parent.enableDebugDisplay = isChecked;
				parent.SaveConfig();
			};
			tabPanel.AddLabel("Show Debug Info", 20, 135);

			//Add height scale
			tabPanel.AddLabel("Height Scaling Factor:", 0, 165);
			tabPanel.AddSlider(name: "heightScale", x: 200, y: 165,
				value: parent.heightScaleFactor * 100, min: 0,
				max: 200, step: 10,
				tooltip: "Multiplier to move the camera slower when zoomed in.")
				.eventValueChanged += (component, value) => {
					parent.heightScaleFactor = value / 100;
					parent.SaveConfig();
				};
			tabPanel.AddLabel("Increasing this will make the camera move slower\n" +
				"when it's zoomed in, so that you can move more precisely.\n" +
				"Set it to 0 to disable this behaviour.\n" +
				"This only applies to camera movement done by this mod.", 20, 185);
		}


		protected void AddInstructionsTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			UIButton tabButton = tabStrip.AddTab("Instructions",
				out UIPanelWrapper tabPanel, "How to use this mod");

			tabPanel.AddLabel("You can select an input device, assign it to control the\n" +
				"camera in some way, and add modifier buttons.\n\n" +
				"· Device: Selects which device to use. You can use USB Human Input Devices\n" +
				"  (eg mice, joysticks) as well as Unity's built-in input system.\n" +
				"  (USB devices must be enabled in the General tab.)\n\n" +
				"· Input Axis: Selects which axis to use on the device.\n" +
				"  Use the Devices tab to see which is which.\n\n" +
				"· Action: Selects what this axis will do.\n\n" +
				"· Invert: Makes the axis move in the opposite direction.\n\n" +
				"· Dead Zone: Inputs smaller than this (after adding the Offset) are ignored.\n" +
				"  Useful for old joysticks that jitter when not being moved.\n\n" +
				"· Offset: Added to the raw axis input. Useful for eg sliders that range from\n" +
				"  0 to 100 when you want -50 to 50.\n\n" +
				"· Smoothing: Whether to use Unity's input smoothing algorithm to make the\n" +
				"  movement more smooth.\n\n" +
				"· Relative: Uses the distance the axis has moved instead of its current\n" +
				"  position. Useful for mice and trackballs.\n\n" +
				"· Modifiers: You can use the same axis for multiple controls by assigning\n" +
				"  modifiers to them. For example, create two inputs:\n" +
				"  · Axis: X Axis;  Action: Move Left/Right;  Modifiers: Shift not held\n" +
				"  · Axis: X Axis;  Action: Turn Left/Right;  Modifiers: Shift held\n" +
				"  Now you can use the same input for both moving and rotating.\n\n" +
				"  Mouse buttons and up to 20 joystick buttons can be used as modifiers;\n" +
				"  just scroll down in the list (even if the scrollbar is missing).\n" +
				"  If an input has multiple modifiers, they must all be satisfied.\n\n" +
				"If things get completely broken, use the Reset Camera button in the General\n" +
				"tab to get back to a sane state."
				, 0, 0);
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
			//+500 because no idea
			scrollbar.maxValue = scrollablePanel.height + scrollbar.scrollSize + 500;
			scrollablePanel.Panel.parent.height = scrollablePanel.height;
			//scrollbar.autoSize = true;
			scrollbar.autoHide = true;
			scrollbar.incrementAmount = 120; //about the height of one entry
			btnAdd.BringToFront();
		}

		protected void AddCurrentInputsTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			//Add current value display
			UIButton tabButton = tabStrip.AddTab("Devices",
				out UIPanelWrapper tabPanel, "Current input values");

			int y = 0;
			foreach(var src in parent.GetInputSources()) {
				y = AddCurrentValues(src.Key, src.Value, tabPanel, y) + 20;
			}
		}

		protected void AddAboutTab(UICustomTabStrip tabStrip, UITabContainer tabContainer) {
			UIButton tabButton = tabStrip.AddTab("About",
				out UIPanelWrapper tabPanel, "About this mod");

			tabPanel.AddLabel($"Joystick Camera Control v{parent.VersionString}" +
				" by Rena\n" +
				"\nThanks to:\n" +
				"· Egi, boformer, Elektrix for help with the API.\n" +
				"· tomarus for the UI code I \"borrowed\" from TerrainGen.\n" +
				"· andrief for ilspymono, and the ilspy developers,\n" +
				"  for tools necessary to find axis definitions.\n" +
				"· Icons from Gnome project and The Noun Project.\n" +
				"· My cat for reminding me to take breaks.\n"
				, 0, 0);

			tabPanel.AddButton("My mods on Steam Workshop", 0, 180, 250, 30).eventClicked += (component, eventParam) => {
				Application.OpenURL("https://steamcommunity.com/id/renakunisaki/myworkshopfiles/?appid=255710");
			};
			tabPanel.AddButton("Source Code on Github", 250, 180, 250, 30).eventClicked += (component, eventParam) => {
				Application.OpenURL("https://github.com/RenaKunisaki/CSL-JoystickCameraControl");
			};
			tabPanel.AddButton("My Twitter", 0, 210, 125, 30).eventClicked += (component, eventParam) => {
				Application.OpenURL("https://twitter.com/RenaKunisaki");
			};
			tabPanel.AddButton("Buy me a Coffee", 125, 210, 200, 30).eventClicked += (component, eventParam) => {
				Application.OpenURL("https://ko-fi.com/renakunisaki");
			};
		}

		/// <summary>
		/// Adds the display of current joystick inputs.
		/// </summary>
		/// <remarks>This is helpful to find which axis maps to which physical
		/// input on the joystick.</remarks>
		protected int AddCurrentValues(string name, InputSource source, UIPanelWrapper parent, int y) {
			parent.AddLabel(name, 100, y).textScale = 1.5f;

			if(source == null || source is PlaceholderInputSource) {
				parent.AddLabel("Device not found or not available", 10, y + 30);
				return y + 40;
			}

			var axisNames = source.GetAxisNames();
			var axes = source.GetAxes();

			//groupRoot.relativePosition = new Vector3(0, 0, 0);
			var groupRoot = parent.Panel;
			int x = 0;
			UIPanelWrapper panel = null;
			bool isFirst = true;

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
			InputSource source = sources[input.inputSource.Name];
			var devNames = sources.Keys.ToList();

			string[] axisNames;
			if(input.inputSource != null) axisNames = input.inputSource.GetAxisNames();
			else axisNames = new string[] { "<device not found>" };
			UIDropDown ddInput = null;

			//Add device dropdown.
			panel.AddLabel("Device:", 0, 5);
			UIDropDown ddDevice = panel.AddDropdown(
				name: "device", x: 70, y: 0, items: devNames.ToArray(),
				tooltip: "Which input device to use.");
			ddDevice.selectedIndex = devNames.IndexOf(input.inputSource.Name);
			ddDevice.eventSelectedIndexChanged += (component, value) => {
				input.inputSource = sources[devNames[value]];
				ddInput.selectedIndex = 0;
				if(input.inputSource != null) ddInput.items = input.inputSource.GetAxisNames();
				else ddInput.items = new string[] { "<device not found>" };
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
			.OnChange += (isChecked) => {
				input.sign = isChecked ? -1 : 1;
				parent.SaveConfig();
			};
			panel.AddLabel("Invert", 20, 60);

			//Add smoothing checkbox.
			panel.AddCheckbox("smoothing", 85, 60, input.smoothing,
			"Use Unity's input smoothing.")
			.OnChange += (isChecked) => {
				input.smoothing = isChecked;
				parent.SaveConfig();
			};
			panel.AddLabel("Smoothing", 105, 60);

			//Add relative checkbox.
			panel.AddCheckbox("relative", 200, 60, input.relative,
			"Use relative input values.")
			.OnChange += (isChecked) => {
				input.relative = isChecked;
				parent.SaveConfig();
			};
			panel.AddLabel("Relative", 220, 60);

			//Add delete button.
			UIButton btnDelete = panel.AddButton("Delete Input", 575, 0, 110, 20,
				"Delete this input.");
			btnDelete.eventClicked += (component, eventParam) => {
				parent.RemoveInput(input);
				parent.SaveConfig();
				//don't do this, it removes the entire page
				//root.parent.RemoveUIComponent(root);
				//UnityEngine.Object.Destroy(root);
				container.Remove(panel.Panel);
				//BUG: this leaves an empty space where the panel was.
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
			chkHeld.OnChange += (isChecked) => {
				mod.condition = isChecked ?
					JoystickInputDef.ModifierCondition.HELD
					: JoystickInputDef.ModifierCondition.NOT_HELD;
				chkNotHeld.isChecked = !isChecked;
				parent.SaveConfig();
			};
			chkNotHeld.OnChange += (isChecked) => {
				mod.condition = isChecked ?
					JoystickInputDef.ModifierCondition.NOT_HELD
					: JoystickInputDef.ModifierCondition.HELD;
				chkHeld.isChecked = !isChecked;
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
