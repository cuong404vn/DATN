using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Nhân vật cần theo dõi
    public Vector3 offset;    // Khoảng cách giữa camera và player
    public float smoothSpeed = 5f; // Tốc độ di chuyển mượt

    void LateUpdate()
    {
        if (player == null) // Nếu chưa có Player, tìm lại
        {
            FindPlayer();
            return;
        }

        // Di chuyển camera theo player với hiệu ứng mượt
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    void FindPlayer()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
            Debug.Log("Camera đã tìm thấy Player!");
        }
        else
        {
            Debug.LogError("Không tìm thấy Player! Kiểm tra lại Tag của Player.");
        }
    }
}
