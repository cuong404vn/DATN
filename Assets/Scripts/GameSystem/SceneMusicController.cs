using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    private AudioSource music;
    private string controllerName;
    private string sceneName;

    void Awake()
    {
        controllerName = gameObject.name;
        sceneName = SceneManager.GetActiveScene().name;



        music = GetComponent<AudioSource>();
        if (music == null)
        {

        }
    }

    void Start()
    {


        if (music == null)
        {

            return;
        }


        if (AudioManager.Instance == null)
        {

            music.Play(); 

            return;
        }


        bool isMuted = AudioManager.Instance.IsMuted();
        music.mute = isMuted;



        music.Play();

    }


}
