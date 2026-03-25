using System;
using DG.Tweening;
using TMPro;
using UBUI.Serialization;
using UnityEngine;

namespace UBUI.Archipelago
{
    [Serializable]
    public class SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static implicit operator Color(SerializableColor s)
        {
            return new Color(s.r, s.g, s.b, s.a);
        }

        public static implicit operator SerializableColor(Color c)
        {
            return new SerializableColor
            {
                r = c.r,
                g = c.g,
                b = c.b,
                a = c.a
            };
        }
    }

    [Serializable]
    public class APConsoleMessageData : SerializableData
    {
        // public SerializedReference<TextMeshProUGUI> textMesh;
        // public SerializedReference<Image> image;
        public float yPadding = 4f;

        [Space]
        public float lifeSpan = 5f;
    }

    public class APConsoleMessage : SerializableComponent<APConsoleMessageData>
    {
        [NonSerialized] public RectTransform rectTransform;
        [NonSerialized] public TextMeshProUGUI textMesh;
        [NonSerialized] public StoredMessage storedMessage;

        private APConsole parent;

        private float lifeTime;


        public void Show(APConsole parentConsole)
        {
            parent = parentConsole;
            lifeTime = 0f;
            enabled = true;

            Vector2 pos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x, pos.y);

            rectTransform.DOAnchorPosX(0f, 0.2f).SetEase(Ease.OutQuad);
        }


        public void SetText(string text)
        {
            textMesh.text = text;

            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = Mathf.Max(textMesh.preferredHeight + (Data.yPadding * 2f), 30f);
            rectTransform.sizeDelta = sizeDelta;
        }


        private void Update()
        {
            if(lifeTime < Data.lifeSpan)
            {
                lifeTime += Time.unscaledDeltaTime;
                if(lifeTime >= Data.lifeSpan)
                {
                    parent.HideMessage(this);
                    enabled = false;
                }
            }
        }
    }
}