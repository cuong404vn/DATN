using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform player;
    private Vector3 offset; 

    void Start()
    {

        offset = transform.position - player.position;
    }

    void LateUpdate()
    {

        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
    }
}
