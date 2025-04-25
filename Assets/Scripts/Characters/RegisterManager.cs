using System;
using System.Text;
using System.Text.RegularExpressions;
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
            ShowError("Please fill in all the fields!");
            return;
        }

        if (!IsValidUsername(username))
        {
            ShowError("Username must not contain special characters or accents!");
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowError("Invalid email format!");
            return;
        }

        if (!IsValidPassword(password))
        {
            ShowError("Password must be exactly 6 characters, no spaces or special characters!");
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
            try
            {
                RegisterResponse responseData = JsonUtility.FromJson<RegisterResponse>(response);
                if (!string.IsNullOrEmpty(responseData.token))
                {
                    ShowError("Registration successful!");
                    PlayerPrefs.SetString("auth_token", responseData.token);
                    await Task.Delay(2000);
                    SceneManager.LoadScene("Login");
                }
                else
                {
                    ShowError("Username or email is already taken");
                }
            }
            catch (Exception ex)
            {

                ShowError("Username or email is already taken");
            }
        }
        else
        {
            ShowError("Username or email is already taken");
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

                ShowError("Server error: " + request.downloadHandler.text);
                return null;
            }
        }
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private bool IsValidPassword(string password)
    {
        if (password.Length != 6)
            return false;

        foreach (char c in password)
        {
            if (!char.IsLetterOrDigit(c) || c > 127)
                return false;
        }

        if (password.Contains(" "))
            return false;

        return true;
    }

    private bool IsValidUsername(string username)
    {

        string pattern = @"^[a-zA-Z0-9_]+$";
        return Regex.IsMatch(username, pattern);
    }

    private void ShowError(string message)
    {

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
