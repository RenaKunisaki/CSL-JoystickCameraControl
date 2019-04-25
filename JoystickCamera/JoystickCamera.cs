using System;
using System.Collections.Generic;
using System.IO;
using ICities;
using UnityEngine;
using static JoystickCamera.JoystickInputDef;

namespace JoystickCamera {
	public class JoystickCamera: ThreadingExtensionBase, IUserMod {
		public string Name => "Joystick Camera Control";
		public string Description => "Use a joystick to control the camera.";
		public readonly float PI_OVER_180 = Mathf.PI / 180f;
		protected List<JoystickInputDef> inputs;
		protected SettingsPanel settingsPanel;
		protected DebugCameraDisplay debugDisplay;
		public bool enableDebugDisplay = false;
		protected bool didMoveWithMouse = false;

		public JoystickCamera() {
			Log("Instantiated");

			//Fun fact: this mod could be reduced to approximately one line:
			//cameraController.m_analogController = true;
			//This activates a built-in but apparently hidden analog mode.
			//But, that mode isn't configurable, and has some issues
			//(eg I can zoom out but not in).

			inputs = new List<JoystickInputDef>();
			try {
				LoadConfig();
			}
			catch(FileNotFoundException) {
				Log("Config file not found");
				AddDefaultInputs();
				SaveConfig();
			}
			catch(Exception ex) {
				Log($"Error loading config file: {ex}");
				AddDefaultInputs();
			}
		}

		/// <summary>
		/// Saves the config.
		/// </summary>
		public void SaveConfig() {
			Log("Saving config...");
			ConfigData data = new ConfigData();
			data.SetInputs(GetInputs());
			(new Configuration()).Save(data);
			Log("Saved config.");
		}

		/// <summary>
		/// Loads the config.
		/// </summary>
		public void LoadConfig() {
			Log("Loading config...");
			var data = (new Configuration()).Load();
			this.inputs = data.GetInputs();
			Log("Loaded config.");
		}

		/// <summary>
		/// Get the input list.
		/// </summary>
		/// <returns>The inputs.</returns>
		public List<JoystickInputDef> GetInputs() {
			return inputs;
		}

		/// <summary>
		/// Add a new input with default settings.
		/// </summary>
		/// <returns>The input.</returns>
		public JoystickInputDef AddInput() {
			JoystickInputDef input = new JoystickInputDef();
			inputs.Add(input);
			return input;
		}

		/// <summary>
		/// Remove the specified input.
		/// </summary>
		/// <param name="input">Input.</param>
		public void RemoveInput(JoystickInputDef input) {
			inputs.Remove(input);
		}

