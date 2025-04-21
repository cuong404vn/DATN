using UnityEngine;
using System.Collections;

public class ExitGate : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.2f;
    public float completionDelay = 5f;

    private Vector3 originalScale;
    private LevelManager levelManager;
    private bool playerEntered = false;

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
        if (other.CompareTag("Player") && !playerEntered)
        {
            playerEntered = true;
            StartCoroutine(CompleteWithDelay());
        }
    }

    IEnumerator CompleteWithDelay()
    {
        Debug.Log("Player entered exit gate. Completing level in " + completionDelay + " seconds...");
        yield return new WaitForSeconds(completionDelay);

        if (levelManager != null)
        {
            levelManager.CompleteLevel();
        }
    }
}