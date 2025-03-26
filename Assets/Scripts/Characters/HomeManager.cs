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
        Debug.Log("🔍 Kiểm tra PlayerPrefs user_id: " + userId);
        Debug.Log("🔍 Kiểm tra PlayerPrefs auth_token: " + PlayerPrefs.GetString("auth_token", ""));

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID không tồn tại, quay về màn hình Login.");
            SceneManager.LoadScene("Login");
            return;
        }

        
        InitializeMapData();

        StartCoroutine(LoadGameProgress());

        logoutButton.onClick.AddListener(Logout);
    }

    void InitializeMapData()
    {
       
        mapDataDict.Add("map1", new MapData { mapID = "map1", displayName = "Map 01", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("map2", new MapData { mapID = "map2", displayName = "Map 02", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("map3", new MapData { mapID = "map3", displayName = "Map 03", status = "locked", stars = 0, highScore = 0 });
        mapDataDict.Add("map4", new MapData { mapID = "map4", displayName = "Map 04", status = "locked", stars = 0, highScore = 0 });
    }

    IEnumerator LoadGameProgress()
    {
        string url = apiUrl + userId;
        Debug.Log("📡 Gọi API: " + url);

        if (loadingIndicator) loadingIndicator.SetActive(true);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("auth_token"));

        Debug.Log("📤 Gửi request với token: " + PlayerPrefs.GetString("auth_token"));

        yield return request.SendWebRequest();

        if (loadingIndicator) loadingIndicator.SetActive(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("📥 Phản hồi từ API: " + jsonResponse);

            Debug.Log("=================================================");
            Debug.Log("📊 THÔNG TIN PROGRESS CỦA USER " + userId);
            Debug.Log(jsonResponse);
            Debug.Log("=================================================");

            try
            {
                
                GameProgress progress = ParseJsonManually(jsonResponse);

                if (progress != null)
                {
                    Debug.Log($"✅ Phân tích JSON thành công: currentMap={progress.currentMap}, totalStars={progress.totalStars}, unlockedMaps.Count={progress.unlockedMaps?.Count ?? 0}");

                    if (progress.unlockedMaps != null)
                    {
                        Debug.Log("📋 CHI TIẾT CÁC MAP TỪ API:");
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
                    Debug.LogError("❌ Không thể phân tích JSON từ API.");
                    UpdateUI(); 
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Lỗi khi xử lý JSON: " + e.Message);
                Debug.LogError("Stack trace: " + e.StackTrace);
                UpdateUI(); 
            }
        }
        else
        {
            Debug.LogError("❌ Lỗi tải tiến độ game: " + request.error);
            Debug.LogError("Response code: " + request.responseCode);
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
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi phân tích JSON thủ công: " + e.Message);
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
        Debug.Log("🔄 BẮT ĐẦU CẬP NHẬT DỮ LIỆU MAP");

     
        currentMapText.text = "Map: " + progress.currentMap;
        totalStarsText.text = "Sao: " + progress.totalStars + " ★";

        Debug.Log($"📝 Cập nhật thông tin tổng quát: Map={progress.currentMap}, Sao={progress.totalStars}");

       
        List<string> mapOrder = new List<string> { "map1", "map2", "map3", "map4" };

     
        int highestUnlockedIndex = -1;

        
        int currentMapIndex = mapOrder.IndexOf(progress.currentMap);
        if (currentMapIndex > highestUnlockedIndex)
        {
            highestUnlockedIndex = currentMapIndex;
            Debug.Log($"🔓 Map cao nhất từ currentMap: {progress.currentMap}, index={currentMapIndex}");
        }

       
        Dictionary<string, UnlockedMap> apiMapData = new Dictionary<string, UnlockedMap>();

        if (progress.unlockedMaps != null && progress.unlockedMaps.Count > 0)
        {
            Debug.Log($"📦 Số lượng map từ API: {progress.unlockedMaps.Count}");

            foreach (UnlockedMap map in progress.unlockedMaps)
            {
                
                if (!apiMapData.ContainsKey(map.mapID))
                {
                    apiMapData.Add(map.mapID, map);
                    Debug.Log($"➕ Thêm map vào apiMapData: {map.mapID}, Status={map.status}, Stars={map.stars}, HighScore={map.highScore}");
                }

               
                int mapIndex = mapOrder.IndexOf(map.mapID);
                if (mapIndex > highestUnlockedIndex &&
                    (map.status == "completed" || map.status == "unlocked"))
                {
                    highestUnlockedIndex = mapIndex;
                    Debug.Log($"🔓 Cập nhật map cao nhất từ unlockedMaps: {map.mapID}, index={mapIndex}");
                }
            }
        }

        Debug.Log($"🔑 Map cao nhất đã mở khóa: index={highestUnlockedIndex}, mapID={(highestUnlockedIndex >= 0 ? mapOrder[highestUnlockedIndex] : "none")}");

       
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

                    Debug.Log($"📊 Cập nhật dữ liệu cho {mapID}: Stars {oldStars}->{mapDataDict[mapID].stars}, HighScore {oldHighScore}->{mapDataDict[mapID].highScore}");
                }
                else
                {
                    Debug.Log($"⚠️ Không tìm thấy dữ liệu API cho {mapID}, giữ nguyên: Stars={mapDataDict[mapID].stars}, HighScore={mapDataDict[mapID].highScore}");
                }
            }
        }

       
        Debug.Log("===== THÔNG TIN MAP SAU KHI CẬP NHẬT =====");
        foreach (var entry in mapDataDict)
        {
            Debug.Log($"🗺️ Map {entry.Key}: Status={entry.Value.status}, Stars={entry.Value.stars}, HighScore={entry.Value.highScore}");
        }

        Debug.Log("🔄 KẾT THÚC CẬP NHẬT DỮ LIỆU MAP");
    }

    void UpdateUI()
    {
       
        Debug.Log("===== THÔNG TIN MAP TRƯỚC KHI CẬP NHẬT UI =====");
        foreach (var entry in mapDataDict)
        {
            Debug.Log($"Map {entry.Key}: Status={entry.Value.status}, Stars={entry.Value.stars}, HighScore={entry.Value.highScore}");
        }

       
        foreach (Transform child in mapsGrid)
        {
            Destroy(child.gameObject);
        }

       
        if (mapsGrid == null)
        {
            Debug.LogError("mapsGrid không được gán trong Inspector!");
            return;
        }

        
        if (mapPanelPrefab == null)
        {
            Debug.LogError("mapPanelPrefab không được gán trong Inspector!");
            return;
        }

       
        int index = 0;
        foreach (var mapEntry in mapDataDict)
        {
            MapData mapData = mapEntry.Value;
            Debug.Log($"Tạo panel cho map {mapData.mapID}: Status={mapData.status}, Stars={mapData.stars}, HighScore={mapData.highScore}");

           
            GameObject mapPanel = Instantiate(mapPanelPrefab, mapsGrid);

            try
            {
                
                Image[] images = mapPanel.GetComponentsInChildren<Image>(true);
                Debug.Log($"Số lượng Image trong panel: {images.Length}");

               
                TMP_Text[] texts = mapPanel.GetComponentsInChildren<TMP_Text>(true);
                Debug.Log($"Số lượng TMP_Text trong panel: {texts.Length}");

                
                if (images.Length > 0 && index < mapImages.Length && mapImages[index] != null)
                {
                    images[0].sprite = mapImages[index];
                    Debug.Log($"Đã cập nhật hình ảnh cho {mapData.mapID}");
                }

                
                if (texts.Length > 0)
                {
                    texts[0].text = mapData.displayName;
                    Debug.Log($"Đã cập nhật tên cho {mapData.mapID}: {mapData.displayName}");
                }

                
                if (texts.Length > 1)
                {
                    string oldText = texts[1].text;
                    texts[1].text = "★ " + mapData.stars;
                    Debug.Log($"Đã cập nhật số sao cho {mapData.mapID}: {mapData.stars}, Text thay đổi từ '{oldText}' thành '{texts[1].text}'");
                }

                
                if (texts.Length > 2)
                {
                    string oldText = texts[2].text;
                    texts[2].text = "♛ " + mapData.highScore;
                    Debug.Log($"Đã cập nhật điểm cao cho {mapData.mapID}: {mapData.highScore}, Text thay đổi từ '{oldText}' thành '{texts[2].text}'");
                }

                
                if (images.Length > 1 && lockIcon != null && unlockIcon != null)
                {
                    images[1].sprite = mapData.status == "unlocked" ? unlockIcon : lockIcon;
                    Debug.Log($"Đã cập nhật icon trạng thái cho {mapData.mapID}: {mapData.status}");
                }

                
                Button mapButton = mapPanel.GetComponent<Button>();
                if (mapButton != null)
                {
                    
                    bool isUnlocked = mapData.status == "completed" || mapData.status == "unlocked";
                    mapButton.interactable = isUnlocked;
                    Debug.Log($"Đã cập nhật button cho {mapData.mapID}: interactable={isUnlocked}, status={mapData.status}");

                    
                    string mapID = mapData.mapID;
                    mapButton.onClick.RemoveAllListeners();
                    mapButton.onClick.AddListener(() => LoadMap(mapID));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi cập nhật UI cho map {mapData.mapID}: {e.Message}\n{e.StackTrace}");
            }

            index++;
        }
    }

    void LoadMap(string mapID)
    {
        Debug.Log("Chon Map: " + mapID);
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

        Debug.Log("Đang đăng xuất...");

        
        yield return null;

        Debug.Log("Chuyển về màn hình đăng nhập");
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
