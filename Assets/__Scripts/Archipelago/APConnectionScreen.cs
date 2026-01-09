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

            if(Data.ipInput?.Value)
            {
                info.ip = Data.ipInput.Value.text;
            }
            if(Data.slotInput?.Value)
            {
                info.slot = Data.slotInput.Value.text;
            }
            if(Data.passInput?.Value)
            {
                info.pass = Data.passInput.Value.text;
            }

            return info;
        }


        public void SetConnectionInfo(APConnectionInfo info)
        {
            if(Data.ipInput?.Value)
            {
                Data.ipInput.Value.text = info.ip;
            }
            if(Data.slotInput?.Value)
            {
                Data.slotInput.Value.text = info.slot;
            }
            if(Data.passInput?.Value)
            {
                Data.passInput.Value.text = info.pass;
            }
        }


        public void Connect()
        {
            OnConnect?.Invoke();
        }


        private void Start()
        {
            Transform t = transform;
            Data.ipInput.FindValue(t);
            Data.slotInput.FindValue(t);
            Data.passInput.FindValue(t);

            Data.connectButton.FindValue(t);
            if(Data.connectButton.Value)
            {
                Data.connectButton.Value.onClick.AddListener(Connect);
            }
        }
    }


    public struct APConnectionInfo
    {
        public string ip;
        public string slot;
        public string pass;
    }
}