using System;
using System.Collections.Generic;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace UBUI.Archipelago
{
    [Serializable]
    public class APConsoleData : SerializableData
    {
        public SerializedReference<TMP_InputField> consoleIn;
        public SerializedReference<Image> content;
        public SerializedReference<Image> raycast;

        [Space]
        public float viewportSize = 500f;
    }

    public class APConsole : SerializableComponent<APConsoleData>, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<string> OnMessageSent;

        public APConsoleMessage MessagePrefab;
        [NonSerialized] public int maxCommandMemory = 10;
        [NonSerialized] public int maxDeadMessages = 300;

        private Mask mask;

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

        private float aliveSize => totalSize - deadSize;
        private float currentSize => hovered ? totalSize : aliveSize;


        private void UpdatePositions()
        {
            float currentPos = 0f;
            if(selected)
            {
                foreach(APConsoleMessage message in deadMessages)
                {
                    message.rectTransform.anchoredPosition = new Vector2(message.rectTransform.anchoredPosition.x, currentPos);
                    message.gameObject.SetActive(true);
                    currentPos -= message.rectTransform.sizeDelta.y;
                }
            }

            foreach(APConsoleMessage message in aliveMessages)
            {
                message.rectTransform.anchoredPosition = new Vector2(message.rectTransform.anchoredPosition.x, currentPos);
                currentPos -= message.rectTransform.sizeDelta.y;
            }
        }


        private void UpdateSize()
        {
            RectTransform contentTransform = (RectTransform)Data.content.Value.transform;

            float totalSize = selected ? currentSize + Data.viewportSize : currentSize;
            contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, totalSize);
        }


        private void ShowScroll()
        {
            if(showing)
            {
                return;
            }
            showing = true;

            Data.raycast.Value.raycastTarget = true;
            mask.showMaskGraphic = true;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(true);
            }

            UpdateSize();
            UpdatePositions();
            ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, deadSize);
        }


        private void HideScroll()
        {
            if(!showing)
            {
                return;
            }
            showing = false;

            Data.raycast.Value.raycastTarget = false;
            mask.showMaskGraphic = false;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(false);
            }

            UpdateSize();
            UpdatePositions();

            float newPos = Mathf.Max(aliveSize - Data.viewportSize, 0f);
            ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
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
                message.image = message.GetComponent<Image>();
                message.textMesh = message.rectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
            message.gameObject.SetActive(true);

            message.rectTransform.localPosition = new Vector2(0f, -currentSize);
            message.rectTransform.localScale = Vector3.one;

            message.SetText(text);

            aliveMessages.Add(message);
            message.Show(this);

            totalSize += message.rectTransform.sizeDelta.y;
            UpdateSize();

            if(!selected)
            {
                float newPos = Mathf.Max(aliveSize - Data.viewportSize, 0f);
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

            if(!selected)
            {
                message.gameObject.SetActive(false);
                UpdateSize();
                UpdatePositions();
            }
            else if(clearDead)
            {
                UpdateSize();
                UpdatePositions();
            }
        }


        public void SendCommand(string text)
        {
            // Avoid adding duplicate commands to the list
            prevCommands.RemoveAll(x => x == text);

            if (prevCommands.Count >= maxCommandMemory)
            {
                prevCommands.RemoveAt(0);
            }

            prevCommands.Add(text);

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

            if(selectedPrevCommand <= 0)
            {
                ResetCommand(true);
                return;
            }

            selectedPrevCommand--;
            int reverseIndex = prevCommands.Count - 1 - selectedPrevCommand;
            string command = prevCommands.ToArray()[reverseIndex];
            consoleIn.SetTextWithoutNotify(command);

            consoleIn.caretPosition = command.Length;
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
            Data.consoleIn.FindValue(t);
            Data.content.FindValue(t);
            Data.raycast.FindValue(t);

            mask = Data.raycast.Value.GetComponent<Mask>();

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
        }
    }
}