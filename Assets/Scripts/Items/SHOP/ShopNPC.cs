using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopNPC : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactionPrompt;
    public GameObject shopPanel;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI playerCoinsText;
    public GameObject maxPotionsMessage;
    public GameObject notEnoughCoinsMessage;

    [Header("Shop Settings")]
    public int healthPotionPrice = 5;
    public float messageDisplayTime = 2f;
    public AudioClip purchaseSound;
    public AudioClip errorSound;

    private bool playerInRange = false;
    private PlayerHealth playerHealth;
    private float messageTimer = 0f;

    void Start()
    {

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);


        HideMessages();
    }

    void Update()
    {

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }


        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }


        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                HideMessages();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerHealth = other.GetComponent<PlayerHealth>();


            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;


            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            CloseShop();
        }
    }

    void ToggleShop()
    {
        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);


            Time.timeScale = !isActive ? 0f : 1f;

            if (playerHealth != null)
            {
                playerHealth.SetShopOpen(!isActive);
            }

            if (!isActive)
            {
                UpdateShopUI();
            }
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);

            // Tiếp tục game khi đóng shop
            Time.timeScale = 1f;

            if (playerHealth != null)
            {
                playerHealth.SetShopOpen(false);
            }


            HideMessages();
        }
    }

    void UpdateShopUI()
    {

        if (priceText != null)
        {
            priceText.text = healthPotionPrice.ToString() + " Coin";
        }


        if (playerCoinsText != null && GameManager.Instance != null)
        {
            playerCoinsText.text = "Coins: " + GameManager.Instance.coins.ToString();
        }
    }

    public void BuyHealthPotion()
    {
        if (playerHealth == null) return;


        UpdateShopUI();


        if (GameManager.Instance.coins >= healthPotionPrice)
        {

            if (playerHealth.AddHealthPotion(1))
            {

                GameManager.Instance.AddCoins(-healthPotionPrice);


                if (purchaseSound != null)
                {
                    AudioSource.PlayClipAtPoint(purchaseSound, transform.position);
                }


                UpdateShopUI();
            }
            else
            {

                ShowMessage(maxPotionsMessage);


                if (errorSound != null)
                {
                    AudioSource.PlayClipAtPoint(errorSound, transform.position);
                }
            }
        }
        else
        {

            ShowMessage(notEnoughCoinsMessage);


            if (errorSound != null)
            {
                AudioSource.PlayClipAtPoint(errorSound, transform.position);
            }
        }
    }

    void ShowMessage(GameObject messageObj)
    {
        if (messageObj != null)
        {

            HideMessages();


            messageObj.SetActive(true);


            messageTimer = messageDisplayTime;
        }
    }

    void HideMessages()
    {
        if (maxPotionsMessage != null)
            maxPotionsMessage.SetActive(false);

        if (notEnoughCoinsMessage != null)
            notEnoughCoinsMessage.SetActive(false);
    }
}
