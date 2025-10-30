using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // ... (ตัวแปรอื่น ๆ ) ...

    // 1. แก้ไขบรรทัดนี้: เปลี่ยน PlayerMove -> SimplePlayerMovement
    private SimplePlayerMovement playerMove;

    void Start()
    {
        // ... (โค้ดอื่น ๆ ) ...

        // 2. แก้ไขบรรทัดนี้: เปลี่ยน PlayerMove -> SimplePlayerMovement
        playerMove = GetComponent<SimplePlayerMovement>();
        if (playerMove == null)
        {
            Debug.LogError("SimplePlayerMovement script not found on Player!");
        }
    }

    // ... (TakeDamage, UpdateHealthUI) ...

    private void Die()
    {
        Debug.Log("Player has died!");

        if (playerMove != null)
        {
            // 3. บรรทัดนี้จะหายแดง เพราะตอนนี้ playerMove ถูกระบุ type เป็น SimplePlayerMovement แล้ว
            playerMove.Respawn();
        }
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}