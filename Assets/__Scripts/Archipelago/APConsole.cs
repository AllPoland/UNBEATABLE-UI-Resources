using System;
using System.Collections.Generic;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UBUI.Animation;
using DG.Tweening;
using System.Linq;

namespace UBUI.Archipelago
{
    public struct StoredMessage : IEquatable<StoredMessage>
    {
        public string text;
        public float size;

        public StoredMessage(string text, float size)
        {
            this.text = text;
            this.size = size;
        }

        public bool Equals(StoredMessage b)
        {
            return b.size == size && b.text == text;
        }
    }

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
        public SerializedReference<Scrollbar> scrollBar;

        [Space]
        public float openSize = 600f;
        public float closedSize = 75;
        public float viewportSize = 500f;
    }

    public class APConsole : SerializableComponent<APConsoleData>, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<string> OnMessageSent;

        public APConsoleMessage MessagePrefab;
        [NonSerialized] public int maxMessageMemory = 300;
        [NonSerialized] public int maxCommandMemory = 10;

        private bool _paused = false;
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
                TMP_InputField consoleIn = Data.consoleIn.Value;
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if(selected == consoleIn.gameObject)
                {
                    ResetCommand(true);
                    EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
                }
                HandleDeselect();
            }
        }

        private List<StoredMessage> aliveMessages = new List<StoredMessage>(20);
        private Queue<StoredMessage> prevMessages;
        private Queue<string> queuedMessages = new Queue<string>();

        private List<string> prevCommands;
        private int selectedPrevCommand;

        private List<APConsoleMessage> visibleMessages = new List<APConsoleMessage>(20);
        private Queue<APConsoleMessage> recycleMessages = new Queue<APConsoleMessage>(20);
        private float deadSize = 0f;
        private float totalSize = 0f;
        private float currentPos = 0f;

        private bool showing = false;

        private bool hovered = false;
        private bool selected => hovered || EventSystem.current.currentSelectedGameObject == Data.consoleIn.Value.gameObject;

        private Tweener scrollbarMaskAnimation;


        private bool IsMessageVisible(StoredMessage message, float pos)
        {
            // Never deactivate alive messages (so their timer keeps working)
            if(aliveMessages.Contains(message))
            {
                return true;
            }

            float adjustedPos = currentPos + pos;
            if(adjustedPos <= -Data.viewportSize + 0.001f)
            {
                // The message is below the viewport
                return false;
            }

            float messageBottom = adjustedPos - message.size;
            if(messageBottom >= -0.001f)
            {
                // The message is above the viewport
                return false;
            }

            return true;
        }


        private APConsoleMessage GetMessageObject()
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

                message.rectTransform.localScale = Vector3.one;
                message.rectTransform.anchoredPosition = Vector2.zero;
                message.enabled = false;
            }
            message.gameObject.SetActive(true);

            return message;
        }


        private void ClearOutsideMessages()
        {
            for(int i = visibleMessages.Count - 1; i >= 0; i--)
            {
                APConsoleMessage message = visibleMessages[i];
                if(!IsMessageVisible(message.storedMessage, message.rectTransform.anchoredPosition.y))
                {
                    message.gameObject.SetActive(false);
                    visibleMessages.Remove(message);
                    recycleMessages.Enqueue(message);
                }
            }
        }


        private void UpdateMessages()
        {
            ClearOutsideMessages();

            float messagePos = 0f;
            foreach(StoredMessage storedMessage in prevMessages)
            {
                if(IsMessageVisible(storedMessage, messagePos))
                {
                    bool alreadyRendered = visibleMessages.Any(x =>
                        x.storedMessage.Equals(storedMessage)
                        && Mathf.Approximately(x.rectTransform.anchoredPosition.y, messagePos)
                    );
                    if(!alreadyRendered)
                    {
                        APConsoleMessage message = GetMessageObject();
                        message.rectTransform.anchoredPosition = new Vector2(0f, messagePos);
                        message.SetText(storedMessage.text);
                        message.storedMessage = storedMessage;

                        visibleMessages.Add(message);
                    }
                }
                messagePos -= storedMessage.size;
            }
        }


        private void SetContentPosition()
        {
            if(selected)
            {
                float newPos = Mathf.Max(deadSize, totalSize - Data.viewportSize);
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
                currentPos = newPos;
            }
            else
            {
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, deadSize);
                currentPos = deadSize;
            }

            UpdateMessages();
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

            Data.raycast.Value.raycastTarget = true;

            SetContentPosition();

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

            Data.raycast.Value.raycastTarget = false;

            SetContentPosition();

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
            if(prevMessages.Count >= maxMessageMemory)
            {
                StoredMessage yeetMessage = prevMessages.Dequeue();
                totalSize -= yeetMessage.size;
            }

            APConsoleMessage message = GetMessageObject();
            message.SetText(text);

            float messageSize = message.rectTransform.sizeDelta.y;
            StoredMessage storedMessage = new StoredMessage(text, messageSize);
            aliveMessages.Add(storedMessage);
            prevMessages.Enqueue(storedMessage);

            if(!selected)
            {
                float newPos = Mathf.Max(totalSize - Data.viewportSize, 0f);
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
            }

            if(IsMessageVisible(storedMessage, totalSize))
            {
                message.storedMessage = storedMessage;
                message.rectTransform.localPosition = new Vector2(0f, -totalSize);

                message.Show(this);
                visibleMessages.Add(message);
            }
            else
            {
                message.gameObject.SetActive(false);
                recycleMessages.Enqueue(message);
            }

            totalSize += messageSize;
            UpdateSize();
            UpdateInputSize(false);
        }


        public void QueueMessage(string text)
        {
            queuedMessages.Enqueue(text);
        }


        public void HideMessage(APConsoleMessage message)
        {
            aliveMessages.Remove(message.storedMessage);
            deadSize += message.storedMessage.size;
            UpdateInputSize(false);

            if(!selected)
            {
                SetContentPosition();
            }
            else UpdateMessages();
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


        private void UpdateScrollbarPosition(float position)
        {
            if(!selected)
            {
                return;
            }

            currentPos = ((RectTransform)Data.content.Value.transform).anchoredPosition.y;
            UpdateMessages();
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
            Data.scrollBar.FindValue(t);

            Data.maskAnimator.Value.Init();
            Data.viewAnimator.Value.Init();

            prevCommands = new List<string>(maxCommandMemory);
            prevMessages = new Queue<StoredMessage>(maxMessageMemory);

            Data.raycast.Value.raycastTarget = false;
            Data.scrollBar.Value.onValueChanged.AddListener(UpdateScrollbarPosition);
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
            if(Paused)
            {
                return;
            }

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
            if(Paused)
            {
                return;
            }

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