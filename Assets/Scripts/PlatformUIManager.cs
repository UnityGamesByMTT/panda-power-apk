using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;

public class PlatformUIManager : MonoBehaviour
{
  [SerializeField] private PlatformManager platformManager;
  [SerializeField] private Image LoginResponseBGImage;
  [SerializeField] private TMP_Text ResponseText;
  [SerializeField] private TMP_InputField UsernameInputField;
  [SerializeField] private TMP_InputField PasswordInputField;
  [SerializeField] private Button LoginButton;
  [SerializeField] private GameObject LoadingPage;

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
      yield return ShowLoginResponse("Invalid username or password");
    }

    if(platformManager.loginSuccess)
    {
      //Begin socket connection and UI transition
      LoadingPage.SetActive(true);
    }
    else{
      LoginButton.interactable = true;
    }
  }

  bool IsStringValid(string s)
  {
    return !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s);
  }

  internal IEnumerator OnLoginResponse(string response)
  {
    string message;
    try
    {
      JObject jsonData = JObject.Parse(response);
      message= jsonData["message"]?.ToString();
    }
    catch (Exception e)
    {
      message = "Unknown error while extracting message from server.";
      Debug.LogError("Login response parse error: " + e.Message);
    }
    
    yield return ShowLoginResponse(message);
    
    if(message.ToLower() == "login successful")
    {
      platformManager.loginSuccess = true;
    }
  }

  IEnumerator ShowLoginResponse(string response)
  {
    LoginResponseBGImage.raycastTarget = true;
    ResponseText.text = response;
    ResponseText.DOFade(0.7f, 0.5f);
    yield return LoginResponseBGImage.DOFade(0.7f, 0.5f).WaitForCompletion();
    yield return new WaitForSeconds(1.5f);
    ResponseText.DOFade(0f, 0.5f);
    yield return LoginResponseBGImage.DOFade(0f, 0.5f).WaitForCompletion();
    LoginResponseBGImage.raycastTarget = false;
  }
}

[Serializable]
public class LoginData
{
  public string username;
  public string password;
}
