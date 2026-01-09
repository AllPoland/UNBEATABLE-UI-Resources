using System;
using TMPro;
using UBUI.Serialization;
using UnityEngine;

namespace UBUI.Colors
{
    [Serializable]
    public class ColoredTextData : SerializableData
    {
        public UIColor color;
    }


    [ExecuteInEditMode]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ColoredText : SerializableComponent<ColoredTextData>
    {
        private TextMeshProUGUI text;


        private void UpdateColor()
        {
            UIColorPalette palette;
            if(ColorManager.Instance)
            {
                palette = ColorManager.Instance.ColorPalette;
            }
            else palette = UIColorPalette.Default;

            text.color = palette.GetColor(Data.color);
        }


        private void Start()
        {
            if(!text)
            {
                text = GetComponent<TextMeshProUGUI>();
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