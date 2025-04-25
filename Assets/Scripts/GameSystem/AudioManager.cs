using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager Instance;

    private bool isMuted;

    void Awake()
    {
       
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            
        }
        else
        {
            Destroy(gameObject); 
            
        }

       
        isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;
       
    }

    
    public void ToggleSound()
    {
        isMuted = !isMuted; 
        PlayerPrefs.SetInt("AudioMuted", isMuted ? 1 : 0); 
        PlayerPrefs.Save(); 
        Debug.Log("AudioManager toggle sound, isMuted = " + isMuted);
    }

   
    public bool IsMuted()
    {
        return isMuted;
    }
}
