using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                // ส่งตำแหน่งของตัวเองไปบอก Player ว่า "จำตรงนี้ไว้นะ"
                player.SetRespawnPoint(transform.position);
                // (Optional) ปิด Checkpoint นี้ไปเลยหลังใช้ครั้งแรก
                // gameObject.SetActive(false); 
            }
            if (GameManager.instance != null)
            {
                GameManager.instance.ResetAllEnemies();
            }
            else
            {
                Debug.LogWarning("GameManager instance not found! Enemies won't reset.");
            }
        }
    }
}