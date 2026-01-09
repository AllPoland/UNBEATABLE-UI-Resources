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
    }

    public class APConnectionScreen : SerializableComponent<APConnectionScreenData>
    {
        [NonSerialized] public UnityEvent OnConnect = new UnityEvent();


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


        public void Connect()
        {
            OnConnect?.Invoke();
        }


        private void Awake()
        {
            Transform t = transform;
            Data.ipInput.FindValue(t);
            Data.slotInput.FindValue(t);
            Data.passInput.FindValue(t);

            Data.connectButton.FindValue(t);
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
}