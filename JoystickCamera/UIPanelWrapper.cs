using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

//Much of this copied from https://github.com/tomarus/cs-terraingen/blob/master/TerrainUI.cs
namespace JoystickCamera {
	public class UICustomCheckbox: UISprite {
		public UICustomCheckbox() {
			this.eventClicked += (component, eventParam) => {
				this.isChecked = !this.isChecked;
			};
		}

		public bool isChecked { get; set; }

		public override void Update() {
			base.Update();
			spriteName = isChecked ? "check-checked" : "check-unchecked";
		}
	}

	public class UICustomSlider: UISlider {
		public delegate void OnUpdateDelegate();
		public OnUpdateDelegate OnUpdate;
		public new virtual void Update() {
			base.Update();
			if(OnUpdate != null) OnUpdate.Invoke();
		}
	}

	public class UIPanelWrapper {
		protected UIPanel panel;
		protected List<UIComponent> children;

		public UIPanelWrapper(UIPanel panel, string name, int x, int y, int width, int height) {
			this.panel = panel;
			this.children = new List<UIComponent>();
			panel.name = name;
			panel.autoLayoutDirection = LayoutDirection.Horizontal;
			this.relativePosition = new Vector3(x, y, 0);
			this.width = width;
			this.height = height;
			panel.isEnabled = true;
			panel.isVisible = true;
			//panel.backgroundSprite = "OptionsDropbox";
			//panel.autoSize = true;
		}

		public UIPanel Panel => this.panel;
		public Vector3 relativePosition {
			get => this.panel.relativePosition;
			set => this.panel.relativePosition = value;
		}
		public float width {
			get => this.panel.width;
			set => this.panel.width = value;
		}
		public float height {
			get => this.panel.height;
			set => this.panel.height = value;
		}

		public UIPanelWrapper AddPanel(string name, int x, int y, int width, int height) {
			UIPanel subPanel = panel.AddUIComponent<UIPanel>();
			var wrapper = new UIPanelWrapper(subPanel, name, x, y, width, height);
			this.children.Add(subPanel);
			return wrapper;
		}

		public UILabel AddLabel(string text, int x, int y) {
			UILabel label = panel.AddUIComponent<UILabel>();
			this.children.Add(label);
			label.relativePosition = new Vector3(x, y, 0);
			label.text = text;
			return label;
		}

		public UIButton AddButton(string text, int x, int y, int width,
		int height = 20, string tooltip = "") {
			UIButton button = panel.AddUIComponent<UIButton>();
			this.children.Add(button);
			button.name = text;
			button.text = text;
			button.tooltip = tooltip;
			button.normalBgSprite = "ButtonMenu";
			button.hoveredBgSprite = "ButtonMenuHovered";
			button.disabledBgSprite = "ButtonMenuDisabled";
			button.focusedBgSprite = "ButtonMenuFocused";
			button.pressedBgSprite = "ButtonMenuPressed";
			button.size = new Vector2(width, height);
			button.relativePosition = new Vector3(x, y, 0);
			button.hoveredTextColor = new Color32(255, 255, 120, 255);
			return button;
		}

