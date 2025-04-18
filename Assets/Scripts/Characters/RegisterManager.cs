using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class RegisterManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button registerButton;
    public Button backToLoginButton;
    public GameObject errorPopup;
    public TMP_Text errorMessageText;
    public Button closeButton;
    public GameObject loadingIndicator;

    private string apiUrl = "https://shopnickgame.online/api/auth/register";

    void Start()
    {
        registerButton.onClick.AddListener(() => Register());
        backToLoginButton.onClick.AddListener(() => SceneManager.LoadScene("Login"));
        closeButton.onClick.AddListener(() => errorPopup.SetActive(false));

        errorPopup.SetActive(false);
        if (loadingIndicator) loadingIndicator.SetActive(false);
    }

    public async void Register()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Vui lòng nhập đầy đủ thông tin!");
            return;
        }

        if (loadingIndicator) loadingIndicator.SetActive(true);

        RegisterRequest registerData = new RegisterRequest
        {
            username = username,
            email = email,
            password = password
        };

        string requestBody = JsonUtility.ToJson(registerData);
        string response = await SendRegisterRequest(requestBody);

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (!string.IsNullOrEmpty(response))
        {
            RegisterResponse responseData = JsonUtility.FromJson<RegisterResponse>(response);
            if (!string.IsNullOrEmpty(responseData.token))
            {
                ShowError("Đăng ký thành công!");
                PlayerPrefs.SetString("auth_token", responseData.token);
                await Task.Delay(2000);
                SceneManager.LoadScene("Login");
            }
            else
            {
                ShowError("Lỗi đăng ký! Hãy thử lại.");
            }
        }
    }

    private async Task<string> SendRegisterRequest(string requestBody)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                ShowError("Lỗi từ máy chủ: " + request.downloadHandler.text);
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
}

[System.Serializable]
public class RegisterRequest
{
    public string username;
    public string email;
    public string password;
}

[System.Serializable]
public class RegisterResponse
{
    public string token;
}
