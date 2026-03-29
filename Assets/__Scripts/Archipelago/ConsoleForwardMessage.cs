using UBUI.Archipelago;
using UnityEngine;

public class ConsoleForwardMessage : MonoBehaviour
{
    private APConsole console;


    public void HandleMessageSend(string message)
    {
        console.QueueMessage(message);
    }


    private void Start()
    {
        console = transform.parent.GetComponentInChildren<APConsole>();
        console.OnMessageSent += HandleMessageSend;

        console.Init();
    }
}