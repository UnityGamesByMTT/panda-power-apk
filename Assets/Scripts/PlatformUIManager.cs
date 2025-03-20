using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class PlatformUIManager : MonoBehaviour
{
  [SerializeField] private PlatformManager platformManager;
  [SerializeField] private Image BlackOverlayBgImage;
  [SerializeField] private TMP_Text NotifText;
  [SerializeField] private TMP_InputField UsernameInputField;
  [SerializeField] private TMP_InputField PasswordInputField;
  [SerializeField] private Button LoginButton;
  [SerializeField] internal GameObject LoadingPage;
  [SerializeField] internal GameObject[] Pages;
  [SerializeField] internal PlatformUIState State = PlatformUIState.Login;
  internal enum PlatformUIState
  {
    Login,
    Lobby,
    Game,
    Loading
  }

  private void Awake()
  {
    LoginButton.onClick.RemoveAllListeners();
    LoginButton.onClick.AddListener(() =>
    {
      StartCoroutine(OnLoginButtonClicked());
    });
  }

  IEnumerator OnLoginButtonClicked()
  {
    LoginButton.interactable = false;
    string username = UsernameInputField.text;
    string password = PasswordInputField.text;

    if (IsStringValid(username) && IsStringValid(password))
    {
      Debug.Log("Username: " + username + "\nPassword: " + password);
      LoginData loginData = new LoginData
      {
        username = username,
        password = password
      };
      string json = JsonConvert.SerializeObject(loginData);
      yield return platformManager.Login(json);
    }
    else
    {
      Debug.LogError("Invalid username or password");
      yield return ShowNotification("Invalid username or password");
    }

    if(string.IsNullOrEmpty(platformManager.userToken)){
      LoginButton.interactable = true;
    }
  }

  internal void ConnectUser(){
    if(!string.IsNullOrEmpty(platformManager.userToken) && !string.IsNullOrEmpty(platformManager.platformID)){
      LoadingPage.SetActive(true);
      platformManager.SetupPlatformSocketConnection();
    }
  }

  internal void setState(string state){
    switch(state){
      case "login":
        State = PlatformUIState.Login;
        LoginButton.interactable = true;
        UsernameInputField.text = "";
        PasswordInputField.text = "";
        break;
      case "lobby":
        State = PlatformUIState.Lobby;
        break;
      case "game":
        State = PlatformUIState.Game;
        break;
      case "loading":
        State = PlatformUIState.Loading;
        break;
    }
    OpenPage();
  }

  void OpenPage(){
    foreach(GameObject page in Pages){
      page.SetActive(false);
    }
    switch(State){
      case PlatformUIState.Login:
        Pages[0].SetActive(true);
        break;
      case PlatformUIState.Lobby:
        Pages[1].SetActive(true);
        break;
      case PlatformUIState.Loading:
        Pages[3].SetActive(true);
        break;
    }
  }

  bool IsStringValid(string s)
  {
    return !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s);
  }

  internal IEnumerator OnLoginResponse(string response)
  {
    string message=null;
    string token=null;
    try
    {
      JObject jsonData = JObject.Parse(response);
      message= jsonData["message"]?.ToString();
      token = jsonData["token"]?.ToString();
    }
    catch (Exception e)
    {
      message = "Unknown error.";
      token = "";
      Debug.LogError("Login response parse error: " + e.Message);
    }
    
    if(message!=null){
      yield return ShowNotification(message);
      
      if(message.ToLower() == "login successful")
      {
        platformManager.userToken = token;
        PlayerPrefs.SetString("UserToken", token);
        ConnectUser();
      }
    }
  }

  internal IEnumerator ShowNotification(string text)
  {
    BlackOverlayBgImage.raycastTarget = true;
    NotifText.text = text;
    NotifText.DOFade(0.7f, 0.5f);
    yield return BlackOverlayBgImage.DOFade(0.7f, 0.5f).WaitForCompletion();
    yield return new WaitForSeconds(1.5f);
    NotifText.DOFade(0f, 0.5f);
    yield return BlackOverlayBgImage.DOFade(0f, 0.5f).WaitForCompletion();
    BlackOverlayBgImage.raycastTarget = false;
  }
}

[Serializable]
public class LoginData
{
  public string username;
  public string password;
}
