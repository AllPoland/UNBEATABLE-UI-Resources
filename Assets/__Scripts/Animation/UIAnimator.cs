using System;
using System.Collections.Generic;
using DG.Tweening;
using UBUI.Serialization;
using UnityEngine;

namespace UBUI.Animation
{
    [Serializable]
    public class UIAnimatorData : SerializableData
    {
        public float duration = 0.2f;
        public UIState eventState = UIState.None;
        public string delayMatch = "";
        public float delayMod = 0f;

        public SerializedVector2 positionOffset = SerializedVector2.zero;
        public bool fade = false;
        public Ease easing = Ease.InQuad;
        public Ease reverseEasing = Ease.OutQuad;
    }


    [RequireComponent(typeof(RectTransform))]
    public class UIAnimator : SerializableComponent<UIAnimatorData>
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private Vector2 startPos;
        private Vector2 endPos;


        public void PlayAnimation()
        {
            rectTransform.DOAnchorPos(endPos, Data.duration).SetEase(Data.easing);

            if(Data.fade && canvasGroup)
            {
                canvasGroup.DOFade(0f, Data.duration);
            }
        }


        public void PlayAnimationReverse(Dictionary<string, float> delays)
        {
            if(string.IsNullOrEmpty(Data.delayMatch) || !delays.TryGetValue(Data.delayMatch, out float delay))
            {
                delay = 0f;
            }
            delay += Data.delayMod;

            rectTransform.DOAnchorPos(startPos, Data.duration).SetEase(Data.reverseEasing).SetDelay(delay);

            if(Data.fade && canvasGroup)
            {
                canvasGroup.DOFade(1f, Data.duration).SetDelay(delay);
            }
        }


        public void HandleNewState(UIState oldState, UIState newState, Dictionary<string, float> delays)
        {
            if(oldState == Data.eventState && newState != Data.eventState)
            {
                PlayAnimation();
            }
            else if(newState == Data.eventState && oldState != Data.eventState)
            {
                PlayAnimationReverse(delays);
            }
        }


        public void Init()
        {
            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();

            startPos = rectTransform.anchoredPosition;
            endPos = startPos + Data.positionOffset;

            rectTransform.anchoredPosition = endPos;
            if(Data.fade && canvasGroup)
            {
                canvasGroup.alpha = 0f;
            }

            UIStateManager.OnTransitionStart += HandleNewState;
        }


        private void OnDestroy()
        {
            UIStateManager.OnTransitionStart -= HandleNewState;
        }
    }
}