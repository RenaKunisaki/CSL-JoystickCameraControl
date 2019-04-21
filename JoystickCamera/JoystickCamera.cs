using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoystickCamera {
	public class JoystickCamera: ThreadingExtensionBase, IUserMod {
		public string Name => "Joystick Camera Control";
		public string Description => "Use a joystick to control the camera.";
		protected float movementSpeed = 100; //How fast to move
		protected float signX = 1, signY = 1;
		protected float deadZone = 0;
		protected string axisNameX = "Horizontal";
		protected string axisNameY = "Vertical";
		protected bool worldRelative = false;

		public JoystickCamera() {
			Log("Instantiated");
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
			UIHelperBase group = helper.AddGroup("Camera Control");
			group.AddSlider("Movement Speed",
				min: 1, max: 1000, step: 10, defaultValue: this.movementSpeed,
				eventCallback: (val) => this.movementSpeed = val);
			group.AddCheckbox("Invert X axis", this.signX < 0 ? true : false,
				(isChecked) => this.signX = isChecked ? -1 : 1);
			group.AddCheckbox("Invert Y axis", this.signY < 0 ? true : false,
				(isChecked) => this.signY = isChecked ? -1 : 1);
			group.AddSlider("Dead Zone",
				min: 0, max: 100, step: 1, defaultValue: this.deadZone,
				eventCallback: (val) => this.deadZone = val / 100);
			group.AddTextfield("X Axis Name",
				defaultContent: this.axisNameX,
				eventChangedCallback: (text) => { },
				eventSubmittedCallback: (text) => this.axisNameX = text);
			group.AddTextfield("Y Axis Name",
				defaultContent: this.axisNameY,
				eventChangedCallback: (text) => { },
				eventSubmittedCallback: (text) => this.axisNameY = text);
			//XXX how the heck do we get a list of valid axis names?
			group.AddCheckbox("Move Relative to Screen", !this.worldRelative,
				(isChecked) => this.worldRelative = !isChecked);
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


			float x = Input.GetAxis(this.axisNameX);
			float y = Input.GetAxis(this.axisNameY);
			if(x > -deadZone && x < deadZone) x = 0;
			if(y > -deadZone && y < deadZone) y = 0;
			//Multiply time delta by 60 since it should be ~ 1/60 of a second
			Vector2 translate = new Vector2(
				x * movementSpeed * signX * (realTimeDelta * 60),
				y * movementSpeed * signY * (realTimeDelta * 60));

			CameraController cameraController = gameObject.GetComponent<CameraController>();
			Vector3 currentPos = cameraController.m_currentPosition;
			Vector3 targetPos = currentPos;

			if(worldRelative) {
				targetPos.x += translate.x;
				targetPos.z += translate.y;
			}
			else {
				float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

				Vector2 vectorWithAngle;
				vectorWithAngle.x = translate.x * Mathf.Cos(cameraAngle) - translate.y * Mathf.Sin(cameraAngle);
				vectorWithAngle.y = translate.x * Mathf.Sin(cameraAngle) + translate.y * Mathf.Cos(cameraAngle);

				targetPos.x -= vectorWithAngle.x;
				targetPos.z += vectorWithAngle.y;
			}

			cameraController.m_targetPosition = targetPos;
		}

		#endregion ThreadingExtensionBase
	}
}