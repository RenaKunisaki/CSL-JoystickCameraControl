﻿using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace JoystickCamera {
	/// <summary>
	/// Displays debug info about camera state in-game.
	/// </summary>
	public class DebugCameraDisplay {
		protected JoystickCamera parent;
		protected UIPanelWrapper panel;
		protected Dictionary<string, UICustomLabel> labels;
		protected long frameCount = 0;

		public DebugCameraDisplay(JoystickCamera parent) {
			this.parent = parent;
			UIView view = GameObject.FindObjectOfType<UIView>();
			UIPanel p = (UIPanel)view.AddUIComponent(typeof(UIPanel));
			this.panel = new UIPanelWrapper(p, "CameraDebug", 256, 8, 320, 256);
			panel.Panel.SendToBack();

			//add static labels
			panel.AddLabel("Axis", 0, 0);
			panel.AddLabel("Current", 80, 0);
			panel.AddLabel("Target", 160, 0);
			panel.AddLabel("Velocity", 240, 0);
			panel.AddLabel("Xpos", 0, 14);
			panel.AddLabel("Ypos", 0, 28);
			panel.AddLabel("Zpos", 0, 42);
			panel.AddLabel("Xang", 0, 56);
			panel.AddLabel("Yang", 0, 70);
			panel.AddLabel("Height", 0, 84);
			panel.AddLabel("Size", 0, 98);

			this.labels = new Dictionary<string, UICustomLabel> {
				{ "xpos",    panel.AddLabel("",  80,  14) },
				{ "xtpos",   panel.AddLabel("", 160,  14) },
				{ "xvel",    panel.AddLabel("", 240,  14) },
				{ "ypos",    panel.AddLabel("",  80,  28) },
				{ "ytpos",   panel.AddLabel("", 160,  28) },
				{ "yvel",    panel.AddLabel("", 240,  28) },
				{ "zpos",    panel.AddLabel("",  80,  42) },
				{ "ztpos",   panel.AddLabel("", 160,  42) },
				{ "zvel",    panel.AddLabel("", 240,  42) },
				{ "xang",    panel.AddLabel("",  80,  56) },
				{ "xtang",   panel.AddLabel("", 160,  56) },
				{ "yang",    panel.AddLabel("",  80,  70) },
				{ "ytang",   panel.AddLabel("", 160,  70) },
				{ "height",  panel.AddLabel("",  80,  84) },
				{ "theight", panel.AddLabel("", 160,  84) },
				{ "sheight", panel.AddLabel("", 240,  84) },
				{ "size",    panel.AddLabel("",  80,  98) },
				{ "tsize",   panel.AddLabel("", 160,  98) },
				{ "target",  panel.AddLabel("",   0, 112) },
				{ "frame",   panel.AddLabel("",   0, 126) },
				{ "simtime", panel.AddLabel("", 160, 126) },
				{ "moving",  panel.AddLabel("",   0, 140) },
				//{ "input",   panel.AddLabel("",   0, 140) },
			};
		}

		public void Remove() {
			//UIView view = GameObject.FindObjectOfType<UIView>();
			foreach(KeyValuePair<string, UICustomLabel> item in this.labels) {
				this.panel.Remove(item.Value);
			}
			UnityEngine.Object.Destroy(this.panel.Panel);
		}

		public void Update(float realTimeDelta, float simulationTimeDelta) {
			frameCount++;
			this.labels["frame"].text = $"Frame {frameCount}";

			GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
			if(gameObject == null) {
				this.labels["moving"].text = "Can't find camera!";
				return;
			}

			CameraController cameraController = gameObject.GetComponent<CameraController>();
			Camera camera = RenderManager.instance.CurrentCameraInfo.m_camera;
			if(cameraController == null) {
				this.labels["moving"].text = "Can't find camera controller!";
				return;
			}
			if(camera == null) {
				this.labels["moving"].text = "Camera controller has no camera!";
				return;
			}

			string target = "none";
			InstanceID targetID = cameraController.GetTarget();
			if(!targetID.IsEmpty) {
				if(targetID.Building != 0) target = $"Building {targetID.Building}";
				else if(targetID.Citizen != 0) target = $"Citizen {targetID.Citizen}";
				else if(targetID.CitizenInstance != 0) target = $"CitizenInstance {targetID.CitizenInstance}";
				else if(targetID.Disaster != 0) target = $"Disaster {targetID.Disaster}";
				else if(targetID.District != 0) target = $"District {targetID.District}";
				else if(targetID.Event != 0) target = $"Event {targetID.Event}";
				else if(targetID.Lightning != 0) target = $"Lightning {targetID.Lightning}";
				else if(targetID.NetLane != 0) target = $"NetLane {targetID.NetLane}";
				else if(targetID.NetNode != 0) target = $"NetNode {targetID.NetNode}";
				else if(targetID.NetSegment != 0) target = $"NetSegment {targetID.NetSegment}";
				else if(targetID.Park != 0) target = $"Park {targetID.Park}";
				else if(targetID.ParkedVehicle != 0) target = $"ParkedVehicle {targetID.ParkedVehicle}";
				else if(targetID.Prop != 0) target = $"Prop {targetID.Prop}";
				else if(targetID.RadioChannel != 0) target = $"RadioChannel {targetID.RadioChannel}";
				else if(targetID.RadioContent != 0) target = $"RadioContent {targetID.RadioContent}";
				else if(targetID.TransportLine != 0) target = $"TransportLine {targetID.TransportLine}";
				else if(targetID.Tree != 0) target = $"Tree {targetID.Tree}";
				else if(targetID.Vehicle != 0) target = $"Vehicle {targetID.Vehicle}";
				else target = $"Unknown {targetID.RawData}";
			}

			var values = new Dictionary<string, float> {
				{"xpos",    cameraController.m_currentPosition.x },
				{"ypos",    cameraController.m_currentPosition.y },
				{"zpos",    cameraController.m_currentPosition.z },
				{"xtpos",   cameraController.m_targetPosition.x },
				{"ytpos",   cameraController.m_targetPosition.y },
				{"ztpos",   cameraController.m_targetPosition.z },
				{"xvel",    camera.velocity.x },
				{"yvel",    camera.velocity.y },
				{"zvel",    camera.velocity.z },
				{"xang",    cameraController.m_currentAngle.x },
				{"yang",    cameraController.m_currentAngle.y },
				{"xtang",   cameraController.m_targetAngle.x },
				{"ytang",   cameraController.m_targetAngle.y },
				{"height",  cameraController.m_currentHeight },
				{"theight", cameraController.m_targetHeight },
				{"sheight", parent.CurrentHeightScale },
				{"size",    cameraController.m_currentSize },
				{"tsize",   cameraController.m_targetSize },
			};

			foreach(KeyValuePair<string, UICustomLabel> item in this.labels) {
				switch(item.Key) {
					case "target":
						item.Value.text = $"Target: {target}";
						break;
					case "moving":
						item.Value.text = parent.DidMoveWithMouse ? "moving" : "";
						break;
					case "input": {
							var inputs = parent.GetInputs();
							var x = inputs[0].lastValue.ToString("###0.00");
							var y = inputs[1].lastValue.ToString("###0.00");
							var px = inputs[0].prevValue.ToString("###0.00");
							var py = inputs[1].prevValue.ToString("###0.00");
							//JoystickCamera.Log($"In {x} {y} p {px} {py}");
							item.Value.text = $"Input {x}\t{y}\nPrev {px}\t{py}";
							break;
						}
					case "frame": break; //we did this earlier
					case "simtime":
						item.Value.text = (realTimeDelta * 1000f).ToString("###0") + "ms";
						break;
					default:
						item.Value.text = values[item.Key].ToString("###0.00");
						break;
				}
			}
		}
	}
}
