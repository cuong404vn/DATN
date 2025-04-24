using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private GameObject dripstone; 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DripstoneTrap trap = dripstone.GetComponent<DripstoneTrap>();
            if (trap != null)
            {
                trap.ActivateTrap();
            }
            Destroy(gameObject);
        }
    }
}