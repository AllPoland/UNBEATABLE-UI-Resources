using System;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace UBUI.Colors
{
    [Serializable]
    public class ColoredGraphicData : SerializableData
    {
        public UIColor color;
    }


    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class ColoredGraphic : SerializableComponent<ColoredGraphicData>
    {
        private Graphic graphic;


        private void UpdateColor()
        {
            UIColorPalette palette;
            if(ColorManager.Instance)
            {
                palette = ColorManager.Instance.ColorPalette;
            }
            else palette = UIColorPalette.Default;

            graphic.color = palette.GetColor(Data.color);
        }


        private void Start()
        {
            if(!graphic)
            {
                graphic = GetComponent<Graphic>();
            }

            if(ColorManager.Instance)
            {
                ColorManager.Instance.OnColorsChanged += UpdateColor;
            }

            UpdateColor();
        }


        private void OnDestroy()
        {
            if(ColorManager.Instance)
            {
                ColorManager.Instance.OnColorsChanged -= UpdateColor;
            }
        }
    }
}