using System;
using ColossalFramework.IO;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace JoystickCamera {
	/// <summary>
	/// Object containing the actual data saved/loaded to the config file.
	/// </summary>
	[XmlRoot("Configuration")]
	public class ConfigData {
		public class ModifierDef {
			public string button;
			public string condition;
		}

		public class InputDef {
			public string output;
			public string axis;
			public float speed;
			public float sign;
			public float deadZone;
			public float offset;
			public bool smoothing = true;

			[XmlArray("modifiers")]
			public List<ModifierDef> modifiers;
		}

		[XmlArray("inputs")]
		public List<InputDef> inputs;

		/// <summary>
		/// Parse the input list and return the JoystickInputDefs.
		/// </summary>
		/// <returns>The inputs.</returns>
		/// <remarks>Ignores inputs with invalid settings.</remarks>
		public List<JoystickInputDef> GetInputs() {
			var result = new List<JoystickInputDef>(inputs.Count);
			foreach(var input in inputs) {
				int outputIdx = Array.IndexOf(JoystickInputDef.OutputName, input.output);
				if(outputIdx < 0) {
					Log($"Invalid output '{input.output}'");
					continue;
				}

				int axisIdx = Array.IndexOf(JoystickInputDef.axisNames, input.axis);
				if(axisIdx < 0) {
					Log($"Invalid axis '{input.axis}'");
					continue;
				}

				var inputDef = new JoystickInputDef(
					(JoystickInputDef.Axis)axisIdx,
					(JoystickInputDef.Output)outputIdx,
					input.speed, input.sign, input.deadZone, input.offset, input.smoothing);

				foreach(var mod in input.modifiers) {
					int btnIdx = Array.IndexOf(JoystickInputDef.modifierButtonName, mod.button);
					if(btnIdx < 0) {
						Log($"Invalid modifier button '{mod.button}'");
						continue;
					}

					int condIdx = Array.IndexOf(JoystickInputDef.modifierConditionName, mod.condition);
					if(condIdx < 0) {
						Log($"Invalid modifier condition '{mod.condition}'");
						continue;
					}

					inputDef.modifiers.Add(new JoystickInputDef.Modifier(
						(JoystickInputDef.ModifierButton)btnIdx,
						(JoystickInputDef.ModifierCondition)condIdx));
				}

				result.Add(inputDef);
			}
			return result;
		}

		/// <summary>
		/// Replace the input list with the specified list.
		/// </summary>
		/// <param name="inputs">Inputs.</param>
		public void SetInputs(List<JoystickInputDef> inputs) {
			this.inputs = new List<InputDef>(inputs.Count);
			foreach(var input in inputs) {
				var item = new InputDef {
					output = input.Name,
					axis = input.AxisName,
					speed = input.speed,
					sign = input.sign,
					deadZone = input.deadZone,
					offset = input.offset,
					smoothing = input.smoothing,
				};
				item.modifiers = new List<ModifierDef>(input.modifiers.Count);
				foreach(var mod in input.modifiers) {
					item.modifiers.Add(new ModifierDef {
						button = JoystickInputDef.modifierButtonName[(int)mod.button],
						condition = JoystickInputDef.modifierConditionName[(int)mod.condition],
					});
				}
				this.inputs.Add(item);
			}
		}

		protected static void Log(string message) {
			JoystickCamera.Log(message);
		}
	}

	/// <summary>
	/// Used to save and load the configuration.
	/// </summary>
	public class Configuration {
		protected string path;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:JoystickCamera.Configuration"/> class.
		/// </summary>
		/// <param name="FileName">Config file name to use.</param>
		public Configuration(string FileName = "JoystickCameraConfig.xml") {
			string FilePath = DataLocation.localApplicationData;
			this.path = Path.Combine(FilePath, FileName);
			JoystickCamera.Log($"Config path is {this.path}");
		}

		/// <summary>
		/// Load the config from the file.
		/// </summary>
		/// <returns>The config.</returns>
		public ConfigData Load() {
			var serializer = new XmlSerializer(typeof(ConfigData));
			using(var streamReader = new StreamReader(path)) {
				return (ConfigData)serializer.Deserialize(streamReader);
			}
		}

		/// <summary>
		/// Save the config to the file.
		/// </summary>
		/// <param name="data">Config to save.</param>
		public void Save(ConfigData data) {
			var serializer = new XmlSerializer(typeof(ConfigData));
			using(var streamWriter = new StreamWriter(path)) {
				serializer.Serialize(streamWriter, data);
			}
		}
	}
}
