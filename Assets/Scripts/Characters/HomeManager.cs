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
    public Transform mapsGrid; // Grid Layout Group chứa các map
    public GameObject mapPanelPrefab; // Prefab cho mỗi map panel
    public Button logoutButton;
    public GameObject loadingIndicator;

    [Header("Map Images")]
    public Sprite[] mapImages; // Mảng hình ảnh cho các map
    public Sprite lockIcon; // Icon khóa
    public Sprite unlockIcon; // Icon mở khóa

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

        // Khởi tạo dữ liệu map mặc định
        InitializeMapData();

        StartCoroutine(LoadGameProgress());

        logoutButton.onClick.AddListener(Logout);
    }

    void InitializeMapData()
    {
        // Khởi tạo dữ liệu mặc định cho 4 map - Sửa key để khớp với mapID
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

            // Hiển thị dữ liệu JSON đã nhận được
            Debug.Log("=================================================");
            Debug.Log("📊 THÔNG TIN PROGRESS CỦA USER " + userId);
            Debug.Log(jsonResponse);
            Debug.Log("=================================================");

            try
            {
                // Phân tích JSON thủ công
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

                    // Cập nhật dữ liệu map và UI
                    UpdateMapData(progress);
                    UpdateUI();
                }
                else
                {
                    Debug.LogError("❌ Không thể phân tích JSON từ API.");
                    UpdateUI(); // Hiển thị dữ liệu mặc định
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Lỗi khi xử lý JSON: " + e.Message);
                Debug.LogError("Stack trace: " + e.StackTrace);
                UpdateUI(); // Hiển thị dữ liệu mặc định
            }
        }
        else
        {
            Debug.LogError("❌ Lỗi tải tiến độ game: " + request.error);
            Debug.LogError("Response code: " + request.responseCode);
            UpdateUI(); // Hiển thị dữ liệu mặc định
        }
    }

    // Phân tích JSON thủ công
    GameProgress ParseJsonManually(string json)
    {
        try
        {
            // Tạo một đối tượng GameProgress mới
            GameProgress progress = new GameProgress();
            progress.unlockedMaps = new List<UnlockedMap>();

            // Lấy giá trị status
            string statusKey = "\"status\":\"";
            int statusStart = json.IndexOf(statusKey) + statusKey.Length;
            int statusEnd = json.IndexOf("\"", statusStart);
            progress.status = json.Substring(statusStart, statusEnd - statusStart);

            // Lấy giá trị currentMap
            string currentMapKey = "\"currentMap\":\"";
            int currentMapStart = json.IndexOf(currentMapKey) + currentMapKey.Length;
            int currentMapEnd = json.IndexOf("\"", currentMapStart);
            progress.currentMap = json.Substring(currentMapStart, currentMapEnd - currentMapStart);

            // Lấy giá trị totalStars
            string totalStarsKey = "\"totalStars\":";
            int totalStarsStart = json.IndexOf(totalStarsKey) + totalStarsKey.Length;
            int totalStarsEnd = json.IndexOf(",", totalStarsStart);
            if (totalStarsEnd == -1) // Nếu không có dấu phẩy, có thể là cuối chuỗi
                totalStarsEnd = json.IndexOf("}", totalStarsStart);
            string totalStarsStr = json.Substring(totalStarsStart, totalStarsEnd - totalStarsStart).Trim();
            progress.totalStars = int.Parse(totalStarsStr);

            // Lấy mảng unlockedMaps
            string unlockedMapsKey = "\"unlockedMaps\":";
            int unlockedMapsStart = json.IndexOf(unlockedMapsKey) + unlockedMapsKey.Length;

            // Tìm vị trí bắt đầu của mảng
            int arrayStart = json.IndexOf("[", unlockedMapsStart);

            // Tìm vị trí kết thúc của mảng
            int arrayEnd = json.LastIndexOf("]");

            // Lấy chuỗi JSON của mảng
            string arrayJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            // Tách các đối tượng trong mảng
            List<string> mapJsons = SplitJsonArray(arrayJson);

            foreach (string mapJson in mapJsons)
            {
                UnlockedMap map = new UnlockedMap();

                // Lấy giá trị mapID
                string mapIDKey = "\"mapID\":\"";
                int mapIDStart = mapJson.IndexOf(mapIDKey) + mapIDKey.Length;
                int mapIDEnd = mapJson.IndexOf("\"", mapIDStart);
                map.mapID = mapJson.Substring(mapIDStart, mapIDEnd - mapIDStart);

                // Lấy giá trị status
                string mapStatusKey = "\"status\":\"";
                int mapStatusStart = mapJson.IndexOf(mapStatusKey) + mapStatusKey.Length;
                int mapStatusEnd = mapJson.IndexOf("\"", mapStatusStart);
                map.status = mapJson.Substring(mapStatusStart, mapStatusEnd - mapStatusStart);

                // Lấy giá trị stars
                string starsKey = "\"stars\":";
                int starsStart = mapJson.IndexOf(starsKey) + starsKey.Length;
                int starsEnd = mapJson.IndexOf(",", starsStart);
                if (starsEnd == -1) // Nếu không có dấu phẩy, có thể là cuối chuỗi
                    starsEnd = mapJson.IndexOf("}", starsStart);
                string starsStr = mapJson.Substring(starsStart, starsEnd - starsStart).Trim();
                map.stars = int.Parse(starsStr);

                // Lấy giá trị highScore
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

    // Tách các đối tượng trong mảng JSON
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

        // Cập nhật thông tin tổng quát
        currentMapText.text = "Map: " + progress.currentMap;
        totalStarsText.text = "Sao: " + progress.totalStars + " ★";

        Debug.Log($"📝 Cập nhật thông tin tổng quát: Map={progress.currentMap}, Sao={progress.totalStars}");

        // Danh sách các map theo thứ tự
        List<string> mapOrder = new List<string> { "map1", "map2", "map3", "map4" };

        // Tìm map cao nhất đã mở khóa từ API
        int highestUnlockedIndex = -1;

        // Tìm index của currentMap trong mapOrder
        int currentMapIndex = mapOrder.IndexOf(progress.currentMap);
        if (currentMapIndex > highestUnlockedIndex)
        {
            highestUnlockedIndex = currentMapIndex;
            Debug.Log($"🔓 Map cao nhất từ currentMap: {progress.currentMap}, index={currentMapIndex}");
        }

        // Tạo một Dictionary để lưu trữ dữ liệu từ API theo mapID
        Dictionary<string, UnlockedMap> apiMapData = new Dictionary<string, UnlockedMap>();

        if (progress.unlockedMaps != null && progress.unlockedMaps.Count > 0)
        {
            Debug.Log($"📦 Số lượng map từ API: {progress.unlockedMaps.Count}");

            foreach (UnlockedMap map in progress.unlockedMaps)
            {
                // Lưu dữ liệu map từ API vào dictionary
                if (!apiMapData.ContainsKey(map.mapID))
                {
                    apiMapData.Add(map.mapID, map);
                    Debug.Log($"➕ Thêm map vào apiMapData: {map.mapID}, Status={map.status}, Stars={map.stars}, HighScore={map.highScore}");
                }

                // Tìm vị trí của map này trong danh sách thứ tự
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

        // Mở khóa tất cả các map từ map1 đến map cao nhất đã mở khóa
        for (int i = 0; i <= highestUnlockedIndex; i++)
        {
            string mapID = mapOrder[i];
            if (mapDataDict.ContainsKey(mapID))
            {
                mapDataDict[mapID].status = "unlocked";

                // Cập nhật số sao và điểm cao từ API nếu có
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

        // In ra thông tin tất cả các map sau khi cập nhật
        Debug.Log("===== THÔNG TIN MAP SAU KHI CẬP NHẬT =====");
        foreach (var entry in mapDataDict)
        {
            Debug.Log($"🗺️ Map {entry.Key}: Status={entry.Value.status}, Stars={entry.Value.stars}, HighScore={entry.Value.highScore}");
        }

        Debug.Log("🔄 KẾT THÚC CẬP NHẬT DỮ LIỆU MAP");
    }

    void UpdateUI()
    {
        // In ra thông tin tất cả các map trước khi cập nhật UI
        Debug.Log("===== THÔNG TIN MAP TRƯỚC KHI CẬP NHẬT UI =====");
        foreach (var entry in mapDataDict)
        {
            Debug.Log($"Map {entry.Key}: Status={entry.Value.status}, Stars={entry.Value.stars}, HighScore={entry.Value.highScore}");
        }

        // Xóa tất cả các map panel cũ
        foreach (Transform child in mapsGrid)
        {
            Destroy(child.gameObject);
        }

        // Kiểm tra mapsGrid
        if (mapsGrid == null)
        {
            Debug.LogError("mapsGrid không được gán trong Inspector!");
            return;
        }

        // Kiểm tra mapPanelPrefab
        if (mapPanelPrefab == null)
        {
            Debug.LogError("mapPanelPrefab không được gán trong Inspector!");
            return;
        }

        // Tạo các map panel mới
        int index = 0;
        foreach (var mapEntry in mapDataDict)
        {
            MapData mapData = mapEntry.Value;
            Debug.Log($"Tạo panel cho map {mapData.mapID}: Status={mapData.status}, Stars={mapData.stars}, HighScore={mapData.highScore}");

            // Tạo map panel
            GameObject mapPanel = Instantiate(mapPanelPrefab, mapsGrid);

            try
            {
                // Tìm tất cả các Image trong mapPanel
                Image[] images = mapPanel.GetComponentsInChildren<Image>(true);
                Debug.Log($"Số lượng Image trong panel: {images.Length}");

                // Tìm tất cả các Text trong mapPanel
                TMP_Text[] texts = mapPanel.GetComponentsInChildren<TMP_Text>(true);
                Debug.Log($"Số lượng TMP_Text trong panel: {texts.Length}");

                // Cập nhật hình ảnh map (giả sử image đầu tiên là hình ảnh map)
                if (images.Length > 0 && index < mapImages.Length && mapImages[index] != null)
                {
                    images[0].sprite = mapImages[index];
                    Debug.Log($"Đã cập nhật hình ảnh cho {mapData.mapID}");
                }

                // Cập nhật tên map (giả sử text đầu tiên là tên map)
                if (texts.Length > 0)
                {
                    texts[0].text = mapData.displayName;
                    Debug.Log($"Đã cập nhật tên cho {mapData.mapID}: {mapData.displayName}");
                }

                // Cập nhật số sao (giả sử text thứ hai là số sao)
                if (texts.Length > 1)
                {
                    string oldText = texts[1].text;
                    texts[1].text = "★ " + mapData.stars;
                    Debug.Log($"Đã cập nhật số sao cho {mapData.mapID}: {mapData.stars}, Text thay đổi từ '{oldText}' thành '{texts[1].text}'");
                }

                // Cập nhật điểm cao (giả sử text thứ ba là điểm cao)
                if (texts.Length > 2)
                {
                    string oldText = texts[2].text;
                    texts[2].text = "♛ " + mapData.highScore;
                    Debug.Log($"Đã cập nhật điểm cao cho {mapData.mapID}: {mapData.highScore}, Text thay đổi từ '{oldText}' thành '{texts[2].text}'");
                }

                // Cập nhật icon trạng thái (giả sử image thứ hai là icon trạng thái)
                if (images.Length > 1 && lockIcon != null && unlockIcon != null)
                {
                    images[1].sprite = mapData.status == "unlocked" ? unlockIcon : lockIcon;
                    Debug.Log($"Đã cập nhật icon trạng thái cho {mapData.mapID}: {mapData.status}");
                }

                // Cập nhật button
                Button mapButton = mapPanel.GetComponent<Button>();
                if (mapButton != null)
                {
                    // Cho phép click nếu map đã hoàn thành hoặc đã mở khóa
                    bool isUnlocked = mapData.status == "completed" || mapData.status == "unlocked";
                    mapButton.interactable = isUnlocked;
                    Debug.Log($"Đã cập nhật button cho {mapData.mapID}: interactable={isUnlocked}, status={mapData.status}");

                    // Thêm sự kiện click
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

        // Đợi một frame
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
    public string status; // "locked" hoặc "unlocked"
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
