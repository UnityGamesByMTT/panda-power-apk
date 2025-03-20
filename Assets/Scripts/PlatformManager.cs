using Best.SocketIO;
using Best.SocketIO.Events;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlatformManager : MonoBehaviour
{
  [SerializeField] private PlatformUIManager platformUIManager;
  [SerializeField] internal bool loginSuccess = false;
  [SerializeField] internal string userToken;
  [SerializeField] internal string platformID;
  private string baseURL = "https://92w9t0d4-5001.inc1.devtunnels.ms";
  private string loginAPI = "/api/users/login";
  private SocketManager manager;
  private void Awake() {
    if(PlayerPrefs.HasKey("PlatformID")){
      platformID = PlayerPrefs.GetString("PlatformID");
    }
    else{
      platformID = Guid.NewGuid().ToString();
      PlayerPrefs.SetString("PlatformID", platformID); 
    }

    if(PlayerPrefs.HasKey("UserToken")){
      userToken = PlayerPrefs.GetString("UserToken");
    }
    platformUIManager.ConnectUser();
  }

  internal IEnumerator Login(string json){
    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
    using (UnityWebRequest webRequest = new UnityWebRequest(baseURL+loginAPI, "POST"))
    {
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if(!string.IsNullOrEmpty(webRequest.downloadHandler.text)){
          string response = webRequest.downloadHandler.text;
          Debug.Log("Login Response: " + response);
          yield return platformUIManager.OnLoginResponse(response);
        }
    }
  }

  internal void  SetupPlatformSocketConnection(){
    SocketOptions opt=new()
    {
      Reconnection=true
    };

    Func<SocketManager, Socket, object> authFunc = (manager, socket) => {
      return new{
        token = userToken,
        origin = platformID,
        playgroundId = platformID
      };
    };
    opt.Auth = authFunc;
    Debug.Log("Auth func config with token: " + userToken + " and platformID: " + platformID);
    manager = new SocketManager(new Uri(baseURL), opt);
    Socket gameSocket = manager.GetSocket("/" + "playground");
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnPlatformSocketConnected);
    gameSocket.On<string>(SocketIOEventTypes.Disconnect, OnPlatformSocketDisconnected);
    gameSocket.On<string>(SocketIOEventTypes.Error, OnPlatformSocketError);
    gameSocket.On<Dictionary<string, object>>("data", OnListenEvent);
    gameSocket.On<string>("alert", OnAlerEvent);
  }

  void OnPlatformSocketError(string s){
    Debug.Log("OnPlatformSocketError: "+ s);
  }

  void OnAlerEvent(string data){
    Debug.Log("Alert received: " + data);
  }

  void OnListenEvent(Dictionary<string, object> data){
    object credits = data["data"].ConvertTo<Dictionary<string, object>>()["credits"];
    Debug.Log(credits.ToString());
  }

  void OnPlatformSocketConnected(ConnectResponse response){
    Debug.Log("Platform Socket Connected with response: " + response.sid);
    platformUIManager.setState("lobby");
  }

  void OnPlatformSocketDisconnected(string response){
    Debug.Log("Platform Socket Disconnected with response: " + response);
    platformUIManager.setState("login");
  }

  void Logout(){
    manager?.Socket.Disconnect();
    // manager.Close();
    userToken="";
    PlayerPrefs.DeleteKey("UserToken");
    platformUIManager.setState("login");
  }

  private void Update() {
    if(Input.GetKeyDown(KeyCode.Space)){
      Logout();
    }          //This code is to logout the user
  }

}

  // string DecodeJwtPayload(string token){
  //   try{
  //     string[] tokenParts = token.Split('.');
  //     if (tokenParts.Length != 3)
  //     {
  //         Debug.LogError("Invalid JWT format");
  //         return null;
  //     }

  //     string payload = tokenParts[1];
  //     payload=payload.Replace('-', '+').Replace('_', '/');
  //     while (payload.Length % 4 != 0) payload += "="; // Fix padding

  //     byte[] decodedBytes = Convert.FromBase64String(payload);
  //     return Encoding.UTF8.GetString(decodedBytes);
  //   }catch(Exception e){
  //     Debug.LogError("Error decoding JWT: " + e.Message);
  //     return null;
  //   }
  // }
