using UnityEngine;

public class Chest : MonoBehaviour
{
    public int keysRequired = 1;
    public GameObject interactionPrompt;
    public AudioClip openSound;
    public AudioClip lockedSound;
    public Animator animator; // Optional: if you want chest opening animation

    private bool playerInRange = false;
    private bool isOpen = false;
    private ItemDrop itemDrop;
    private AudioSource audioSource;

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
            // Successful opening
            GameManager.Instance.AddKeys(-keysRequired);
            OpenChest();
        }
        else
        {
            // Not enough keys
            string message = "Cần " + keysRequired + " chìa khóa để mở rương này!";
            Debug.Log(message);

            // Show message to player if MessageManager exists
            if (MessageManager.Instance != null)
            {
                MessageManager.Instance.ShowMessage(message);
            }

            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
        }
    }

    void OpenChest()
    {
        isOpen = true;

        // Play sound
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        // Drop items
        if (itemDrop != null)
        {
            itemDrop.DropItem();
        }

        // Hide prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
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
}