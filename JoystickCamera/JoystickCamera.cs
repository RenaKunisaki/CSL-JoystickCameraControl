using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace JoystickCamera {
	public class JoystickCamera: ThreadingExtensionBase, IUserMod {
		public string Name => "Joystick Camera Control";
		public string Description => "Use a joystick to control the camera.";
		public readonly float PI_OVER_180 = Mathf.PI / 180f;
		protected List<JoystickInputDef> inputs;

		public JoystickCamera() {
			Log("Instantiated");

			//Fun fact: this mod could be reduced to approximately one line:
			//cameraController.m_analogController = true;
			//This activates a built-in but apparently hidden analog mode.
			//But, that mode isn't configurable, and has some issues
			//(eg I can zoom out but not in).

			inputs = new List<JoystickInputDef>();
			AddDefaultInputs();
		}

		public List<JoystickInputDef> GetInputs() {
			return inputs;
		}

		public JoystickInputDef AddInput() {
			JoystickInputDef input = new JoystickInputDef();
			inputs.Add(input);
			return input;
		}

		public void RemoveInput(JoystickInputDef input) {
			inputs.Remove(input);
		}

		protected void AddDefaultInputs() {
			inputs.Add(new JoystickInputDef {
				axis = JoystickInputDef.Axis.HORIZONTAL,
				speed = 100,
				output = JoystickInputDef.Output.CAMERA_MOVE_X,
			});
			inputs.Add(new JoystickInputDef {
				axis = JoystickInputDef.Axis.VERTICAL,
				speed = 100,
				output = JoystickInputDef.Output.CAMERA_MOVE_Z,
			});
			inputs.Add(new JoystickInputDef {
				axis = JoystickInputDef.Axis.ROTATION_HORIZONTAL_CAMERA,
				speed = 5,
				output = JoystickInputDef.Output.CAMERA_TURN_X,
			});
			inputs.Add(new JoystickInputDef {
				axis = JoystickInputDef.Axis.ROTATION_VERTICAL_CAMERA,
				speed = 5,
				output = JoystickInputDef.Output.CAMERA_ZOOM,
			});
		}

		#region logging

		/// <summary>
		/// Writes a message to the debug logs. "NoOffScreenScroll" tag
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
			new SettingsPanel(this, helper).Run();
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
		/// Called once per rendered frame.
		/// Thread: Main
		/// </summary>
		/// <param name="realTimeDelta">Seconds since previous frame.</param>
		/// <param name="simulationTimeDelta">Smoothly interpolated to be used
		/// from main thread. On normal speed it is roughly same as realTimeDelta.</param>
		public override void OnUpdate(float realTimeDelta, float simulationTimeDelta) {
			GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
			if(gameObject == null) return;

			//XXX add modifiers when holding shift, joy button, etc.
			//maybe even buttons to jump to camera positions?
			//settings per joystick?
			//could also bind buttons to game actions, but Steam already
			//lets you do that by binding them to keys...
			//maybe useful if you don't want a key
			//also, have world-relative movement be a separate axis,
			//so you can use both.

			float t = realTimeDelta * 60; //should be ~1/60 of a second
			Vector3 translateRelative = new Vector3(); //screen relative movement
			Vector3 translateWorld = new Vector3(); //compass movement
			Vector2 rotate = new Vector2();
			float zoom = 0;

			foreach(JoystickInputDef input in this.inputs) {
				float v = input.Read() * t;
				switch(input.output) {
					case JoystickInputDef.Output.CAMERA_MOVE_X:
						translateRelative.x = v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_Y:
						translateRelative.y = v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_Z:
						translateRelative.z = v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_NS:
						translateWorld.x = v;
						break;
					case JoystickInputDef.Output.CAMERA_MOVE_EW:
						translateWorld.z = v;
						break;
					case JoystickInputDef.Output.CAMERA_ZOOM:
						zoom = v;
						break;
					case JoystickInputDef.Output.CAMERA_TURN_X:
						rotate.x = v;
						break;
					case JoystickInputDef.Output.CAMERA_TURN_Y:
						rotate.y = v;
						break;
				}
			}

			CameraController cameraController = gameObject.GetComponent<CameraController>();
			Vector3 currentPos = cameraController.m_currentPosition;
			Vector3 targetPos = currentPos;
			translateRelative = cameraController.transform.localToWorldMatrix.MultiplyVector(translateRelative);

			targetPos.x += translateRelative.x + translateWorld.x;
			targetPos.y += translateRelative.y + translateWorld.y;
			targetPos.z += translateRelative.z + translateWorld.z;

			cameraController.m_targetPosition = targetPos;
			cameraController.m_targetAngle.x += rotate.x;
			cameraController.m_targetAngle.y += rotate.y;
			cameraController.m_targetSize += zoom;
			//cameraController.m_targetHeight += translateWorld.z;
		}

		#endregion ThreadingExtensionBase
	}
}