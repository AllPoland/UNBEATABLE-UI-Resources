using System;
using DG.Tweening;
using TMPro;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace UBUI.Archipelago
{
    [Serializable]
    public class APConsoleMessageData : SerializableData
    {
        public SerializedReference<TextMeshProUGUI> textMesh;
        public SerializedReference<Image> image;
        public float yPadding = 4f;
        public Color aliveColor = Color.black;
        public Color deadColor = Color.grey;

        [Space]
        public float lifeSpan = 5f;
    }

    public class APConsoleMessage : SerializableComponent<APConsoleMessageData>
    {
        [NonSerialized] public RectTransform rectTransform;

        private APConsole parent;

        private float lifeTime;


        public void Show(APConsole parentConsole)
        {
            parent = parentConsole;
            lifeTime = 0f;
            Data.image.Value.color = Data.aliveColor;
            enabled = true;

            Vector2 pos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x, pos.y);

            rectTransform.DOAnchorPosX(0f, 0.25f).SetEase(Ease.OutQuad);
        }


        public void SetText(string text)
        {
            TextMeshProUGUI textMesh = Data.textMesh.Value;
            textMesh.text = text;

            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = Mathf.Max(textMesh.preferredHeight + (Data.yPadding * 2f), 30f);
            rectTransform.sizeDelta = sizeDelta;
        }


        public override void Init()
        {
            Transform t = transform;
            Data.textMesh.FindValue(t);
            Data.image.FindValue(t);

            rectTransform = (RectTransform)t;
        }


        private void Update()
        {
            if(lifeTime < Data.lifeSpan)
            {
                lifeTime += Time.unscaledDeltaTime;
                if(lifeTime >= Data.lifeSpan)
                {
                    Data.image.Value.color = Data.deadColor;
                    parent.HideMessage(this);
                    enabled = false;
                }
            }
        }
    }
}