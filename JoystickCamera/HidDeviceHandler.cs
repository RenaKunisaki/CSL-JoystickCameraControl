using System;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace JoystickCamera {
	/// <summary>
	/// Handles HID communication.
	/// </summary>
	public class HidDeviceHandler {
		protected HidDevice device;
		protected HidStream hidStream;
		protected ReportDescriptor reportDescriptor;
		protected DeviceItem deviceItem;
		protected string deviceName;
		protected byte[] inputReportBuffer;
		protected HidDeviceInputReceiver inputReceiver;
		protected DeviceItemInputParser inputParser;
		protected List<bool> buttons;
		protected Usage usage;

		public readonly Dictionary<Usage, string> usageNames = new Dictionary<Usage, string> {
			{ Usage.GenericDesktopX, "X Axis" },
			{ Usage.GenericDesktopY, "Y Axis" },
			{ Usage.GenericDesktopZ, "Z Axis" },
			{ Usage.GenericDesktopRx, "Rx Axis" },
			{ Usage.GenericDesktopRy, "Ry Axis" },
			{ Usage.GenericDesktopRz, "Rz Axis" },
			{ Usage.GenericDesktopWheel, "Wheel" },
			{ Usage.GenericDesktopSlider, "Slider" }, //XXX are Slider and Dial inputs or devices?
			{ Usage.GenericDesktopDial, "Dial" },
		};

		public class Axis {
			public String name;
			public Usage usage;
			public double value = 0;
		}

		protected Dictionary<Usage, Axis> axes;

		public HidDeviceHandler(HidDevice device) {
			this.device = device;
			this.deviceName = device.GetFriendlyName();
			buttons = new List<bool>();
			axes = new Dictionary<Usage, Axis>();
			Open();
		}

		public string Name { get => deviceName; }

		/// <summary>
		/// Get all available devices.
		/// </summary>
		/// <returns>Array of HidDeviceHandler.</returns>
		public static HidDeviceHandler[] GetDevices() {
			var result = new List<HidDeviceHandler>();
			var devices = DeviceList.Local.GetHidDevices();
			JoystickCamera.Log($"Found {devices.Count()} devices");
			foreach(var dev in devices) {
				string name = "<error getting device name>";
				try {
					name = dev.GetFriendlyName();
					JoystickCamera.Log($"Device: {dev.GetFriendlyName()}");
					if(CanUseDevice(dev)) {
						JoystickCamera.Log($"Opening device: {name}");
						result.Add(new HidDeviceHandler(dev));
					}
				}
				catch(Exception ex) {
					var s = ex.ToString();
					//why the shit is DeviceUnauthorizedAccessException protected?
					if(s.Contains("DeviceUnauthorizedAccessException")) {
						JoystickCamera.Log($"Access denied to device: {name}");
					}
					else JoystickCamera.Log($"Error opening device {name}: {ex}");
				}
			}
			return result.ToArray();
		}

		protected static bool CanUseDevice(HidDevice device) {
			var reportDescriptor = device.GetReportDescriptor();
			foreach(var deviceItem in reportDescriptor.DeviceItems) {
				foreach(var usage in deviceItem.Usages.GetAllValues()) {
					var us = (HidSharp.Reports.Usage)usage;
					JoystickCamera.Log(string.Format("Usage: {0:X4} {1}", usage, us));
					if(IsSupportedUsage(us)) return true;
				}
			}
			return false;
		}

		protected static bool IsSupportedUsage(Usage usage) {
			if(usage == Usage.GenericDesktopMouse) return true;
			if(usage == Usage.GenericDesktopJoystick) return true;
			if(usage == Usage.GenericDesktopGamepad) return true;
			if(usage == Usage.GenericDesktopMultiaxisController) return true;
			//XXX more device types?
			return false;
		}

		public string[] GetAxisNames() {
			var names = new List<string>(axes.Count);
			foreach(var axis in axes.Values) {
				names.Add(axis.name);
			}
			return names.ToArray();
		}

		/// <summary>
		/// Get the current axis states.
		/// </summary>
		/// <returns>The axes.</returns>
		public Dictionary<string, double> GetAxes() {
			var result = new Dictionary<string, double>();
			foreach(var axis in axes.Values) {
				result[axis.name] = axis.value;
			}
			return result;
		}

		public int GetButtonCount() {
			return buttons.Count;
		}

		/// <summary>
		/// .Get the current button states.
		/// </summary>
		/// <returns>The buttons.</returns>
		public bool[] GetButtons() {
			return buttons.ToArray();
		}

		/// <summary>
		/// Try to open the device.
		/// </summary>
		protected void Open() {
			hidStream = device.Open();
			hidStream.ReadTimeout = 1000;
			reportDescriptor = device.GetReportDescriptor();

			foreach(var item in reportDescriptor.DeviceItems) {
				foreach(var usage in item.Usages.GetAllValues()) {
					var us = (Usage)usage;
					if(us == Usage.GenericDesktopMouse
					|| us == Usage.GenericDesktopJoystick) {
						//XXX more device types?
						//Get the report descriptor info and read the descriptors
						this.usage = us;
						Log($"Opening device {us} {device.GetFriendlyName()}");
						deviceItem = item;
						inputReportBuffer = new byte[device.GetMaxInputReportLength()];
						inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
						inputParser = deviceItem.CreateDeviceItemInputParser();
						inputReceiver.Start(hidStream);

						foreach(var inputReport in reportDescriptor.InputReports) {
							foreach(var usg in inputReport.GetAllUsages()) {
								Log($"Input report: {(Usage)usg}");
								//If this is a button or an axis, add it.
								//XXX get info such as the neutral value, range, etc.
								if(usg >= (int)Usage.Button1
								&& usg <= (int)Usage.Button31) {
									buttons.Add(false);
								}
								else if(usageNames.ContainsKey((HidSharp.Reports.Usage)usg)) {
									axes[(HidSharp.Reports.Usage)usg] = new Axis {
										name = usageNames[(HidSharp.Reports.Usage)usg],
										usage = (HidSharp.Reports.Usage)usg,
									};
								}
							}
						}
						return;
					}
				}
			}
			throw new NotSupportedException("Unsupported device type");
		}

		/// <summary>
		/// Poll the device and update state.
		/// </summary>
		public void Update() {
			if(!inputReceiver.IsRunning) {
				throw new IOException("Device disconnected");
			}

			if(this.usage == Usage.GenericDesktopMouse) {
				//Mice only report relative movements, and only when actually moved.
				//If we didn't get any report, then the movement is zero.
				foreach(var axis in axes.Values) {
					axis.value = 0;
				}
			}

			// Periodically check if the receiver has any reports.
			while(inputReceiver.TryRead(inputReportBuffer, 0, out Report report)) {
				// Parse the report if possible.
				// This will return false if (for example) the report applies to a different DeviceItem.
				if(inputParser.TryParseReport(inputReportBuffer, 0, report)) {
					while(inputParser.HasChanged) {
						var idx = inputParser.GetNextChangedIndex();
						var prevVal = inputParser.GetPreviousValue(idx);
						var newVal = inputParser.GetValue(idx);
						var usage = (HidSharp.Reports.Usage)newVal.Usages.FirstOrDefault();
						//var prevPhysVal = prevVal.GetPhysicalValue();
						//var newPhysVal = newVal.GetPhysicalValue();
						var prevLogVal = prevVal.GetLogicalValue();
						var newLogVal = newVal.GetLogicalValue();
						//var prevLogVal = prevVal.GetScaledValue(-100.0, 100.0);
						//var newLogVal = newVal.GetScaledValue(-100.0, 100.0);

						//If this is a button or axis, update it.
						if((int)usage >= (int)HidSharp.Reports.Usage.Button1
 						&& (int)usage <= (int)HidSharp.Reports.Usage.Button31) {
							buttons[(int)usage - (int)HidSharp.Reports.Usage.Button1] = (newLogVal > 0);
						}
						else if(usageNames.ContainsKey(usage)) {
							if(this.usage == Usage.GenericDesktopJoystick) {
								newLogVal -= 127; //XXX probably better way to do this
							}
							axes[usage].value = newLogVal;
						}
						//Log($"Input change: {usage}: {prevPhysVal} -> {newPhysVal}");
					}
				}
			}
		}

		public void Log(String message) {
			JoystickCamera.Log($"[{deviceName}] {message}");
		}
	}
}
