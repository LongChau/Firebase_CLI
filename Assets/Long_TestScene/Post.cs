using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HugeTools.Network
{
    public class Post 
    {
        public static IEnumerator Send(string url, Dictionary<string, object> body, Action<string> callback = null)
        {
            string json = JsonUtility.ToJson(body);
            yield return Send(url, json, callback);
        }
        
        public static IEnumerator Send(string url, string bodyJsonString, Action<string> callback = null)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
            
            request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            callback?.Invoke(request.downloadHandler.text);
        }
    }
}