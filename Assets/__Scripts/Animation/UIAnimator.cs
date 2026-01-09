using System;
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
        public SerializedVector2 positionOffset = SerializedVector2.zero;
        public bool fade = false;
        public Ease easing = Ease.InQuad;
        public Ease reverseEasing = Ease.OutQuad;

        public float delay = 0f;
        public float reverseDelay = 0f;

        public SerializedBehaviour<CanvasGroup> canvasGroup;
    }


    [RequireComponent(typeof(RectTransform))]
    public class UIAnimator : SerializableComponent<UIAnimatorData>
    {
        private RectTransform rectTransform;
        private Vector2 startPos;
        private Vector2 endPos;


        public void PlayAnimation()
        {
            rectTransform.DOAnchorPos(endPos, Data.duration).SetEase(Data.easing).SetDelay(Data.delay);

            if(Data.fade && Data.canvasGroup.Value)
            {
                Data.canvasGroup.Value.DOFade(0f, Data.duration).SetDelay(Data.delay);
            }
        }


        public void PlayAnimationReverse()
        {
            rectTransform.DOAnchorPos(startPos, Data.duration).SetEase(Data.reverseEasing).SetDelay(Data.reverseDelay);

            if(Data.fade && Data.canvasGroup.Value)
            {
                Data.canvasGroup.Value.DOFade(1f, Data.duration).SetDelay(Data.reverseDelay);
            }
        }


        public void HandleNewState(UIState oldState, UIState newState)
        {
            if(Data.eventState == UIState.None)
            {
                return;
            }

            if(oldState == Data.eventState && newState != Data.eventState)
            {
                PlayAnimation();
            }
            else if(newState == Data.eventState && oldState != Data.eventState)
            {
                PlayAnimationReverse();
            }
        }


        public void Init()
        {
            rectTransform = (RectTransform)transform;
            Data.canvasGroup.FindValue(rectTransform);

            startPos = rectTransform.anchoredPosition;
            endPos = startPos + Data.positionOffset;
        }


        private void Awake()
        {
            Init();
        }

        
        private void OnEnable()
        {
            UIStateManager.OnStateChanged += HandleNewState;
        }


        private void OnDisable()
        {
            UIStateManager.OnStateChanged -= HandleNewState;
        }
    }
}