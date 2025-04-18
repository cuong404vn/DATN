using UnityEngine;
using TMPro;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float defaultMessageDuration = 2f;

    private Coroutine activeMessage;

    private void Awake()
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

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    public void ShowMessage(string message, float duration = 0f)
    {
        if (duration <= 0f)
        {
            duration = defaultMessageDuration;
        }

        if (activeMessage != null)
        {
            StopCoroutine(activeMessage);
        }

        activeMessage = StartCoroutine(DisplayMessage(message, duration));
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);

            yield return new WaitForSeconds(duration);

            messagePanel.SetActive(false);
        }

        activeMessage = null;
    }
}