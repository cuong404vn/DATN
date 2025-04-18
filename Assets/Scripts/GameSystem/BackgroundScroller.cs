using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform player; // Kéo Player vào đây trong Inspector
    private Vector3 offset; // Khoảng cách giữa Background và Player

    void Start()
    {
        // Lưu khoảng cách ban đầu giữa Background và Player
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        // Di chuyển Background theo Player, giữ nguyên khoảng cách ban đầu
        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, transform.position.z);
    }
}
