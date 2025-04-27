using UnityEngine;
using TMPro;
using System.Collections;

public class Chest : MonoBehaviour
{
    public int keysRequired = 1;
    public GameObject interactionPrompt;
    public AudioClip openSound;
    public AudioClip lockedSound;
    public Animator animator;

    [Header("Tích hợp UI")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    private bool playerInRange = false;
    private bool isOpen = false;
    private ItemDrop itemDrop;
    private AudioSource audioSource;
    private Coroutine activeMessageCoroutine;

    void Start()
    {
        itemDrop = GetComponent<ItemDrop>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenChest();
        }
    }

    void TryOpenChest()
    {
        if (GameManager.Instance.keys >= keysRequired)
        {

            GameManager.Instance.AddKeys(-keysRequired);
            OpenChest();
        }
        else
        {

            string message = "Need " + keysRequired + " key to open chest!";



            ShowMessage(message);

            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
    }

    void OpenChest()
    {
        isOpen = true;


        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }


        if (animator != null)
        {
            animator.SetTrigger("Open");


            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            Invoke(nameof(RemoveChest), animationLength);
        }
        else
        {

            RemoveChest();
        }


        if (itemDrop != null)
        {
            itemDrop.DropItem();
        }


        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void RemoveChest()
    {

        gameObject.SetActive(false);

    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }


    public void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {

            if (activeMessageCoroutine != null)
            {
                StopCoroutine(activeMessageCoroutine);
            }


            activeMessageCoroutine = StartCoroutine(DisplayMessage(message, messageDuration));
        }
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        if (messagePanel != null && messageText != null)
        {

            messageText.text = message;
            messagePanel.SetActive(true);


            yield return new WaitForSecondsRealtime(duration);


            messagePanel.SetActive(false);
        }

        activeMessageCoroutine = null;
    }
}