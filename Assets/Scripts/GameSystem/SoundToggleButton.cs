using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundToggleButton : MonoBehaviour
{
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Image iconImage;

    private string buttonName;
    private string sceneName;

    void Awake()
    {
        buttonName = gameObject.name;
        sceneName = SceneManager.GetActiveScene().name;

    }

    void Start()
    {



        if (AudioManager.Instance == null)
        {

            return;
        }


        UpdateIcon();
    }

    public void OnClickToggleSound()
    {



        if (AudioManager.Instance == null)
        {

            return;
        }


        AudioManager.Instance.ToggleSound();


        UpdateIcon();
    }

    void UpdateIcon()
    {



        if (AudioManager.Instance == null)
        {

            return;
        }


        if (iconImage == null)
        {

            return;
        }


        if (soundOnSprite == null || soundOffSprite == null)
        {

            return;
        }


        bool isMuted = AudioManager.Instance.IsMuted();
        iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;

    }
}
