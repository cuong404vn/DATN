using UnityEngine;

public class ExitGate : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.2f;

    private Vector3 originalScale;
    private LevelManager levelManager;

    void Start()
    {
        originalScale = transform.localScale;
        levelManager = FindAnyObjectByType<LevelManager>();
    }

    void Update()
    {
   
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

      
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
          
            if (levelManager != null)
            {
                levelManager.CompleteLevel();
            }
        }
    }
}