		public UIDropDown AddDropdown(string name, int x, int y, string[] items,
		string tooltip = "") {
			//holy shit having to do all this manually
			UIDropDown dropdown = panel.AddUIComponent<UIDropDown>();
			//panel.AttachUIComponent(dropdown.gameObject);
			this.children.Add(dropdown);
			dropdown.items = items;
			dropdown.selectedIndex = 0;
			dropdown.isEnabled = true;
			dropdown.isVisible = true;
			dropdown.isInteractive = true;
			dropdown.name = name;
			dropdown.relativePosition = new Vector3(x, y, 0);
			dropdown.size = new Vector2(200, 25);
			dropdown.width = 200;
			dropdown.height = 25;
			dropdown.listWidth = 200;
			dropdown.itemHeight = 25;
			dropdown.itemPadding = new RectOffset(4, 20, 4, 4);
			dropdown.autoListWidth = true;
			dropdown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
			dropdown.horizontalAlignment = UIHorizontalAlignment.Center;
			dropdown.verticalAlignment = UIVerticalAlignment.Middle;
			dropdown.focusedBgSprite = "OptionsDropboxFocused";
			dropdown.hoveredBgSprite = "OptionsDropboxHovered";
			dropdown.listBackground = "OptionsDropboxListbox";
			dropdown.itemHover = "ListItemHover";
			dropdown.itemHighlight = "ListItemHighlight";
			dropdown.normalBgSprite = "OptionsDropbox";
			dropdown.textFieldPadding = new RectOffset(4, 20, 4, 4);
			dropdown.popupColor = new Color32(255, 255, 255, 255);
			dropdown.popupTextColor = new Color32(170, 170, 170, 255);
			dropdown.useGUILayout = true;
			dropdown.tooltip = tooltip;
			//dropdown.useGradient = true;

			//this outlines the text, not the box...
			//dropdown.useOutline = true;
			//dropdown.outlineColor = new Color32(128, 128, 128, 255);
			//dropdown.outlineSize = 1;
			dropdown.triggerButton = dropdown; //clicking itself opens it
			return dropdown;
		}

		public UICustomSlider AddSlider(string name, int x, int y, float value,
		float min, float max, float step, string tooltip = "") {
			UICustomSlider slider = panel.AddUIComponent<UICustomSlider>();
			this.children.Add(slider);
			slider.name = name;
			slider.minValue = min;
			slider.maxValue = max;
			slider.stepSize = step;
			slider.value = value;
			slider.relativePosition = new Vector3(x, y, 0);
			slider.size = new Vector2(170, 16);
			slider.tooltip = tooltip;

			UISprite thumbSprite = slider.AddUIComponent<UISprite>();
			this.children.Add(thumbSprite);
			thumbSprite.name = name + "_thumb";
			thumbSprite.spriteName = "OptionsScrollbarThumb";
			thumbSprite.Show();
			thumbSprite.size = new Vector2(16, 16);
			thumbSprite.pivot = UIPivotPoint.MiddleCenter;

			slider.backgroundSprite = "OptionsScrollbarTrack";
			slider.thumbObject = thumbSprite;
			slider.orientation = UIOrientation.Horizontal;
			slider.isVisible = true;
			slider.enabled = true;
			slider.canFocus = true;
			slider.isInteractive = true;

			UILabel valueLabel = panel.AddUIComponent<UILabel>();
			this.children.Add(valueLabel);
			valueLabel.name = name + "_ValueLabel";
			valueLabel.text = slider.value.ToString("0");
			valueLabel.relativePosition = new Vector3(x + 175, y);
			valueLabel.textScale = 0.8f;

			slider.eventValueChanged += (component, f) => {
				valueLabel.text = slider.value.ToString("0");
			};

			return slider;
		}

		public UICustomCheckbox AddCheckbox(string name, int x, int y, bool state = false, string tooltip = "") {
			UICustomCheckbox box = panel.AddUIComponent<UICustomCheckbox>();
			this.children.Add(box);
			box.relativePosition = new Vector3(x, y, 0);
			box.size = new Vector2(16, 16);
			box.Show();
			box.enabled = true;
			box.isChecked = state;
			box.tooltip = tooltip;
			box.color = new Color32(185, 221, 254, 255);
			return box;
		}

		public bool Remove(string name) {
			UIComponent component = panel.Find(name);
			if(component == null) {
				JoystickCamera.Log($"Can't find UI component '{name}' to remove");
				return false;
			}
			Remove(component);
			return true;
		}

		public void Remove(UIComponent component, bool relayout = true) {
			this.children.Remove(component);

			if(relayout) {
				//Move children below this one up
				//Note the origin is actually lower left corner, so
				//we're really moving children above this one down...
				foreach(UIComponent child in this.children) {
					if(child.position.y < component.position.y) {
						child.position = new Vector3(
							child.position.x,
							child.position.y + component.height,
							child.position.z);
					}
				}
				panel.height -= component.height;
			}
			panel.RemoveUIComponent(component);
			UnityEngine.Object.Destroy(component);
		}
	}
}
