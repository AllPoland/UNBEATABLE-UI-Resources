using System;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace UBUI.Colors
{
    [Serializable]
    public class ColoredGraphicData
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


        private void OnEnable()
        {
            graphic = GetComponent<Graphic>();

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


        public override void SetData(object data)
        {
            if(data is not ColoredGraphicData graphicData)
            {
                Data = default(ColoredGraphicData);
                throw new InvalidCastException();
            }

            Data = graphicData;
        }
    }
}