using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text currentMapText;
    public TMP_Text totalStarsText;
    public Transform mapsGrid;
    public GameObject mapPanelPrefab;
    public Button logoutButton;
    public GameObject loadingIndicator;

    [Header("Map Images")]
    public Sprite[] mapImages;
    public Sprite lockIcon;
    public Sprite unlockIcon;

    private string userId;
    private string apiUrl = "https://shopnickgame.online/api/progress/get/";
    private Dictionary<string, MapData> mapDataDict = new Dictionary<string, MapData>();

    void Start()
    {
        userId = PlayerPrefs.GetString("user_id", "");
        string authToken = PlayerPrefs.GetString("auth_token", "");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(authToken))
        {
            Debug.Log("Missing userId or token, returning to login");
            SceneManager.LoadScene("Login");
            return;
        }

        InitializeMapData();
        StartCoroutine(LoadGameProgress());
        logoutButton.onClick.AddListener(Logout);
    }

    void InitializeMapData()
    {

        mapDataDict.Add("ToaThanh", new MapData { mapID = "ToaThanh", displayName = "The Citadel", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("KhuRung", new MapData { mapID = "KhuRung", displayName = "The forest", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("LongDat", new MapData { mapID = "LongDat", displayName = "The Underground ", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("CamThanh", new MapData { mapID = "CamThanh", displayName = "The Sealed Citadel", status = "locked", stars = 0, highScore = 0 });
    }

    IEnumerator LoadGameProgress()
    {
        string url = apiUrl + userId;
        if (loadingIndicator) loadingIndicator.SetActive(true);

        UnityWebRequest request = UnityWebRequest.Get(url);
        string authToken = PlayerPrefs.GetString("auth_token", "");
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Progress Response: " + jsonResponse);

            if (jsonResponse.Contains("error") || jsonResponse.Contains("unauthorized"))
            {
                Debug.Log("Token invalid or expired, returning to login");
                Logout();
                yield break;
            }

            try
            {
                GameProgress progress = ParseJsonManually(jsonResponse);

                if (progress != null)
                {
                    if (progress.unlockedMaps != null)
                    {
                        foreach (var map in progress.unlockedMaps)
                        {
                            Debug.Log($"  🗺️ Map: {map.mapID}, Status: {map.status}, Stars: {map.stars}, HighScore: {map.highScore}");
                        }
                    }

                    UpdateMapData(progress);
                    UpdateUI();
                }
                else
                {
                    UpdateUI();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing progress: " + e.Message);
                UpdateUI();
            }
        }
        else
        {
            Debug.LogError("Failed to load progress: " + request.error);
            if (request.responseCode == 401)
            {
                Debug.Log("Unauthorized access, returning to login");
                Logout();
                yield break;
            }
            UpdateUI();
        }
    }

    GameProgress ParseJsonManually(string json)
    {
        try
        {
            GameProgress progress = new GameProgress();
            progress.unlockedMaps = new List<UnlockedMap>();

            string statusKey = "\"status\":\"";
            int statusStart = json.IndexOf(statusKey) + statusKey.Length;
            int statusEnd = json.IndexOf("\"", statusStart);
            progress.status = json.Substring(statusStart, statusEnd - statusStart);

            string currentMapKey = "\"currentMap\":\"";
            int currentMapStart = json.IndexOf(currentMapKey) + currentMapKey.Length;
            int currentMapEnd = json.IndexOf("\"", currentMapStart);
            progress.currentMap = json.Substring(currentMapStart, currentMapEnd - currentMapStart);

            string totalStarsKey = "\"totalStars\":";
            int totalStarsStart = json.IndexOf(totalStarsKey) + totalStarsKey.Length;
            int totalStarsEnd = json.IndexOf(",", totalStarsStart);
            if (totalStarsEnd == -1)
                totalStarsEnd = json.IndexOf("}", totalStarsStart);
            string totalStarsStr = json.Substring(totalStarsStart, totalStarsEnd - totalStarsStart).Trim();
            progress.totalStars = int.Parse(totalStarsStr);

            string unlockedMapsKey = "\"unlockedMaps\":";
            int unlockedMapsStart = json.IndexOf(unlockedMapsKey) + unlockedMapsKey.Length;

            int arrayStart = json.IndexOf("[", unlockedMapsStart);

            int arrayEnd = json.LastIndexOf("]");

            string arrayJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            List<string> mapJsons = SplitJsonArray(arrayJson);

            foreach (string mapJson in mapJsons)
            {
                UnlockedMap map = new UnlockedMap();

                string mapIDKey = "\"mapID\":\"";
                int mapIDStart = mapJson.IndexOf(mapIDKey) + mapIDKey.Length;
                int mapIDEnd = mapJson.IndexOf("\"", mapIDStart);
                map.mapID = mapJson.Substring(mapIDStart, mapIDEnd - mapIDStart);

                string mapStatusKey = "\"status\":\"";
                int mapStatusStart = mapJson.IndexOf(mapStatusKey) + mapStatusKey.Length;
                int mapStatusEnd = mapJson.IndexOf("\"", mapStatusStart);
                map.status = mapJson.Substring(mapStatusStart, mapStatusEnd - mapStatusStart);

                string starsKey = "\"stars\":";
                int starsStart = mapJson.IndexOf(starsKey) + starsKey.Length;
                int starsEnd = mapJson.IndexOf(",", starsStart);
                if (starsEnd == -1)
                    starsEnd = mapJson.IndexOf("}", starsStart);
                string starsStr = mapJson.Substring(starsStart, starsEnd - starsStart).Trim();
                map.stars = int.Parse(starsStr);

                string highScoreKey = "\"highScore\":";
                int highScoreStart = mapJson.IndexOf(highScoreKey) + highScoreKey.Length;
                int highScoreEnd = mapJson.IndexOf("}", highScoreStart);
                string highScoreStr = mapJson.Substring(highScoreStart, highScoreEnd - highScoreStart).Trim();
                map.highScore = int.Parse(highScoreStr);

                progress.unlockedMaps.Add(map);
            }

            return progress;
        }
        catch
        {
            return null;
        }
    }

    List<string> SplitJsonArray(string arrayJson)
    {
        List<string> result = new List<string>();

        int startIndex = 0;
        int braceCount = 0;

        for (int i = 0; i < arrayJson.Length; i++)
        {
            char c = arrayJson[i];

            if (c == '{')
            {
                if (braceCount == 0)
                    startIndex = i;
                braceCount++;
            }
            else if (c == '}')
            {
                braceCount--;
                if (braceCount == 0)
                {
                    string objectJson = arrayJson.Substring(startIndex, i - startIndex + 1);
                    result.Add(objectJson);
                }
            }
        }

        return result;
    }

    void UpdateMapData(GameProgress progress)
    {
        currentMapText.text = "Map: " + progress.currentMap;
        totalStarsText.text = "Total stars" + progress.totalStars ;

        List<string> mapOrder = new List<string> { "ToaThanh", "KhuRung", "LongDat", "CamThanh" };

        int highestUnlockedIndex = -1;

        int currentMapIndex = mapOrder.IndexOf(progress.currentMap);
        if (currentMapIndex > highestUnlockedIndex)
        {
            highestUnlockedIndex = currentMapIndex;
        }

        Dictionary<string, UnlockedMap> apiMapData = new Dictionary<string, UnlockedMap>();

        if (progress.unlockedMaps != null && progress.unlockedMaps.Count > 0)
        {
            foreach (UnlockedMap map in progress.unlockedMaps)
            {
                if (!apiMapData.ContainsKey(map.mapID))
                {
                    apiMapData.Add(map.mapID, map);
                }

                int mapIndex = mapOrder.IndexOf(map.mapID);
                if (mapIndex > highestUnlockedIndex &&
                    (map.status == "completed" || map.status == "unlocked"))
                {
                    highestUnlockedIndex = mapIndex;
                }
            }
        }

        for (int i = 0; i <= highestUnlockedIndex; i++)
        {
            string mapID = mapOrder[i];
            if (mapDataDict.ContainsKey(mapID))
            {
                mapDataDict[mapID].status = "unlocked";

                if (apiMapData.ContainsKey(mapID))
                {
                    int oldStars = mapDataDict[mapID].stars;
                    int oldHighScore = mapDataDict[mapID].highScore;

                    mapDataDict[mapID].stars = apiMapData[mapID].stars;
                    mapDataDict[mapID].highScore = apiMapData[mapID].highScore;
                }
                else
                {

                }
            }
        }

        foreach (var entry in mapDataDict)
        {

        }
    }

    void UpdateUI()
    {
        foreach (var entry in mapDataDict)
        {

        }

        foreach (Transform child in mapsGrid)
        {
            Destroy(child.gameObject);
        }

        if (mapsGrid == null)
        {
            return;
        }

        if (mapPanelPrefab == null)
        {
            return;
        }

        int index = 0;
        foreach (var mapEntry in mapDataDict)
        {
            MapData mapData = mapEntry.Value;

            GameObject mapPanel = Instantiate(mapPanelPrefab, mapsGrid);

            try
            {
                Image[] images = mapPanel.GetComponentsInChildren<Image>(true);

                TMP_Text[] texts = mapPanel.GetComponentsInChildren<TMP_Text>(true);

                if (images.Length > 0 && index < mapImages.Length && mapImages[index] != null)
                {
                    images[0].sprite = mapImages[index];
                }

                if (texts.Length > 0)
                {
                    texts[0].text = mapData.displayName;
                }

                if (texts.Length > 1)
                {
                    string oldText = texts[1].text;
                    texts[1].text = "" + mapData.stars;
                }

                if (texts.Length > 2)
                {
                    string oldText = texts[2].text;
                    texts[2].text = "SCORE: " + mapData.highScore;
                }

                if (images.Length > 1 && lockIcon != null && unlockIcon != null)
                {
                    images[1].sprite = mapData.status == "unlocked" ? unlockIcon : lockIcon;
                }

                Button mapButton = mapPanel.GetComponent<Button>();
                if (mapButton != null)
                {
                    bool isUnlocked = mapData.status == "completed" || mapData.status == "unlocked";
                    mapButton.interactable = isUnlocked;

                    string mapID = mapData.mapID;
                    mapButton.onClick.RemoveAllListeners();
                    mapButton.onClick.AddListener(() => LoadMap(mapID));
                }
            }
            catch
            {

            }

            index++;
        }
    }

    void LoadMap(string mapID)
    {
        PlayerPrefs.SetString("current_map", mapID);
        SceneManager.LoadScene(mapID);
    }

    public void Logout()
    {
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("current_map");
        PlayerPrefs.Save();

        yield return null;

        SceneManager.LoadScene("Login");
    }
}

[System.Serializable]
public class GameProgress
{
    public string status;
    public string currentMap;
    public int totalStars;
    public List<UnlockedMap> unlockedMaps;
}

[System.Serializable]
public class UnlockedMap
{
    public string mapID;
    public string status;
    public int stars;
    public int highScore;
}

[System.Serializable]
public class MapData
{
    public string mapID;
    public string displayName;
    public string status;
    public int stars;
    public int highScore;
}

[System.Serializable]
public class ApiResponse
{
    public string status;
    public string currentMap;
    public int totalStars;
    public List<ApiMap> unlockedMaps;
}

[System.Serializable]
public class ApiMap
{
    public string mapID;
    public string status;
    public int stars;
    public int highScore;
}
