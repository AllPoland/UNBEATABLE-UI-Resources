using System;
using System.Collections.Generic;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UBUI.Animation;
using DG.Tweening;

namespace UBUI.Archipelago
{
    [Serializable]
    public class APConsoleData : SerializableData
    {
        public SerializedReference<Image> inputContainer;
        public SerializedReference<TMP_InputField> consoleIn;
        public SerializedReference<Image> content;
        public SerializedReference<Image> raycast;
        public SerializedReference<UIAnimator> maskAnimator;
        public SerializedReference<UIAnimator> viewAnimator;
        public SerializedReference<RectMask2D> viewportMask;

        [Space]
        public float openSize = 600f;
        public float closedSize = 75;
        public float viewportSize = 500f;
    }

    public class APConsole : SerializableComponent<APConsoleData>, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<string> OnMessageSent;

        public APConsoleMessage MessagePrefab;
        [NonSerialized] public int maxCommandMemory = 10;
        [NonSerialized] public int maxDeadMessages = 300;

        private List<string> prevCommands;
        private int selectedPrevCommand;

        private Queue<string> queuedMessages = new Queue<string>();
        private Queue<APConsoleMessage> deadMessages;
        private List<APConsoleMessage> aliveMessages = new List<APConsoleMessage>(20);
        private Queue<APConsoleMessage> recycleMessages = new Queue<APConsoleMessage>(10);
        private float deadSize = 0f;
        private float totalSize = 0f;

        private bool showing = false;

        private bool hovered = false;
        private bool selected => hovered || EventSystem.current.currentSelectedGameObject == Data.consoleIn.Value.gameObject;

        private Tweener scrollbarMaskAnimation;


