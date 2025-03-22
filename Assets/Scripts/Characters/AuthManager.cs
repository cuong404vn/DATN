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
            ShowError("Vui lòng nhập tài khoản và mật khẩu!");
            return;
        }

        if (loadingIndicator) loadingIndicator.SetActive(true); 

        LoginRequest loginData = new LoginRequest { username = username, password = password };
        string requestBody = JsonUtility.ToJson(loginData);

        string response = await SendLoginRequest(requestBody);

        if (loadingIndicator) loadingIndicator.SetActive(false); 

        if (!string.IsNullOrEmpty(response))
        {
            LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(response);
            if (!string.IsNullOrEmpty(responseData.token))
            {
                Debug.Log("Đăng nhập thành công!" + responseData.token);
                PlayerPrefs.SetString("auth_token", responseData.token);
                LoadMapBossScene();
            }
            else
            {
                ShowError("Sai Tên Đăng Nhập Hoặc Mật Khẩu");
            }
        }
    }

    private async Task<string> SendLoginRequest(string requestBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                string errorResponse = request.downloadHandler.text;
                Debug.LogError("❌ Lỗi đăng nhập: " + errorResponse);
                ShowError("Lỗi từ máy chủ: " + errorResponse);
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
        Debug.Log("🔄 Chuyển sang màn hình MAP_BOSS...");
        SceneManager.LoadScene("MAP BOSS");
    }


    private void GoToRegisterScene()
    {
        Debug.Log("Đang chuyển sang màn hình đăng ký...");
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
}

