using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ChatMessage
{
    [SerializeField]
    private string senderID;
    [SerializeField]
    private string message;
    [SerializeField]
    private string timeStamp;

    public string SenderID { get => senderID; set => senderID = value; }
    public string Message { get => message; set => message = value; }
    public string TimeStamp { get => timeStamp; set => timeStamp = value; }

    public ChatMessage(string senderID, string message, string timeStamp)
    {
        this.senderID = senderID;
        this.message = message;
        this.timeStamp = timeStamp;
    }

    /// <summary>
    /// Convert this data to dictionary.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result["senderID"] = senderID;
        result["message"] = message;
        result["timeStamp"] = timeStamp;

        return result;
    }

}
