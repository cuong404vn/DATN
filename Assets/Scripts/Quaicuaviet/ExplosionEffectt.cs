using UnityEngine;

public class ExplosionEffectt : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(gameObject, ps.main.duration); // Hủy sau khi hiệu ứng kết thúc
        }
    }
}