using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlatformManager : MonoBehaviour
{
  private string loginURL = "https://play.dingdinghouse.com/api/users/login";

  internal IEnumerator Login(string json){
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    using (UnityWebRequest webRequest = new UnityWebRequest(loginURL, "POST"))
    {
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Login API Error: " + webRequest.error);
        }
        else
        {
            Debug.Log("Login API Response: " + webRequest.downloadHandler.text);
        }
    }
  }
}
