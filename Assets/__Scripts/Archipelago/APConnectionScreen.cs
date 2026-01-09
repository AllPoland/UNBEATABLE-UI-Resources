using System;
using TMPro;
using UBUI.Serialization;
using UnityEngine;

namespace UBUI.Archipelago
{
    [Serializable]
    public class APConnectionScreenData : SerializableData
    {
        public SerializedReference<TMP_InputField> ipInput;
        public SerializedReference<TMP_InputField> slotInput;
        public SerializedReference<TMP_InputField> passInput;
    }

    public class APConnectionScreen : SerializableComponent<APConnectionScreenData>
    {
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


        private void Start()
        {
            Transform t = transform;
            Data.ipInput.FindValue(t);
            Data.slotInput.FindValue(t);
            Data.passInput.FindValue(t);
        }
    }


    public struct APConnectionInfo
    {
        public string ip;
        public string slot;
        public string pass;
    }
}