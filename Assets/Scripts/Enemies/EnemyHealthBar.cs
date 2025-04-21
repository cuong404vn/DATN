using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private Slider slider;
    private Text nameText;
    private Camera mainCam;

    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        nameText = GetComponentInChildren<Text>();
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = mainCam.transform.rotation;
        }
    }

    public void SetHealth(int current, int max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }
    }

    public void SetName(string enemyName)
    {
        if (nameText != null)
            nameText.text = enemyName;
    }
}
