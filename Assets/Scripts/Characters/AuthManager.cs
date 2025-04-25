using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Text errorMessage;
    public GameObject errorPopup;
    public TMP_Text errorMessageText;
    public Button closeButton;
    public Button registerButton;
    public GameObject loadingIndicator;

    private string apiUrl = "https://shopnickgame.online/api/auth";

    void Start()
    {
        loginButton.onClick.AddListener(() => Login());
        registerButton.onClick.AddListener(() => GoToRegisterScene());
        closeButton.onClick.AddListener(() => errorPopup.SetActive(false));

        errorPopup.SetActive(false);
        if (loadingIndicator) loadingIndicator.SetActive(false);
    }

    public async void Login()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your account and password!");
            return;
        }

        if (loadingIndicator) loadingIndicator.SetActive(true);

        string token = await SendLoginRequest(username, password);

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
               


                await Task.Delay(500);

                LoadMapBossScene();
            }
            catch
            {
                ShowError("Error processing data from server");
            }
        }
    }

    private async Task<string> SendLoginRequest(string username, string password)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new LoginRequest { username = username, password = password }));
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            string jsonResponse = request.downloadHandler.text;
           

            if (request.result != UnityWebRequest.Result.Success)
            {
                ShowError("Connection error: " + request.error);
                return null;
            }

            try
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                if (response != null && !string.IsNullOrEmpty(response.token))
                {
                    PlayerPrefs.SetString("auth_token", response.token);

                    if (response.userData != null && !string.IsNullOrEmpty(response.userData.userID))
                    {
                        PlayerPrefs.SetString("user_id", response.userData.userID);
                    }
                    else if (!string.IsNullOrEmpty(response.user_id))
                    {
                        PlayerPrefs.SetString("user_id", response.user_id);
                    }
                    else
                    {
                        PlayerPrefs.SetString("user_id", username);
                    }

                    PlayerPrefs.Save();
                    return response.token;
                }
                else
                {
                    ShowError("Incorrect account or password");
                    return null;
                }
            }
            catch (Exception e)
            {
             
                ShowError("Error processing data from server");
                return null;
            }
        }
    }

    private void ShowError(string message)
    {
        Debug.LogError(message);
        errorMessageText.text = message;
        errorPopup.SetActive(true);
    }

    private void LoadMapBossScene()
    {

        SceneManager.LoadScene("Home");
    }

    private void GoToRegisterScene()
    {

        SceneManager.LoadScene("Register");
    }
}

[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public string user_id;
    public bool success;
    public string message;
    public UserData userData;
}

[System.Serializable]
public class UserData
{
    public string userID;
    public string username;
}