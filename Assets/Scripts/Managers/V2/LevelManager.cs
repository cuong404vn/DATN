using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using SimpleJSON;

public class LevelManager : MonoBehaviour
{
    [Header("Game parameters")]
    public float timeFor3Stars = 300f; 
    public float timeFor2Stars = 600f; 
    public float timeFor1Star = 900f; 
    public int pointsPerEnemy = 50;
    public int coinToPointsRatio = 10; 

    [Header("References")]
    public GameObject exitGate;
    public GameObject summaryScreen;
    public Transform summaryContent;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalStarsText;
    public TextMeshProUGUI enemiesDefeatedText;
    public TextMeshProUGUI coinsCollectedText;
    public GameObject[] starIcons;

  
    private float gameTime;
    private int enemiesDefeated;
    private int totalScore;
    private int earnedStars;
    private bool isGameCompleted = false;
    private string currentMapID;

   
    private string updateProgressURL = "https://shopnickgame.online/api/progress/save";

    void Start()
    {
      
        gameTime = 0f;
        enemiesDefeated = 0;
        totalScore = 0;

      
        if (exitGate != null) exitGate.SetActive(false);
        if (summaryScreen != null) summaryScreen.SetActive(false);

      
        currentMapID = SceneManager.GetActiveScene().name;
        Debug.Log("Start Game " + currentMapID);

      
        UpdateUI();
    }

    void Update()
    {
        if (!isGameCompleted)
        {
        
            gameTime += Time.deltaTime;

           
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            if (timerText != null)
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

  
    public void EnemyDefeated()
    {
        enemiesDefeated++;
        totalScore += pointsPerEnemy;
        UpdateUI();
    }

  
    public void UpdateScore(int additionalPoints)
    {
        totalScore += additionalPoints;
        UpdateUI();
    }

  
    public void ShowExitGate(Vector3 position)
    {
        if (exitGate != null)
        {
            exitGate.transform.position = position;
            exitGate.SetActive(true);
        }
    }

    
    public void CompleteLevel()
    {
        if (isGameCompleted) return;

        isGameCompleted = true;

        
        if (gameTime <= timeFor3Stars)
            earnedStars = 3;
        else if (gameTime <= timeFor2Stars)
            earnedStars = 2;
        else if (gameTime <= timeFor1Star)
            earnedStars = 1;
        else
            earnedStars = 1; 

        
        int coinPoints = GameManager.Instance.coins * coinToPointsRatio;
        totalScore += coinPoints;

        
        ShowSummary();

        
        StartCoroutine(UpdateProgressToServer());
    }

    
    private void ShowSummary()
    {
        if (summaryScreen != null)
        {
            summaryScreen.SetActive(true);

            
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);

            if (finalTimeText != null)
                finalTimeText.text = string.Format("Time Done: {0:00}:{1:00}", minutes, seconds);

            if (finalScoreText != null)
                finalScoreText.text = "Poin: " + totalScore;

            if (finalStarsText != null)
                finalStarsText.text = "Star: " + earnedStars;

            if (enemiesDefeatedText != null)
                enemiesDefeatedText.text = "Monsters kill: " + enemiesDefeated;

            if (coinsCollectedText != null)
                coinsCollectedText.text = "Coin:  " + GameManager.Instance.coins;

            
            if (starIcons != null && starIcons.Length >= 3)
            {
                for (int i = 0; i < starIcons.Length; i++)
                {
                    starIcons[i].SetActive(i < earnedStars);
                }
            }
        }
    }

    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
    }

        private IEnumerator UpdateProgressToServer()
    {
        string userId = PlayerPrefs.GetString("user_id", "");
        string token = PlayerPrefs.GetString("auth_token", "");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            
            yield break;
        }

       
        string jsonData = "{\"userID\":\"" + userId + "\",\"mapID\":\"" + currentMapID +
                          "\",\"status\":\"completed\",\"stars\":" + earnedStars +
                          ",\"score\":" + totalScore + "}";

        

        
        UnityWebRequest request = new UnityWebRequest(updateProgressURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

       
        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;
        

        if (responseText.Contains("\"status\":\"success\""))
        {
           
        }
        else
        {
            
        }
    }

  
    public void ReturnToHome()
    {
        SceneManager.LoadScene("Home");
    }

  
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}