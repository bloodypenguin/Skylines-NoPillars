using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ColossalFramework.UI;
using UnityEngine;

namespace NoPillars
{
    public static class NoPillarsUI
    {
        private static UIDropDown _pillarsDropDown;
        private static UIDropDown _collideDropDown;
        private static UIPanel _panel;

        public static int Pillars { get; private set; } = 0;

        public static int Collide { get; private set; } = 0;

        public static void Initialize()
        {
            Dispose();


            var uiView = UIView.GetAView();
            _panel = uiView.AddUIComponent(typeof(UIPanel)) as UIPanel;
            _panel.name = "NoPillarsPanel";
            _panel.backgroundSprite = "MenuPanel2";
            var hidePillarsButton = NoPillars.Pillars.networkSkinsEnabled;
            _panel.size = new Vector2(100 + 200 + 12, hidePillarsButton ? 72 : 100);
            _panel.isVisible = false;
            _panel.relativePosition = new Vector3(0, hidePillarsButton ? 874 : 846);
            UIUtil.SetupTitle("No Pillars Mod", _panel);

            if (!hidePillarsButton)
            {
                var pillarsLabel = _panel.AddUIComponent<UILabel>();
                pillarsLabel.text = "Pillars";
                pillarsLabel.width = 100;
                pillarsLabel.height = 24;
                pillarsLabel.relativePosition = new Vector2(4, 44);
                pillarsLabel.textScale = 0.8f;

                _pillarsDropDown = UIUtil.CreateDropDown(_panel);
                _pillarsDropDown.width = 200;
                _pillarsDropDown.height = 24;
                _pillarsDropDown.listWidth = 200;
                _pillarsDropDown.tooltip = "Change pillars mode";
                _pillarsDropDown.relativePosition = new Vector2(8 + 100, 44);
                _pillarsDropDown.eventSelectedIndexChanged += (comp, i) => { Pillars = i; };
                _pillarsDropDown.items = new[] { "Default", "No Pillars" };
                _pillarsDropDown.listPosition = UIDropDown.PopupListPosition.Above;

                foreach (var item in NoPillars.Pillars.pillars.Select(buildingInfo => Regex.Replace(buildingInfo.name.Split('.').Last(), "([a-z])_?([A-Z])", "$1 $2")))
                {
                    _pillarsDropDown.AddItem(item.Length > 25 ? item.Substring(0, 25) + "…" : item);
                }
            }

            var collideLabel = _panel.AddUIComponent<UILabel>();
            collideLabel.text = "Zoning/Collision";
            collideLabel.width = 100;
            collideLabel.height = 24;
            collideLabel.textScale = 0.8f;
            collideLabel.relativePosition = new Vector2(4, hidePillarsButton ? 44 : 72);

            _collideDropDown = UIUtil.CreateDropDown(_panel);
            _collideDropDown.width = 200;
            _collideDropDown.height = 24;
            _collideDropDown.listWidth = 200;
            _collideDropDown.tooltip = "Change zoning/collision mode";
            _collideDropDown.relativePosition = new Vector2(8 + 100, hidePillarsButton ? 44 : 72);
            _collideDropDown.eventSelectedIndexChanged += (comp, i) => { Collide = i; };
            _collideDropDown.items = new[] { "Default", "No Collision (+No Zoning)", "No Zoning", "Force Zoning" };
            _collideDropDown.listPosition = UIDropDown.PopupListPosition.Above;

            Reset();
        }

        public static void Reset()
        {
            Hide();
            if (_collideDropDown != null)
            {
                _collideDropDown.selectedIndex = 0;
            }
            if (_pillarsDropDown != null)
            {
                _pillarsDropDown.selectedIndex = 0;
            }
            Pillars = 0;
            Collide = 0;
        }

        public static void Dispose()
        {
            Reset();
            if (_panel != null)
            {
                Object.Destroy(_panel.gameObject);
            }
            _pillarsDropDown = null;
            _collideDropDown = null;
            _panel = null;
        }

        public static void Hide()
        {
            _panel?.Hide();
        }

        public static void Show()
        {
            _panel?.Show();
        }
    }
}