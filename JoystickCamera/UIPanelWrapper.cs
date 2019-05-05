using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

//Much of this copied from https://github.com/tomarus/cs-terraingen/blob/master/TerrainUI.cs
namespace JoystickCamera {
	/// <summary>
	/// A custom checkbox widget. Used because it's easier than the default one.
	/// </summary>
	public class UICustomCheckbox: UISprite {
		public delegate void OnChangeDelegate(bool isChecked);
		public OnChangeDelegate OnChange;

		public UICustomCheckbox() {
			this.eventClicked += (component, eventParam) => {
				this.isChecked = !this.isChecked;
				if(OnChange != null) OnChange.Invoke(isChecked);
			};
		}

		public bool isChecked { get; set; }

		public override void Update() {
			base.Update();
			spriteName = isChecked ? "check-checked" : "check-unchecked";
		}
	}

	/// <summary>
	/// A custom slider widget. Used for the current value display.
	/// </summary>
	public class UICustomSlider: UISlider {
		public delegate void OnUpdateDelegate();
		public OnUpdateDelegate OnUpdate;
		public new virtual void Update() {
			base.Update();
			if(OnUpdate != null) OnUpdate.Invoke();
		}
	}

	/// <summary>
	/// A custom label widget.
	/// </summary>
	public class UICustomLabel: UILabel {
		public delegate void OnUpdateDelegate();
		public OnUpdateDelegate OnUpdate;
		public new virtual void Update() {
			base.Update();
			if(OnUpdate != null) OnUpdate.Invoke();
		}
	}

	public class UICustomTabStrip: UITabstrip {
		protected int _numTabs = 0;
		public UIButton AddTab(string caption, out UIPanelWrapper panel, string tooltip = null) {
			UIButton tabButton = base.AddTab(caption);
			tabButton.normalBgSprite = "SubBarButtonBase";
			tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
			tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
			tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
			tabButton.pressedBgSprite = "SubBarButtonBasePressed";
			tabButton.textPadding = new RectOffset(10, 10, 10, 10);
			tabButton.autoSize = true;
			if(tooltip != null) tabButton.tooltip = tooltip;
			UIPanel _panel = this.tabContainer.components[this._numTabs] as UIPanel;
			panel = new UIPanelWrapper(_panel, $"tabstrip_panel{_numTabs}", 0, 0,
				//500, 500);
				(int)this.tabContainer.parent.width, (int)this.tabContainer.parent.height);
			this.selectedIndex = _numTabs; //or else things won't hide correctly
			_numTabs++;
			return tabButton;
		}
	}

	/// <summary>
	/// A wrapper for UIPanel that provides some helpful methods.
	/// </summary>
	public class UIPanelWrapper {
		protected UIPanel panel;
		protected List<UIComponent> children;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:JoystickCamera.UIPanelWrapper"/> class.
		/// </summary>
		/// <param name="panel">Panel to wrap.</param>
		/// <param name="name">Panel name, for internal identification.</param>
		/// <param name="x">Relative X coord of panel in its parent.</param>
		/// <param name="y">Relative Y coord of panel in its parent.</param>
		/// <param name="width">Panel width.</param>
		/// <param name="height">Panel height.</param>
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
			//panel.backgroundSprite = "OptionsDropbox"; //debug
			//panel.autoSize = true;
		}

		/// <summary>
		/// The underlying UIPanel.
		/// </summary>
		/// <value>The panel.</value>
		public UIPanel Panel => this.panel;

		//These voilate naming convention to match the UIPanel names.
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

		/// <summary>
		/// Add a sub-panel to this panel.
		/// </summary>
		/// <returns>The panel.</returns>
		/// <param name="name">Sub-panel name, for internal identification.</param>
		/// <param name="x">X coord of sub-panel in the panel.</param>
		/// <param name="y">Y coord of sub-panel in the panel.</param>
		/// <param name="width">Sub-panel width.</param>
		/// <param name="height">Sub-panel height.</param>
		public UIPanelWrapper AddPanel(string name, int x, int y, int width, int height) {
			UIPanel subPanel = panel.AddUIComponent<UIPanel>();
			var wrapper = new UIPanelWrapper(subPanel, name, x, y, width, height);
			this.children.Add(subPanel);
			return wrapper;
		}

