using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI keysText;

    [Header("Audio")]
    public AudioClip coinCollectSound;
    public AudioClip keyCollectSound;
    private AudioSource audioSource;

    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;

        if (coinsText == null)
            coinsText = GameObject.Find("CoinsText")?.GetComponent<TextMeshProUGUI>();

        if (keysText == null)
            keysText = GameObject.Find("KeysText")?.GetComponent<TextMeshProUGUI>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += UpdateCoinsUI;
            GameManager.Instance.OnKeysChanged += UpdateKeysUI;

            UpdateCoinsUI(GameManager.Instance.coins);
            UpdateKeysUI(GameManager.Instance.keys);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            GameManager.Instance.OnKeysChanged -= UpdateKeysUI;
        }
    }

    void UpdateCoinsUI(int value)
    {

        int oldValue = 0;
        if (coinsText != null && !string.IsNullOrEmpty(coinsText.text))
        {
            int.TryParse(coinsText.text, out oldValue);
        }


        if (coinsText != null)
            coinsText.text = value.ToString();


        if (coinCollectSound != null && value > oldValue)
        {

            PlaySound(coinCollectSound);
        }
    }

    void UpdateKeysUI(int value)
    {

        int oldValue = 0;
        if (keysText != null && !string.IsNullOrEmpty(keysText.text))
        {
            int.TryParse(keysText.text, out oldValue);
        }


        if (keysText != null)
            keysText.text = value.ToString();


        if (keyCollectSound != null && value > oldValue)
        {

            PlaySound(keyCollectSound);
        }
    }


    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {

            return;
        }


        if (AudioManager.Instance != null && AudioManager.Instance.IsMuted())
        {

            return;
        }




        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);

            return;
        }


        if (GameAudioController.Instance != null)
        {


            GameAudioController.Instance.PlayNewMusic(clip);

            return;
        }



        AudioSource tempAudio = gameObject.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.volume = 0.7f;
        tempAudio.Play();


        Destroy(tempAudio, clip.length + 0.1f);
    }
}