using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JoystickCamera {
	public class Axis {
		protected float value, prevValue, smoothValue;

		public void SetValue(float value) {
			prevValue = this.value;
			this.smoothValue = Mathf.MoveTowards(prevValue, value, Time.deltaTime);
			this.value = value;
		}

		public void SetValue(double value) {
			SetValue((float)value);
		}

		public float GetValue(bool smooth = false) {
			if(smooth) return smoothValue;
			else return value;
		}

		public float GetPreviousValue() {
			return prevValue;
		}
	}

	public class Button {
		protected bool state, prevState;

		public void SetState(bool state) {
			prevState = this.state;
			this.state = state;
		}

		public bool GetState() {
			return state;
		}

		public bool GetPreviousState() {
			return prevState;
		}
	}

	/// <summary>
	/// Base class for input sources.
	/// </summary>
	public class InputSource {
		protected Dictionary<string, Axis> axes;
		protected Dictionary<string, Button> buttons;
		protected string name;
		public string Name => name;

		public InputSource(string name = null) {
			if(name != null) this.name = name;
			axes = new Dictionary<string, Axis>();
			buttons = new Dictionary<string, Button>();
		}

		/// <summary>
		/// Called each frame to update the input states.
		/// </summary>
		public virtual void Update() {
			//default: do nothing
		}

		public Dictionary<string, Axis> GetAxes() {
			return axes;
		}

		public Dictionary<string, Button> GetButtons() {
			return buttons;
		}

		public string[] GetAxisNames() {
			return axes.Keys.ToArray();
		}

		public string[] GetButtonNames() {
			return buttons.Keys.ToArray();
		}

		public Axis GetAxis(string name) {
			try {
				return this.axes[name];
			}
			catch(KeyNotFoundException) {
				JoystickCamera.Log($"Axis '{name}' not found on InputSource '{name}'");
				return null;
			}
		}

		public Button GetButton(string name) {
			try {
				return this.buttons[name];
			}
			catch(KeyNotFoundException) {
				JoystickCamera.Log($"Button '{name}' not found on InputSource '{name}'");
				return null;
			}
		}
	}
}
