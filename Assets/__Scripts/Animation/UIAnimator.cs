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


        public void PlayAnimationReverse(float delay)
        {
            rectTransform.DOAnchorPos(startPos, Data.duration).SetEase(Data.reverseEasing).SetDelay(delay);

            if(Data.fade && canvasGroup)
            {
                canvasGroup.DOFade(1f, Data.duration).SetDelay(delay);
            }
        }


        public void HandleNewState(UIState oldState, UIState newState, float endDelay)
        {
            if(oldState == Data.eventState && newState != Data.eventState)
            {
                PlayAnimation();
            }
            else if(newState == Data.eventState && oldState != Data.eventState)
            {
                PlayAnimationReverse(endDelay);
            }
        }


        public void Init()
        {
            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();

            startPos = rectTransform.anchoredPosition;
            endPos = startPos + Data.positionOffset;
        }


        private void Awake()
        {
            Init();
        }

        
        private void Start()
        {
            UIStateManager.OnTransitionStart += HandleNewState;
        }


        private void OnDestroy()
        {
            UIStateManager.OnTransitionStart -= HandleNewState;
        }
    }
}