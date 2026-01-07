using System;
using UnityEngine;

namespace UBUI.Colors
{
    public class ColorManager : MonoBehaviour
    {
        public static ColorManager Instance { get; private set; }

        private UIColorPalette _colorPalette = new UIColorPalette();
        public UIColorPalette ColorPalette
        {
            get => _colorPalette;
            private set
            {
                _colorPalette = value;
                OnColorsChanged?.Invoke();
            }
        }

        public event Action OnColorsChanged;


        public void SetColors(Color[] palette)
        {
            ColorPalette = new UIColorPalette(palette);
        }


        public void ResetColors()
        {
            ColorPalette = new UIColorPalette();
        }


        private void Awake()
        {
            if(Instance && Instance != this)
            {
                enabled = false;
                return;
            }

            Instance = this;
        }
    }
}