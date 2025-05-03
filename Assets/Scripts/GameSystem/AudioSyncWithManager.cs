using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSyncWithManager : MonoBehaviour
{
    private AudioListener audioListener;
    private string objectName;
    private string sceneName;

    void Awake()
    {
        objectName = gameObject.name;
        sceneName = SceneManager.GetActiveScene().name;



        audioListener = GetComponent<AudioListener>();
        if (audioListener == null)
        {

        }
        else
        {

        }
    }

    void Start()
    {



        if (AudioManager.Instance != null)
        {


            if (audioListener != null)
            {

                bool isMuted = AudioManager.Instance.IsMuted();



                audioListener.enabled = !isMuted;

            }
            else
            {

            }
        }
       
    }

    void OnEnable()
    {



        if (audioListener != null && AudioManager.Instance != null)
        {
            bool isMuted = AudioManager.Instance.IsMuted();
            audioListener.enabled = !isMuted;

        }
    }

}
