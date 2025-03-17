using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using Unity.VisualScripting;

public class PlatformUIManager : MonoBehaviour
{
  [SerializeField] private PlatformManager platformManager;
  [SerializeField] private TMP_InputField UsernameInputField;
  [SerializeField] private TMP_InputField PasswordInputField;
  [SerializeField] private Button LoginButton;

  private void Awake() {
    LoginButton.onClick.RemoveAllListeners();
    LoginButton.onClick.AddListener(() => StartCoroutine(OnLoginButtonClicked()));  
  }

  IEnumerator OnLoginButtonClicked()
  {
    LoginButton.interactable=false;
    string username = UsernameInputField.text;
    string password = PasswordInputField.text;

    if(IsStringValid(username) && IsStringValid(password)){
      Debug.Log("Username: " + username + "\nPassword: " + password);
      LoginData loginData = new LoginData();
      loginData.username = username;
      loginData.password = password;
      string json = JsonConvert.SerializeObject(loginData);
      yield return platformManager.Login(json);
    }
    else{
      Debug.LogError("Invalid username or password");
    }
    LoginButton.interactable=true;
  }

  bool IsStringValid(string s){
    if(string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)){
      return false;
    }
    else{
      return true;
    }
  }
}

[Serializable]
public class LoginData{
  public string username;
  public string password;
}
