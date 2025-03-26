using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI keysText;

    void Start()
    {
        
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
        if (coinsText != null)
            coinsText.text = value.ToString();
        else
            Debug.LogWarning("CoinsText is null!");
    }

    void UpdateKeysUI(int value)
    {
        if (keysText != null)
            keysText.text = value.ToString();
        else
            Debug.LogWarning("KeysText is null!");
    }
}