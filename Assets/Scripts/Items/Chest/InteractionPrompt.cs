using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultText = "E";

    private void Awake()
    {
        if (promptText == null)
        {
            promptText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (promptText != null)
        {
            promptText.text = defaultText;
        }
    }

    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
}