		public UIPanelWrapper AddScrollablePanel(string name, int x, int y, int width, int height,
		out UIScrollbar scrollbar) {
			var subPanel = panel.AddUIComponent<UIScrollablePanel>();
			subPanel.relativePosition = new Vector3(x, y, 0);
			subPanel.width = width;
			subPanel.height = height;
			subPanel.autoSize = true;
			subPanel.clipChildren = false;
			//why is this necessary? no idea, these scrollbars suck
			//I'm definitely doing something wrong but I don't fucking know
			//and I'm sick of fighting with it
			subPanel.freeScroll = true;
			var subSubPanel = subPanel.AddUIComponent<UIPanel>();
			var wrapper = new UIPanelWrapper(subSubPanel, name, x, y, width, height);
			this.children.Add(subPanel);

			var cSharpIsAButt = panel.AddUIComponent<UIScrollbar>();
			scrollbar = cSharpIsAButt;
			cSharpIsAButt.relativePosition = new Vector3(width - 15, 0, 0);
			cSharpIsAButt.width = 15;
			cSharpIsAButt.height = height;
			cSharpIsAButt.orientation = UIOrientation.Vertical;
			cSharpIsAButt.pivot = UIPivotPoint.BottomLeft;
			//scrollbar.AlignTo((UIComponent)uiPanel2, UIAlignAnchor.TopRight);
			cSharpIsAButt.minValue = 0.0f;
			//scrollbar.maxValue = 900f;
			cSharpIsAButt.value = 0.0f;
			cSharpIsAButt.incrementAmount = 10;
			//subPanel.verticalScrollbar = scrollbar;

			//really, this isn't done for us?
			//is this such a common thing that people want a scrollbar
			//that you can't see?
			UISlicedSprite uiSlicedSprite1 = cSharpIsAButt.AddUIComponent<UISlicedSprite>();
			uiSlicedSprite1.relativePosition = (Vector3)Vector2.zero;
			uiSlicedSprite1.autoSize = true;
			uiSlicedSprite1.size = uiSlicedSprite1.parent.size;
			uiSlicedSprite1.fillDirection = UIFillDirection.Vertical;
			uiSlicedSprite1.spriteName = "ScrollbarTrack";
			cSharpIsAButt.trackObject = (UIComponent)uiSlicedSprite1;

			UISlicedSprite handle = uiSlicedSprite1.AddUIComponent<UISlicedSprite>();
			//UIDragHandle is great if you want to just drag the whole box like a window...
			//var handle = uiSlicedSprite1.AddUIComponent<UIDragHandle>();
			//handle.target = subPanel;

			handle.relativePosition = (Vector3)Vector2.zero;
			handle.fillDirection = UIFillDirection.Vertical;
			//handle.autoSize = true;
			handle.width = handle.parent.width - 4f;
			//handle.height = 500; //ignored
			handle.spriteName = "ScrollbarThumb";
			cSharpIsAButt.thumbObject = (UIComponent)handle;
			scrollbar.scrollSize = 50;

			cSharpIsAButt.eventValueChanged += (component, value) => {
				//JoystickCamera.Log($"Scroll pos {subPanel.scrollPosition.x}, {subPanel.scrollPosition.y} to {value}");
				//subPanel.scrollPosition = new Vector2(0, value);
				//AWFUL HORRIBLE HACK
				//if I change the scroll position it jumps back up when I click things.
				//if I do this instead, it doesn't.
				//the only thing more awful than this UI system is having to try
				//to use it with zero documentation.
				subPanel.relativePosition = new Vector3(0, -value, 0);
			};
			subPanel.eventSizeChanged += (component, value) => {
				//we can't access `scrollbar` directly in here because it's
				//an out parameter, hence this name.
				JoystickCamera.Log("Resize subpanel");
				cSharpIsAButt.maxValue = value.y + cSharpIsAButt.scrollSize;
			};
			subSubPanel.eventSizeChanged += (component, value) => {
				JoystickCamera.Log("Resize subsubpanel");
				subPanel.FitToContents();
			};

			return wrapper;
		}

