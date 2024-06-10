using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class API
{
    public static string baseUrl = "http://127.0.0.1:5000";

    public static async Task<JObject> Get(string endpoint)
    {
        string url = $"{baseUrl}/{endpoint}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                return null;
            }
            else
            {
                var jsonResponse = webRequest.downloadHandler.text;
                var responseObject = JObject.Parse(jsonResponse);
                return responseObject;
            }
        }
    }

    public static async Task<JObject> Post(string endpoint, object data)
    {
        string url = $"{baseUrl}/{endpoint}";
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                return null;
            }
            else
            {
                var jsonResponse = webRequest.downloadHandler.text;
                var responseObject = JObject.Parse(jsonResponse);
                return responseObject;
            }
        }
    }
}
