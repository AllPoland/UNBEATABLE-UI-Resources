using System;
using TMPro;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace UBUI.Colors
{
    [Serializable]
    public class ColoredInputFieldData : SerializableData
    {
        public UIColor caretColor;
    }


    [ExecuteInEditMode]
    [RequireComponent(typeof(Graphic))]
    public class ColoredInputField : SerializableComponent<ColoredInputFieldData>
    {
        private TMP_InputField inputField;


        private void UpdateColor()
        {
            UIColorPalette palette;
            if(ColorManager.Instance)
            {
                palette = ColorManager.Instance.ColorPalette;
            }
            else palette = UIColorPalette.Default;

            inputField.caretColor = palette.GetColor(Data.caretColor);
        }


        private void Start()
        {
            if(!inputField)
            {
                inputField = GetComponent<TMP_InputField>();
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