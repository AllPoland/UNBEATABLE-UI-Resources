using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UBUI.Colors
{
    [Serializable]
    [ExecuteInEditMode]
    public class ColoredGraphic : MonoBehaviour
    {
        public string componentName = "ColoredGraphic";

        public List<Graphic> targetGraphics;
        public UIColor color;


        private void SetGraphicColors(Color newColor)
        {
            foreach(Graphic graphic in targetGraphics)
            {
                graphic.color = newColor;
            }
        }


        private void UpdateColor()
        {
            UIColorPalette palette;
            if(ColorManager.Instance)
            {
                palette = ColorManager.Instance.ColorPalette;
            }
            else palette = UIColorPalette.Default;

            Color newColor = palette.GetColor(color);
            SetGraphicColors(newColor);
        }


        private void OnEnable()
        {
            if(targetGraphics == null)
            {
                targetGraphics = new List<Graphic>();
            }

            if(targetGraphics.Count == 0)
            {
                // Allows for shorthand addition of this component to a graphic
                Graphic target = GetComponent<Graphic>();
                if(!target)
                {
                    // No target graphic
                    return;
                }

                targetGraphics.Add(target);
            }

            if(ColorManager.Instance)
            {
                ColorManager.Instance.OnColorsChanged += UpdateColor;
            }

            UpdateColor();
        }


        private void OnDisable()
        {
            if(ColorManager.Instance)
            {
                ColorManager.Instance.OnColorsChanged -= UpdateColor;
            }
        }
    }
}