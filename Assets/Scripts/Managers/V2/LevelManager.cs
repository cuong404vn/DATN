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

    public static bool IsComingFromPortal = false;

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

    [Header("Audio")]
    public AudioClip victorySound;

    private float gameTime;
    private int enemiesDefeated;
    private int totalScore;
    private int earnedStars;
    private bool isGameCompleted = false;
    private string currentMapID;
    private bool isFromPortal = false;
    private string originalMapID;

    private string updateProgressURL = "https://shopnickgame.online/api/progress/save";


    private void Awake()
    {

        int isTransitioning = PlayerPrefs.GetInt("IsTransitioningScene", 0);


        if (isTransitioning == 1)
        {

            IsComingFromPortal = true;


            originalMapID = PlayerPrefs.GetString("OriginalMapID", "");

        }
    }

    void Start()
    {

        bool isRestarting = PlayerPrefs.GetInt("IsRestarting", 0) == 1;


        int isTransitioning = PlayerPrefs.GetInt("IsTransitioningScene", 0);



        if (isRestarting)
        {

            gameTime = 0f;
            enemiesDefeated = 0;
            totalScore = 0;

            originalMapID = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("OriginalMapID", originalMapID);
            PlayerPrefs.Save();


        }
        else if (IsComingFromPortal || isTransitioning == 1)
        {

            isFromPortal = true;


            gameTime = PlayerPrefs.GetFloat("GameTime", 0f);
            totalScore = PlayerPrefs.GetInt("TotalScore", 0);
            enemiesDefeated = PlayerPrefs.GetInt("EnemiesDefeated", 0);


            if (string.IsNullOrEmpty(originalMapID))
            {
                originalMapID = PlayerPrefs.GetString("OriginalMapID", "");

            }



            PlayerPrefs.SetInt("IsTransitioningScene", 0);
            PlayerPrefs.Save();
            IsComingFromPortal = false;

        }
        else
        {
            gameTime = 0f;
            enemiesDefeated = 0;
            totalScore = 0;

            originalMapID = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("OriginalMapID", originalMapID);
            PlayerPrefs.Save();

        }

        if (exitGate != null) exitGate.SetActive(false);
        if (summaryScreen != null) summaryScreen.SetActive(false);

        currentMapID = SceneManager.GetActiveScene().name;


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


    public void SaveLevelState()
    {


        PlayerPrefs.SetFloat("GameTime", gameTime);
        PlayerPrefs.SetInt("TotalScore", totalScore);
        PlayerPrefs.SetInt("EnemiesDefeated", enemiesDefeated);




        if (string.IsNullOrEmpty(originalMapID))
        {
            originalMapID = SceneManager.GetActiveScene().name;

        }


        PlayerPrefs.SetInt("IsTransitioningScene", 1);
        PlayerPrefs.SetString("OriginalMapID", originalMapID);
        PlayerPrefs.Save();


        IsComingFromPortal = true;


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


        if (string.IsNullOrEmpty(originalMapID))
        {
            originalMapID = PlayerPrefs.GetString("OriginalMapID", "");
            if (string.IsNullOrEmpty(originalMapID))
            {
                originalMapID = SceneManager.GetActiveScene().name;

            }
            else
            {

            }
        }


        StartCoroutine(UpdateProgressToServer());
    }

    private void ShowSummary()
    {
        if (summaryScreen != null)
        {

            Time.timeScale = 0f;

            summaryScreen.SetActive(true);


            PlayVictorySound();

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
            scoreText.text = "" + totalScore;
    }

    private IEnumerator UpdateProgressToServer()
    {
        string userId = PlayerPrefs.GetString("user_id", "");
        string token = PlayerPrefs.GetString("auth_token", "");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {

            yield break;
        }


        if (string.IsNullOrEmpty(originalMapID))
        {
            originalMapID = PlayerPrefs.GetString("OriginalMapID", SceneManager.GetActiveScene().name);

        }


        string mapToUpdate = originalMapID;


        string jsonData = "{\"userID\":\"" + userId + "\",\"mapID\":\"" + mapToUpdate +
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

        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }


    private void PlayVictorySound()
    {
        if (victorySound != null)
        {

            if (GameAudioController.Instance != null)
            {
                GameAudioController.Instance.PlayNewMusic(victorySound);
            }
            else
            {

                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = victorySound;
                audioSource.volume = 0.7f;
                audioSource.Play();


                if (AudioManager.Instance != null && AudioManager.Instance.IsMuted())
                {
                    audioSource.mute = true;
                }
            }
        }
    }

    public void RestartLevel()
    {

        Time.timeScale = 1f;

        string originalMapID = PlayerPrefs.GetString("OriginalMapID", "");


        if (string.IsNullOrEmpty(originalMapID))
        {
            originalMapID = SceneManager.GetActiveScene().name;

        }


        GameManager.Instance.ResetLevelStats();


        PlayerPrefs.DeleteKey("IsTransitioningScene");
        PlayerPrefs.DeleteKey("PlayerCurrentHealth");
        PlayerPrefs.DeleteKey("PlayerCurrentPotions");
        PlayerPrefs.DeleteKey("GameTime");
        PlayerPrefs.DeleteKey("TotalScore");
        PlayerPrefs.DeleteKey("EnemiesDefeated");
        PlayerPrefs.DeleteKey("PlayerCoins");
        PlayerPrefs.DeleteKey("PlayerKeys");


        PlayerPrefs.SetInt("IsRestarting", 1);
        PlayerPrefs.Save();


        SceneManager.LoadScene(originalMapID);
    }
}