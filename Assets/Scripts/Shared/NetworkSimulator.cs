using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSimulator : MonoBehaviour
{
    public static NetworkSimulator Instance;

    [Range(0f, 5f)] public float SimulatedLatency = 0.02f;

    public event Action<BaseMessage> OnClientReceivedMessage;
    public event Action<BaseMessage> OnServerReceivedMessage;

    private void Awake()
    {
        Instance = this;
    }

    // client send msg
    public void SendToServer(BaseMessage msg)
    {
        StartCoroutine(DeliverMessage(msg, OnServerReceivedMessage, SimulatedLatency));
    }

    //sever send msg to client
    public void SendToClient(BaseMessage msg)
    {
        StartCoroutine(DeliverMessage(msg, OnClientReceivedMessage, SimulatedLatency));
    }

    private IEnumerator DeliverMessage(BaseMessage msg, Action<BaseMessage> destination, float delay)
    {
        yield return new WaitForSeconds(delay);
        destination?.Invoke(msg);
    }

}