		/// <summary>
		/// Add a label.
		/// </summary>
		/// <returns>The label.</returns>
		/// <param name="text">Text.</param>
		/// <param name="x">X coord of label in the panel.</param>
		/// <param name="y">Y coord of label in the panel.</param>
		public UICustomLabel AddLabel(string text, int x, int y) {
			UICustomLabel label = panel.AddUIComponent<UICustomLabel>();
			this.children.Add(label);
			label.relativePosition = new Vector3(x, y, 0);
			label.text = text;
			return label;
		}

		/// <summary>
		/// Add a button.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="text">Button text.</param>
		/// <param name="x">X coord of button in the panel.</param>
		/// <param name="y">Y coord of button in the panel.</param>
		/// <param name="width">Button width.</param>
		/// <param name="height">Button height.</param>
		/// <param name="tooltip">Tooltip text displayed on hover.</param>
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

		/// <summary>
		/// Add a dropdown list.
		/// </summary>
		/// <returns>The dropdown.</returns>
		/// <param name="name">Name, for internal identification.</param>
		/// <param name="x">X coord of dropdown in panel.</param>
		/// <param name="y">Y coord of dropdown in panel.</param>
		/// <param name="items">Items to display.</param>
		/// <param name="tooltip">Tooltip text.</param>
		/// <remarks>Known bug: the popup list has no scrollbar.</remarks>
		public UIDropDown AddDropdown(string name, int x, int y, string[] items,
		string tooltip = "") {
			//holy shit having to do all this manually
			//like, why isn't some of this a default or a template
			//(there are templates but not any that are useful here)
			//I sure hope nobody ever has a non-default color scheme or font size.
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

		/// <summary>
		/// Add a slider.
		/// </summary>
		/// <returns>The slider.</returns>
		/// <param name="name">Name, for internal identification.</param>
		/// <param name="x">X coord of slider in panel.</param>
		/// <param name="y">Y coord of slider in panel.</param>
		/// <param name="value">Initial value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="step">Step sizes.</param>
		/// <param name="tooltip">Tooltip text.</param>
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

		/// <summary>
		/// Add a checkbox.
		/// </summary>
		/// <returns>The checkbox.</returns>
		/// <param name="name">Name, for internal identification.</param>
		/// <param name="x">X coord of checkbox in panel.</param>
		/// <param name="y">Y coord of ckechbox in panel.</param>
		/// <param name="state">Initial state (true=checked, false=not).</param>
		/// <param name="tooltip">Tooltip text.</param>
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

		public UICustomTabStrip AddTabStrip(string name, out UITabContainer container) {
			var strip = panel.AddUIComponent<UICustomTabStrip>();
			container = panel.AddUIComponent<UITabContainer>();
			strip.tabPages = container;
			return strip;
		}

		/// <summary>
		/// Remove a widget by name.
		/// </summary>
		/// <returns><c>true</c> if removed, <c>false</c> if not.</returns>
		/// <param name="name">Widget name.</param>
		/// <remarks>This currently isn't used and might not even work.</remarks>
		public bool Remove(string name) {
			UIComponent component = panel.Find(name);
			if(component == null) {
				JoystickCamera.Log($"Can't find UI component '{name}' to remove");
				return false;
			}
			Remove(component);
			return true;
		}

		/// <summary>
		/// Remove a widget.
		/// </summary>
		/// <param name="component">Widget to remove.</param>
		/// <param name="relayout">If set to <c>true</c>, all widgets below this one
		/// will be shifted upward by the height of the removed item.</param>
		/// <remarks>Also destroys the object.</remarks>
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

		/// <summary>
		/// Get the width and height of the area used.
		/// </summary>
		/// <returns>The bounds.</returns>
		/// <remarks>Assumes the upper left boundary is (0, 0).</remarks>
		public Vector2 GetBounds() {
			Vector2 bounds = new Vector2(0, 0);
			foreach(var child in children) {
				var x = child.relativePosition.x + child.width;
				var y = child.relativePosition.y + child.height;
				if(x > bounds.x) bounds.x = x;
				if(y > bounds.y) bounds.y = y;
			}
			return bounds;
		}
	}
}