        private void UpdatePositions()
        {
            float currentPos = 0f;
            foreach(APConsoleMessage message in deadMessages)
            {
                message.rectTransform.anchoredPosition = new Vector2(message.rectTransform.anchoredPosition.x, currentPos);
                message.gameObject.SetActive(selected);
                currentPos -= message.rectTransform.sizeDelta.y;
            }

            foreach(APConsoleMessage message in aliveMessages)
            {
                message.rectTransform.anchoredPosition = new Vector2(message.rectTransform.anchoredPosition.x, currentPos);
                currentPos -= message.rectTransform.sizeDelta.y;
            }

            if(selected)
            {
                float newPos = Mathf.Max(deadSize, totalSize - Data.viewportSize);
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
            }
            else ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, deadSize);
        }


        private void UpdateSize()
        {
            RectTransform contentTransform = (RectTransform)Data.content.Value.transform;

            float newSize = totalSize + Data.viewportSize;
            contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, newSize);
        }


        private void UpdateInputSize(bool delayOnClose)
        {
            bool shouldOpen = selected || aliveMessages.Count > 0;
            float size = shouldOpen ? Data.openSize : Data.closedSize;
            Vector2 newInputSize = new Vector2(size, Data.inputContainer.Value.rectTransform.sizeDelta.y);

            float delay;
            if(shouldOpen)
            {
                delay = 0f;
            }
            else delay = delayOnClose ? 0.15f : 0f;

            Data.inputContainer.Value.rectTransform.DOKill();
            Data.inputContainer.Value.rectTransform.DOSizeDelta(newInputSize, 0.1f)
                .SetEase(Ease.OutQuad)
                .SetDelay(delay);
        }


        private void ShowScroll()
        {
            if(showing)
            {
                return;
            }
            showing = true;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(true);
            }

            Data.raycast.Value.raycastTarget = true;

            UpdatePositions();

            UpdateInputSize(true);
            Data.maskAnimator.Value.PlayAnimationReverse(0.1f);
            Data.viewAnimator.Value.PlayAnimationReverse(0.1f);

            // Animate the mask which reveals the scrollbar
            scrollbarMaskAnimation?.Kill();
            scrollbarMaskAnimation = DOVirtual.Float(-20f, 0f, 0.05f, (f) => Data.viewportMask.Value.padding = new Vector4(0f, 0f, f, 0f))
                .SetDelay(0.1f);
        }


        private void HideScroll()
        {
            if(!showing)
            {
                return;
            }
            showing = false;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(false);
            }

            Data.raycast.Value.raycastTarget = false;

            UpdatePositions();

            UpdateInputSize(true);
            Data.maskAnimator.Value.PlayAnimation();
            Data.viewAnimator.Value.PlayAnimation();

            // Animate the mask which reveals the scrollbar, to hide it
            scrollbarMaskAnimation?.Kill();
            scrollbarMaskAnimation = DOVirtual.Float(0f, -20f, 0.05f, (f) => Data.viewportMask.Value.padding = new Vector4(0f, 0f, f, 0f))
                .SetDelay(0.1f);
        }


        private void ShowMessage(string text)
        {
            APConsoleMessage message;
            if(recycleMessages.Count > 0)
            {
                message = recycleMessages.Dequeue();
            }
            else
            {
                message = Instantiate(MessagePrefab, Data.content.Value.transform, false);
                message.Data = MessagePrefab.Data;
                message.rectTransform = (RectTransform)message.transform;
                message.textMesh = message.rectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
            message.gameObject.SetActive(true);

            message.rectTransform.localPosition = new Vector2(0f, -totalSize);
            message.rectTransform.localScale = Vector3.one;

            message.SetText(text);

            aliveMessages.Add(message);
            message.Show(this);

            totalSize += message.rectTransform.sizeDelta.y;
            UpdateSize();
            UpdateInputSize(false);

            if(!selected)
            {
                float newPos = Mathf.Max(totalSize - Data.viewportSize, 0f);
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
            }
        }


        public void QueueMessage(string text)
        {
            queuedMessages.Enqueue(text);
        }


        public void HideMessage(APConsoleMessage message)
        {
            aliveMessages.Remove(message);

            bool clearDead = deadMessages.Count >= maxDeadMessages;
            if(clearDead)
            {
                APConsoleMessage recycle = deadMessages.Dequeue();
                float size = recycle.rectTransform.sizeDelta.y;
                totalSize -= size;
                deadSize -= size;

                recycle.gameObject.SetActive(false);
                recycleMessages.Enqueue(recycle);
            }

            deadMessages.Enqueue(message);
            deadSize += message.rectTransform.sizeDelta.y;
            UpdateInputSize(false);

            if(!selected || clearDead)
            {
                UpdatePositions();
            }
        }


        private void RegisterPrevCommand(string text)
        {
            // Avoid adding duplicate commands to the list
            prevCommands.RemoveAll(x => x == text);

            if (prevCommands.Count >= maxCommandMemory)
            {
                prevCommands.RemoveAt(0);
            }

            prevCommands.Add(text);
        }


        public void SendCommand(string text)
        {
            RegisterPrevCommand(text);
            ResetCommand(true);
            OnMessageSent?.Invoke(text);
        }


        public void HandleSubmit()
        {
            TMP_InputField consoleIn = Data.consoleIn.Value;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if(selected != consoleIn.gameObject)
            {
                return;
            }

            if(string.IsNullOrEmpty(consoleIn.text))
            {
                // Reselect the console because pressing enter deselects it
                consoleIn.Select();
                consoleIn.ActivateInputField();
                return;
            }

            SendCommand(consoleIn.text);
            consoleIn.Select();
            consoleIn.ActivateInputField();
        }


        public void HandleEscape()
        {
            TMP_InputField consoleIn = Data.consoleIn.Value;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if(selected == consoleIn.gameObject)
            {
                ResetCommand(true);
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }


        public void HandleUp()
        {
            TMP_InputField consoleIn = Data.consoleIn.Value;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if(selected != consoleIn.gameObject)
            {
                return;
            }

            if(selectedPrevCommand >= prevCommands.Count - 1)
            {
                consoleIn.caretPosition = consoleIn.text.Length;
                return;
            }

            selectedPrevCommand++;
            int reverseIndex = prevCommands.Count - 1 - selectedPrevCommand;
            string command = prevCommands.ToArray()[reverseIndex];
            consoleIn.SetTextWithoutNotify(command);

            consoleIn.caretPosition = command.Length;
        }


        public void HandleDown()
        {
            TMP_InputField consoleIn = Data.consoleIn.Value;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if(selected != consoleIn.gameObject)
            {
                return;
            }

            if(selectedPrevCommand < 0)
            {
                // This command was manually typed
                return;
            }

            if(selectedPrevCommand == 0)
            {
                // We're on the first previous command, so we want to reset to blank
                ResetCommand(true);
                return;
            }

            selectedPrevCommand--;
            int reverseIndex = prevCommands.Count - 1 - selectedPrevCommand;
            string command = prevCommands.ToArray()[reverseIndex];
            consoleIn.SetTextWithoutNotify(command);

            consoleIn.caretPosition = command.Length;
        }


        public void HandleTilde()
        {
            TMP_InputField consoleIn = Data.consoleIn.Value;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if(selected == consoleIn.gameObject)
            {
                return;
            }

            consoleIn.Select();
            consoleIn.ActivateInputField();
            consoleIn.caretPosition = consoleIn.text.Length;

            ShowScroll();
        }


        private void HandleDeselect()
        {
            if(selected)
            {
                ShowScroll();
            }
            else HideScroll();
        }


        public void ResetCommand(bool resetText)
        {
            selectedPrevCommand = -1;
            if (resetText)
            {
                Data.consoleIn.Value.SetTextWithoutNotify("");
            }
        }


        public override void Init()
        {
            Transform t = transform;
            Data.inputContainer.FindValue(t);
            Data.consoleIn.FindValue(t);
            Data.content.FindValue(t);
            Data.raycast.FindValue(t);
            Data.maskAnimator.FindValue(t);
            Data.viewAnimator.FindValue(t);
            Data.viewportMask.FindValue(t);

            Data.maskAnimator.Value.Init();
            Data.viewAnimator.Value.Init();

            prevCommands = new List<string>(maxCommandMemory);
            deadMessages = new Queue<APConsoleMessage>(maxDeadMessages);
        }

        
        private void OnEnable()
        {
            Init();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            ShowScroll();
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            if(!selected)
            {
                HideScroll();
            }
        }


        private void Update()
        {
            // Display messages on the main Unity thread to avoid being smited
            // Also only show one per frame to avoid giant lag spikes
            if(queuedMessages.Count > 0)
            {
                ShowMessage(queuedMessages.Dequeue());
            }

            if(showing && !Data.consoleIn.Value.isFocused)
            {
                HandleDeselect();
            }
        }


        private void LateUpdate()
        {
            // Handle console inputs
            if(Input.GetKeyDown(KeyCode.Return))
            {
                HandleSubmit();
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape();
            }

            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                HandleUp();
            }

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                HandleDown();
            }

            if(Input.GetKeyDown(KeyCode.BackQuote))
            {
                HandleTilde();
            }
        }
    }
}