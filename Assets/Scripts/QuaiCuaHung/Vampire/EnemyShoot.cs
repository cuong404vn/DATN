using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public GameObject projectilePrefab; 
    public Transform firePoint; 
    public float projectileSpeed = 10f; 
    public float fireRate = 2f;

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
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = firePoint.right * projectileSpeed; 

        Destroy(projectile, 5f);
    }
}