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

        private Queue<string> prevCommands;
        private int selectedPrevCommand;

        private Queue<APConsoleMessage> deadMessages;
        private List<APConsoleMessage> aliveMessages = new List<APConsoleMessage>(20);
        private Queue<APConsoleMessage> recycleMessages = new Queue<APConsoleMessage>(10);
        private float deadSize = 0f;
        private float totalSize = 0f;
        private bool hovered = false;

        private float aliveSize => totalSize - deadSize;
        private float currentSize => hovered ? totalSize : aliveSize;


        private void UpdatePositions()
        {
            float currentPos = 0f;
            if(hovered)
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

            float totalSize = hovered ? currentSize + Data.viewportSize : currentSize;
            contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, totalSize);
        }


        private void ShowScroll()
        {
            Data.raycast.Value.raycastTarget = true;
            mask.showMaskGraphic = true;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(true);
            }

            hovered = true;
            UpdateSize();
            UpdatePositions();
            ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, deadSize);
        }


        private void HideScroll()
        {
            Data.raycast.Value.raycastTarget = false;
            mask.showMaskGraphic = false;

            foreach(APConsoleMessage message in deadMessages)
            {
                message.gameObject.SetActive(false);
            }

            hovered = false;
            UpdateSize();
            UpdatePositions();

            float newPos = Mathf.Max(aliveSize - Data.viewportSize, 0f);
            ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
        }


        public void ShowMessage(string text)
        {
            APConsoleMessage message;
            if(recycleMessages.Count > 0)
            {
                message = recycleMessages.Dequeue();
            }
            else
            {
                message = Instantiate(MessagePrefab, Data.content.Value.transform, false);
                message.Init();
            }

            message.transform.localPosition = new Vector2(0f, -currentSize);
            message.rectTransform.localScale = Vector3.one;

            message.SetText(text);
            message.gameObject.SetActive(true);

            aliveMessages.Add(message);
            message.Show(this);

            totalSize += message.rectTransform.sizeDelta.y;
            UpdateSize();

            if(!hovered)
            {
                float newPos = Mathf.Max(aliveSize - Data.viewportSize, 0f);
                ((RectTransform)Data.content.Value.transform).anchoredPosition = new Vector2(0f, newPos);
            }
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

            if(!hovered)
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
            if (prevCommands.Count >= maxCommandMemory)
            {
                prevCommands.Dequeue();
            }

            prevCommands.Enqueue(text);

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
                return;
            }

            SendCommand(consoleIn.text);
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
                return;
            }

            selectedPrevCommand++;
            int reverseIndex = prevCommands.Count - 1 - selectedPrevCommand;
            consoleIn.SetTextWithoutNotify(prevCommands.ToArray()[reverseIndex]);
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
            consoleIn.SetTextWithoutNotify(prevCommands.ToArray()[reverseIndex]);
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

            prevCommands = new Queue<string>(maxCommandMemory);
            deadMessages = new Queue<APConsoleMessage>(maxDeadMessages);
        }

        
        private void OnEnable()
        {
            Init();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowScroll();
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            HideScroll();
        }
    }
}