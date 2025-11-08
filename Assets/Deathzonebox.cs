using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // ไม่ต้องใช้ตัวแปร respawnPoint แล้ว เพราะเราจะไปใช้ของ PlayerHealth แทน
    // public Transform respawnPoint; 

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าสิ่งที่ตกลงมาคือ Player หรือไม่
        if (other.CompareTag("Player"))
        {
            // ดึงสคริปต์ PlayerHealth จากตัว Player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // สั่งให้ PlayerHealth ทำงานฟังก์ชัน Respawn() ทันที
                // ซึ่งฟังก์ชันนี้จะจัดการทั้งการย้ายตำแหน่ง, รีเซ็ตเลือด, และรีเซ็ตศัตรูให้เองครบจบในที่เดียว
                playerHealth.Respawn();
            }
            else
            {
                Debug.LogWarning("DeathZone: ไม่พบสคริปต์ PlayerHealth บนตัว Player!");
            }
        }
        // ถ้าเป็นอย่างอื่นที่ไม่ใช่ Player ตกลงมา ให้ทำลายทิ้ง
        else
        {
            Destroy(other.gameObject);
        }
    }
}