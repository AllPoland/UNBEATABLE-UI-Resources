using UnityEngine;

namespace UBUI.Colors
{
    public class UIColorPalette
    {
        public Color TextColor = new Color(1f, 0.2941177f, 0.4901961f, 1f);
        public Color BackgroundColor = new Color(0.9764706f, 0.9686275f, 0.8352941f, 1f);
        public Color AccentColor = new Color(0.8784314f, 0.8705882f, 0.7490196f, 1f);
        public Color SecondaryTextColor = new Color(0.7058824f, 0.7019608f, 0.6f, 1f);
        public Color SelectionColor = new Color(0f, 0f, 0f, 1f);


        public UIColorPalette() { }


        public UIColorPalette(Color[] colors)
        {
            if(colors.Length == 0)
            {
                return;
            }

            TextColor = colors[0];

            if(colors.Length < 2)
            {
                return;
            }

            BackgroundColor = colors[1];

            if(colors.Length < 3)
            {
                return;
            }

            AccentColor = colors[2];

            if(colors.Length < 4)
            {
                return;
            }

            SecondaryTextColor = colors[3];

            if(colors.Length < 5)
            {
                return;
            }

            SelectionColor = colors[4];
        }


        public Color ColorFromEnum(UIColor color)
        {
            switch(color)
            {
                default:
                case UIColor.Text:
                    return TextColor;
                case UIColor.Background:
                    return BackgroundColor;
                case UIColor.Accent:
                    return AccentColor;
                case UIColor.SecondaryText:
                    return SecondaryTextColor;
                case UIColor.Selection:
                    return SelectionColor;
            }
        }
    }


    public enum UIColor
    {
        Text,
        Background,
        Accent,
        SecondaryText,
        Selection
    }
}