using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StoryVideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button skipButton;

    void Start()
    {
        skipButton.onClick.AddListener(SkipVideo);

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void SkipVideo()
    {
        string userId = PlayerPrefs.GetString("user_id");
        string watchedKey = userId + "_WatchedStory";

        PlayerPrefs.SetInt(watchedKey, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Home");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        string userId = PlayerPrefs.GetString("user_id");
        string watchedKey = userId + "_WatchedStory";

        PlayerPrefs.SetInt(watchedKey, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Home");
    }
}
