using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab của chưởng
    public Transform firePoint; // Điểm xuất phát của chưởng
    public float projectileSpeed = 10f; // Tốc độ chưởng
    public float fireRate = 2f; // Tần suất bắn (giây/lần)

    private float nextFireTime;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Tạo chưởng tại firePoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Lấy Rigidbody2D và thiết lập vận tốc để bắn thẳng
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = firePoint.right * projectileSpeed; // Bắn theo hướng firePoint (phải)

        // Hủy chưởng sau 5 giây
        Destroy(projectile, 5f);
    }
}