using System;
using TMPro;
using UBUI.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UBUI.Archipelago
{
    [Serializable]
    public class APConnectionScreenData : SerializableData
    {
        public SerializedReference<TMP_InputField> ipInput;
        public SerializedReference<TMP_InputField> portInput;
        public SerializedReference<TMP_InputField> slotInput;
        public SerializedReference<TMP_InputField> passInput;
        public SerializedReference<Button> connectButton;
        public SerializedReference<TextMeshProUGUI> errorText;
    }

    public class APConnectionScreen : SerializableComponent<APConnectionScreenData>
    {
        private const string connectingText = "<mspace=11>//<mspace=17> </mspace><cspace=0.35em>...";

        [NonSerialized] public UnityEvent OnConnect = new UnityEvent();

        private TextMeshProUGUI buttonText;
        private string originalButtonText;


        public APConnectionInfo GetConnectionInfo()
        {
            APConnectionInfo info = new APConnectionInfo();

            info.ip = Data.ipInput.Value.text;
            info.port = Data.portInput.Value.text;
            info.slot = Data.slotInput.Value.text;
            info.pass = Data.passInput.Value.text;

            return info;
        }


        public void SetConnectionInfo(APConnectionInfo info)
        {
            Data.ipInput.Value.text = info.ip;
            Data.portInput.Value.text = info.port;
            Data.slotInput.Value.text = info.slot;
            Data.passInput.Value.text = info.pass;
        }


        public void CancelAndShowError(FailConnectionReason reason)
        {
            string errorMessage;
            switch(reason)
            {
                default:
                    errorMessage = "an error occurred. check logs.";
                    break;
                case FailConnectionReason.Timeout:
                    errorMessage = "connection timed out.";
                    break;
                case FailConnectionReason.BadSlot:
                    errorMessage = "invalid player slot.";
                    break;
                case FailConnectionReason.BadGame:
                    errorMessage = "wrong game.";
                    break;
                case FailConnectionReason.SlotTaken:
                    errorMessage = "the slot is already taken.";
                    break;
                case FailConnectionReason.BadVersion:
                    errorMessage = "incompatible client or apworld version.";
                    break;
                case FailConnectionReason.WrongPassword:
                    errorMessage = "incorrect password.";
                    break;
                case FailConnectionReason.BadItemHandle:
                    errorMessage = "incorrect item flags.";
                    break;
            }

            Data.errorText.Value.text = $"<cspace=0.15em>[{errorMessage}]</cspace>";

            buttonText.text = originalButtonText;
            Data.connectButton.Value.interactable = true;
        }


        public void Connect()
        {
            OnConnect?.Invoke();
            Data.connectButton.Value.interactable = false;
            buttonText.text = buttonText.text = connectingText;
        }


        public void Init()
        {
            Transform t = transform;
            Data.ipInput.FindValue(t);
            Data.portInput.FindValue(t);
            Data.slotInput.FindValue(t);
            Data.passInput.FindValue(t);

            Data.connectButton.FindValue(t);

            Data.errorText.FindValue(t);

            buttonText = Data.connectButton.Value.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            originalButtonText = buttonText.text;

            Data.connectButton.Value.onClick.AddListener(Connect);
        }
    }


    public struct APConnectionInfo
    {
        public string ip;
        public string port;
        public string slot;
        public string pass;
    }


    public enum FailConnectionReason
    {
        General,
        Timeout,
        BadSlot,
        BadGame,
        SlotTaken,
        BadVersion,
        WrongPassword,
        BadItemHandle
    }
}