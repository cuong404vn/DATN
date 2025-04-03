using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int healAmount = 1; // Số lượng máu hồi phục khi sử dụng
    public int price = 10; // Giá mua bình HP bằng coin

    // Sprite và hiệu ứng
    public Sprite potionSprite;
    public AudioClip useSound;

    // Phương thức này sẽ được gọi khi người chơi nhặt bình máu
    public void Collect(PlayerHealth playerHealth)
    {
        if (playerHealth != null)
        {
            bool added = playerHealth.AddHealthPotion(1);
            if (added)
            {
                // Phát âm thanh nhặt item nếu có
                if (useSound != null)
                {
                    AudioSource.PlayClipAtPoint(useSound, transform.position, 0.7f);
                }

                // Hủy gameObject sau khi nhặt
                Destroy(gameObject);
            }
        }
    }
}