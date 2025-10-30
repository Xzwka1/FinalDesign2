using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Setting")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    // --- ⬇️ (แก้ไข) เปลี่ยนชื่อคลาสที่อ้างอิง ⬇️ ---
    private SimplePlayerMovement playerMove;
    private CharacterController controller; // ❗️ (เพิ่ม) เราต้องใช้ Controller ด้วย
    private Vector3 respawnPoint; // ❗️ (เพิ่ม) เก็บจุดเกิด

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // --- ⬇️ (แก้ไข) GetComponent ให้ครบ ⬇️ ---
        playerMove = GetComponent<SimplePlayerMovement>();
        controller = GetComponent<CharacterController>(); // ❗️ (เพิ่ม)
        respawnPoint = transform.position; // ❗️ (เพิ่ม) บันทึกจุดเกิด

        if (playerMove == null) Debug.LogError("SimplePlayerMovement script not found!");
        if (controller == null) Debug.LogError("CharacterController component not found!");
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // ถ้าตายแล้ว ไม่ต้องรับดาเมจซ้ำ

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // --- ⬇️ (เพิ่ม) Log ที่คุณต้องการ ⬇️ ---
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        UpdateHealthUI();
        if (currentHealth <= 0) Die();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        if (playerMove != null && controller != null)
        {
            // --- ⬇️ (แก้ไข) เรียก Respawn ให้ถูกรูปแบบ ⬇️ ---
            playerMove.Respawn(respawnPoint, controller);
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}