using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace JoystickCamera {
	public class JoystickCamera: ThreadingExtensionBase, IUserMod {
		public string Name => "Joystick Camera Control";
		public string Description => "Use a joystick to control the camera.";

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
			float x = Input.GetAxis("Horizontal") * 100;
			float y = Input.GetAxis("Vertical") * 100;
			Vector2 translate = new Vector2(x, y);

			GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
			if(gameObject == null) return;

			CameraController cameraController = gameObject.GetComponent<CameraController>();
			Vector3 currentPos = cameraController.m_currentPosition;
			float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

			Vector2 vectorWithAngle;

			vectorWithAngle.x = translate.x * Mathf.Cos(cameraAngle) - translate.y * Mathf.Sin(cameraAngle);
			vectorWithAngle.y = translate.x * Mathf.Sin(cameraAngle) + translate.y * Mathf.Cos(cameraAngle);

			Vector3 targetPos = currentPos;
			targetPos.x -= vectorWithAngle.x;
			targetPos.z += vectorWithAngle.y;

			cameraController.m_targetPosition = targetPos;
		}

		#endregion ThreadingExtensionBase
	}
}
