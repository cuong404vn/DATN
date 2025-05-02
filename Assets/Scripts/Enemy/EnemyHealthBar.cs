using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0.5f, 0); 

    public Image fillImage;
    public Text nameText; 

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

   
        transform.position = target.position + offset;

     
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    }

    public void SetHealth(int current, int max)
    {
        if (fillImage != null)
            fillImage.fillAmount = (float)current / max;
    }

    public void SetName(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }
}
