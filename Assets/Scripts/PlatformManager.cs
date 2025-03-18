using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlatformManager : MonoBehaviour
{
  [SerializeField] private PlatformUIManager platformUIManager;
  [SerializeField] internal bool loginSuccess = false;
  private string baseURL = "https://play.dingdinghouse.com";
  private string loginAPI = "/api/users/login";

  internal IEnumerator Login(string json){
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    using (UnityWebRequest webRequest = new UnityWebRequest(baseURL+loginAPI, "POST"))
    {
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        Debug.Log("Login Response: " + webRequest.downloadHandler.text);
        yield return platformUIManager.OnLoginResponse(webRequest.downloadHandler.text);  
    }
  }
  
}
