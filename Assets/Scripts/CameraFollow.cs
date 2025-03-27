using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  
    public Vector3 offset;    
    public float smoothSpeed = 5f; 

    void LateUpdate()
    {
        if (player == null) 
        {
            FindPlayer();
            return;
        }

        
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void FindPlayer() 
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        
        }
        else
        {
        
        }
    }
}
