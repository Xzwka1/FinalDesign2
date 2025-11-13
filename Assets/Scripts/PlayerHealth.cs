using UnityEngine;
using UnityEngine.UI; // ต้องมีสำหรับ UI components

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Setting")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    // public Slider healthSlider; // ❗️ ลบอันนี้ทิ้ง หรือ คอมเมนต์ไว้
    public Image hpFillImage; // ❗️ เพิ่ม: อ้างอิงถึง Image ที่เป็นหลอดเลือดสีแดง (HP_Fill)

    // --- ส่วนประกอบที่จำเป็น ---
    private CharacterController controller;

    // --- ระบบ Respawn ---
    private Vector3 currentRespawnPosition;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(); // ❗️ อัปเดต UI ตอนเริ่มเกม

        controller = GetComponent<CharacterController>();
        if (controller == null) Debug.LogError("PlayerHealth: ไม่พบ CharacterController!");

        currentRespawnPosition = transform.position;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0; // กันเลือดติดลบ

        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        UpdateHealthUI(); // ❗️ อัปเดต UI ทุกครั้งที่รับดาเมจ

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ฟังก์ชันอัปเดต UI
    void UpdateHealthUI()
    {
        if (hpFillImage != null)
        {
            // คำนวณค่า Fill Amount (0.0 ถึง 1.0)
            hpFillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        Respawn();
    }

    public void Respawn()
    {
        Debug.Log("Player Respawning...");

        if (controller != null)
        {
            controller.enabled = false;
            transform.position = currentRespawnPosition;
            controller.enabled = true;
        }
        else
        {
            transform.position = currentRespawnPosition;
        }

        currentHealth = maxHealth;
        UpdateHealthUI(); // ❗️ อัปเดต UI ให้เลือดเต็มหลังเกิดใหม่

        if (GameManager.instance != null)
        {
            GameManager.instance.ResetAllEnemies();
        }
    }

    public void SetRespawnPoint(Vector3 newPosition)
    {
        currentRespawnPosition = newPosition;
        Debug.Log("Checkpoint Set!");
    }
}