		/// <summary>
		/// Adds the default inputs, when no config file is available.
		/// </summary>
		protected void AddDefaultInputs() {
			inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.HORIZONTAL,
				output: JoystickInputDef.Output.CAMERA_MOVE_X,
				speed: 100,
				modifiers: new Modifier[] {
					new Modifier(ModifierButton.SHIFT_ANY, ModifierCondition.NOT_HELD),
				}
			));
			inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.VERTICAL,
				output: JoystickInputDef.Output.CAMERA_MOVE_Z,
				speed: 100
			));
			inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.ROTATION_HORIZONTAL_CAMERA,
				output: JoystickInputDef.Output.CAMERA_TURN_X,
				speed: 5
			));
			inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.ROTATION_VERTICAL_CAMERA,
				output: JoystickInputDef.Output.CAMERA_ZOOM,
				speed: 5
			));
			/* inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.HORIZONTAL,
				output: JoystickInputDef.Output.CAMERA_MOVE_EW,
				speed: 100,
				modifiers: new Modifier[] {
					new Modifier(ModifierButton.SHIFT_ANY, ModifierCondition.HELD),
				}
			));
			inputs.Add(new JoystickInputDef(
				axis: JoystickInputDef.Axis.VERTICAL,
				output: JoystickInputDef.Output.CAMERA_MOVE_NS,
				speed: 100,
				modifiers: new Modifier[] {
					new Modifier(ModifierButton.SHIFT_ANY, ModifierCondition.HELD),
				}
			)); */
		}

		#region logging

		/// <summary>
		/// Writes a message to the debug logs. "JoystickCamera" tag
		/// and timestamp are automatically prepended.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void Log(String message) {
			String time = DateTime.Now.ToUniversalTime()
				.ToString("yyyyMMdd' 'HHmmss'.'fff");
			message = $"{time}: {message}{Environment.NewLine}";
			try {
				UnityEngine.Debug.Log("[JoystickCamera] " + message);
			}
			catch(NullReferenceException) {
				//Happens if Unity logger isn't set up yet
			}
		}

		#endregion logging

		#region Settings UI

		/// <summary>
		/// Called to display the UI in the settings window.
		/// </summary>
		/// <param name="helper">UI Helper.</param>
		public void OnSettingsUI(UIHelperBase helper) {
			this.settingsPanel = new SettingsPanel(this, helper);
			this.settingsPanel.Run();
		}

		#endregion Settings UI

		#region ThreadingExtensionBase

		/// <summary>
		/// Called by the game after this instance is created.
		/// </summary>
		/// <param name="threading">The threading.</param>
		public override void OnCreated(IThreading threading) {
			base.OnCreated(threading);
			Log("Created");
			int numSticks = 0;
			foreach(string name in Input.GetJoystickNames()) {
				Log($"Joystick {numSticks}: {name}");
				numSticks++;
			}
			Log($"Found {numSticks} joysticks");
		}

		/// <summary>
		/// Called by the game before this instance is about to be destroyed.
		/// </summary>
		public override void OnReleased() {
			Log("Released");
			base.OnReleased();
		}

		/// <summary>
		/// Get the current modifier keys' states.
		/// </summary>
		/// <returns>The modifiers.</returns>
		protected Dictionary<ModifierButton, bool> GetModifiers() {
			var modifiers = new Dictionary<ModifierButton, bool> {
				{ ModifierButton.SHIFT_LEFT, Input.GetKey(KeyCode.LeftShift) },
				{ ModifierButton.SHIFT_RIGHT, Input.GetKey(KeyCode.RightShift) },
				{ ModifierButton.CTRL_LEFT, Input.GetKey(KeyCode.LeftControl) },
				{ ModifierButton.CTRL_RIGHT, Input.GetKey(KeyCode.RightControl) },
				{ ModifierButton.ALT_LEFT, Input.GetKey(KeyCode.LeftAlt) },
				{ ModifierButton.ALT_RIGHT, Input.GetKey(KeyCode.RightAlt) },
				{ ModifierButton.CMD_LEFT, Input.GetKey(KeyCode.LeftCommand) },
				{ ModifierButton.CMD_RIGHT, Input.GetKey(KeyCode.RightCommand) },
				{ ModifierButton.WIN_LEFT, Input.GetKey(KeyCode.LeftWindows) },
				{ ModifierButton.WIN_RIGHT, Input.GetKey(KeyCode.RightWindows) },
				//Unity uses 0-based buttons but literally no joystick/mouse ever does
				{ ModifierButton.MOUSE1, Input.GetKey(KeyCode.Mouse0) },
				{ ModifierButton.MOUSE2, Input.GetKey(KeyCode.Mouse1) },
				{ ModifierButton.MOUSE3, Input.GetKey(KeyCode.Mouse2) },
				{ ModifierButton.MOUSE4, Input.GetKey(KeyCode.Mouse3) },
				{ ModifierButton.MOUSE5, Input.GetKey(KeyCode.Mouse4) },
				{ ModifierButton.MOUSE6, Input.GetKey(KeyCode.Mouse5) },
				{ ModifierButton.MOUSE7, Input.GetKey(KeyCode.Mouse6) },
				{ ModifierButton.BUTTON1, Input.GetKey(KeyCode.JoystickButton0) },
				{ ModifierButton.BUTTON2, Input.GetKey(KeyCode.JoystickButton1) },
				{ ModifierButton.BUTTON3, Input.GetKey(KeyCode.JoystickButton2) },
				{ ModifierButton.BUTTON4, Input.GetKey(KeyCode.JoystickButton3) },
				{ ModifierButton.BUTTON5, Input.GetKey(KeyCode.JoystickButton4) },
				{ ModifierButton.BUTTON6, Input.GetKey(KeyCode.JoystickButton5) },
				{ ModifierButton.BUTTON7, Input.GetKey(KeyCode.JoystickButton6) },
				{ ModifierButton.BUTTON8, Input.GetKey(KeyCode.JoystickButton7) },
				{ ModifierButton.BUTTON9, Input.GetKey(KeyCode.JoystickButton8) },
				{ ModifierButton.BUTTON10, Input.GetKey(KeyCode.JoystickButton9) },
				{ ModifierButton.BUTTON11, Input.GetKey(KeyCode.JoystickButton10) },
				{ ModifierButton.BUTTON12, Input.GetKey(KeyCode.JoystickButton11) },
				{ ModifierButton.BUTTON13, Input.GetKey(KeyCode.JoystickButton12) },
				{ ModifierButton.BUTTON14, Input.GetKey(KeyCode.JoystickButton13) },
				{ ModifierButton.BUTTON15, Input.GetKey(KeyCode.JoystickButton14) },
				{ ModifierButton.BUTTON16, Input.GetKey(KeyCode.JoystickButton15) },
				{ ModifierButton.BUTTON17, Input.GetKey(KeyCode.JoystickButton16) },
				{ ModifierButton.BUTTON18, Input.GetKey(KeyCode.JoystickButton17) },
				{ ModifierButton.BUTTON19, Input.GetKey(KeyCode.JoystickButton18) },
				{ ModifierButton.BUTTON20, Input.GetKey(KeyCode.JoystickButton19) },
			};
			modifiers[ModifierButton.SHIFT_ANY] = modifiers[ModifierButton.SHIFT_LEFT] || modifiers[ModifierButton.SHIFT_RIGHT];
			modifiers[ModifierButton.CTRL_ANY] = modifiers[ModifierButton.CTRL_LEFT] || modifiers[ModifierButton.CTRL_RIGHT];
			modifiers[ModifierButton.ALT_ANY] = modifiers[ModifierButton.ALT_LEFT] || modifiers[ModifierButton.ALT_RIGHT];
			modifiers[ModifierButton.CMD_ANY] = modifiers[ModifierButton.CMD_LEFT] || modifiers[ModifierButton.CMD_RIGHT];
			modifiers[ModifierButton.WIN_ANY] = modifiers[ModifierButton.WIN_LEFT] || modifiers[ModifierButton.WIN_RIGHT];
			return modifiers;
		}

		/// <summary>
		/// Called once per rendered frame during gameplay.
		/// Thread: Main
		/// </summary>
		/// <param name="realTimeDelta">Seconds since previous frame.</param>
		/// <param name="simulationTimeDelta">Smoothly interpolated to be used
		/// from main thread. On normal speed it is roughly same as realTimeDelta.</param>
		public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
			GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
			if(gameObject == null) return;

			if(enableDebugDisplay) {
				if(this.debugDisplay == null) {
					Log("Creating debug display");
					this.debugDisplay = new DebugCameraDisplay();
				}
				this.debugDisplay.Update();
			}
			else {
				if(this.debugDisplay != null) {
					Log("Removing debug display");
					this.debugDisplay.Remove();
				}
				this.debugDisplay = null;
			}

			float t = realTimeDelta * 60; //should be ~1/60 of a second
			Vector3 translateRelative = new Vector3(0, 0, 0); //screen relative movement
			Vector3 translateWorld = new Vector3(0, 0, 0); //compass movement
			Vector2 rotate = new Vector2(0, 0);
			float zoom = 0;
			var modifiers = GetModifiers();

			//Read each input and assign the output to appropriate variable.
			foreach(JoystickInputDef input in this.inputs) {
				float v = input.Read(modifiers) * t;
				switch(input.output) {
					case JoystickInputDef.Output.CAMERA_MOVE_X:
						translateRelative.x += v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_Y:
						translateRelative.y += v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_Z:
						translateRelative.z += v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_NS:
						translateWorld.x += v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_EW:
						translateWorld.z += v;
						break;
					case JoystickInputDef.Output.CAMERA_ZOOM:
						zoom += v;
						break;
					case JoystickInputDef.Output.CAMERA_TURN_X:
						rotate.x += v;
						break;
					case JoystickInputDef.Output.CAMERA_TURN_Y:
						rotate.y += v;
						break;
					default:
						Log($"[BUG] Missing case for output {input.output}");
						break;
				}
			}

			//Get camera objects and current position
			CameraController cameraController = gameObject.GetComponent<CameraController>();
			Vector3 currentPos = cameraController.m_currentPosition;
			Vector3 targetPos = currentPos;

			//Transform relative to camera angle
			//Ignore Y rotation, only use X (which is actually the Y axis rotation >.>)
			//because otherwise, when looking down, "forward" is down, and the game
			//doesn't like to let you change height, and probably you want to pan
			//instead of zooming anyway.
			//Some of this borrowed from https://github.com/brittanygh/CS-CameraButtons/blob/master/CameraButtons/Class1.cs
			//translateRelative = cameraController.transform.localToWorldMatrix.MultiplyVector(translateRelative);
			var angle = cameraController.m_currentAngle.x * PI_OVER_180;
			float rx = translateRelative.x * Mathf.Cos(angle) + translateRelative.z * Mathf.Sin(angle);
			float rz = -translateRelative.x * Mathf.Sin(angle) + translateRelative.z * Mathf.Cos(angle);
			translateRelative.x = rx;
			translateRelative.z = rz;
			targetPos.x += translateRelative.x + translateWorld.x;
			targetPos.y += translateRelative.y + translateWorld.y;
			targetPos.z += translateRelative.z + translateWorld.z;

			float epsilon = 0.001f;
			bool anyMovement = (Mathf.Abs(translateRelative.x) > epsilon
			|| Mathf.Abs(translateRelative.y) > epsilon
			|| Mathf.Abs(translateRelative.z) > epsilon
			|| Mathf.Abs(translateWorld.x) > epsilon
			|| Mathf.Abs(translateWorld.y) > epsilon
			|| Mathf.Abs(translateWorld.z) > epsilon);
			bool anyRotation = (Mathf.Abs(rotate.x) > epsilon || Mathf.Abs(rotate.y) > epsilon);
			bool anyMouse = modifiers[ModifierButton.MOUSE1] || modifiers[ModifierButton.MOUSE2]
				|| modifiers[ModifierButton.MOUSE3] || modifiers[ModifierButton.MOUSE4]
				|| modifiers[ModifierButton.MOUSE5] || modifiers[ModifierButton.MOUSE6]
				|| modifiers[ModifierButton.MOUSE7];

			if(anyMovement) cameraController.ClearTarget();

			if(anyMouse && (anyMovement || anyRotation)) didMoveWithMouse = true;
			else if(!anyMouse) didMoveWithMouse = false;

			if(didMoveWithMouse) { //lock the mouse
				Cursor.lockState = CursorLockMode.Locked;
				//keep it visible or else the game will unlock it again.
				//it actually does hide, anyway.
				Cursor.visible = true;
			}
			else Cursor.lockState = CursorLockMode.None;

			cameraController.m_targetPosition = targetPos;
			cameraController.m_targetAngle.x += rotate.x;
			cameraController.m_targetAngle.y += rotate.y;
			cameraController.m_targetSize += zoom;
			//this seems to also be ignored...
			//is the camera Y position not where the camera actually is in world space?
			//should add a debug display of its position...
			cameraController.m_targetHeight = targetPos.y;
			//cameraController.m_currentHeight += translateRelative.z;
		}

		#endregion ThreadingExtensionBase
	}
}