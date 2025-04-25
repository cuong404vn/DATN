using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    private AudioSource music;

    void Start()
    {
        music = GetComponent<AudioSource>();
        music.mute = AudioManager.Instance.IsMuted();
        music.Play(); 
    }
}
