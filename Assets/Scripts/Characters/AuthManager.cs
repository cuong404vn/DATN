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

        string token = await SendLoginRequest(username, password);

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                Debug.Log("Đăng nhập thành công với token: " + token);

 
                await Task.Delay(500);

                LoadMapBossScene();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Lỗi khi xử lý phản hồi JSON: " + e.Message);
                Debug.LogError("Phản hồi gốc: " + token);
                ShowError("Lỗi khi xử lý dữ liệu từ máy chủ");
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

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Phản hồi đầy đủ từ API: " + jsonResponse);

               
                try
                {
                   
                    string userDataKey = "\"userData\":";
                    int userDataStart = jsonResponse.IndexOf(userDataKey);

                    if (userDataStart != -1)
                    {
                      
                        string userIDKey = "\"userID\":\"";
                        int userIDStart = jsonResponse.IndexOf(userIDKey, userDataStart) + userIDKey.Length;
                        int userIDEnd = jsonResponse.IndexOf("\"", userIDStart);

                        if (userIDStart > userIDKey.Length && userIDEnd > userIDStart)
                        {
                            string userID = jsonResponse.Substring(userIDStart, userIDEnd - userIDStart);
                            Debug.Log("✅ Đã tìm thấy userID trong userData: " + userID);

                    
                            PlayerPrefs.SetString("user_id", userID);
                            Debug.Log("✅ Đã lưu user_id: " + userID);

                           
                            string tokenKey = "\"token\":\"";
                            int tokenStart = jsonResponse.IndexOf(tokenKey) + tokenKey.Length;
                            int tokenEnd = jsonResponse.IndexOf("\"", tokenStart);
                            string token = jsonResponse.Substring(tokenStart, tokenEnd - tokenStart);

                          
                            PlayerPrefs.SetString("auth_token", token);
                            Debug.Log("✅ Đã lưu auth_token: " + token);

                            return token;
                        }
                        else
                        {
                            Debug.LogWarning("Không tìm thấy userID trong userData, sử dụng username thay thế");
                            PlayerPrefs.SetString("user_id", username);
                            Debug.Log("✅ Lưu user_id vào PlayerPrefs: " + username);

                            
                            string tokenKey = "\"token\":\"";
                            int tokenStart = jsonResponse.IndexOf(tokenKey) + tokenKey.Length;
                            int tokenEnd = jsonResponse.IndexOf("\"", tokenStart);
                            string token = jsonResponse.Substring(tokenStart, tokenEnd - tokenStart);

                            
                            PlayerPrefs.SetString("auth_token", token);

                            return token;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Không tìm thấy userData trong phản hồi, sử dụng username thay thế");
                        PlayerPrefs.SetString("user_id", username);

                     
                        string tokenKey = "\"token\":\"";
                        int tokenStart = jsonResponse.IndexOf(tokenKey) + tokenKey.Length;
                        int tokenEnd = jsonResponse.IndexOf("\"", tokenStart);
                        string token = jsonResponse.Substring(tokenStart, tokenEnd - tokenStart);

                       
                        PlayerPrefs.SetString("auth_token", token);

                        return token;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Lỗi khi xử lý JSON: " + e.Message);
                    return null;
                }
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
        Debug.Log("🔄 Chuyển sang màn hình HOME...");
        SceneManager.LoadScene("Home");
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
    public string user_id; }