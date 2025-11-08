using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Setting")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    // การอ้างอิง Component อื่นๆ
    private CharacterController controller;
    // เก็บตำแหน่ง Checkpoint ล่าสุด
    private Vector3 currentRespawnPosition;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        controller = GetComponent<CharacterController>();
        if (controller == null) Debug.LogError("CharacterController component not found on Player!");

        // กำหนดจุดเกิดเริ่มต้นเป็นตำแหน่งที่ยืนอยู่ตอนเริ่มเกม
        currentRespawnPosition = transform.position;
    }

    // ฟังก์ชันรับดาเมจ
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // ถ้าตายแล้ว ไม่ต้องรับดาเมจเพิ่ม

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // อัปเดตหลอดเลือด
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // ฟังก์ชันตาย (พลังหมด)
    private void Die()
    {
        Debug.Log("Player has died due to lack of health!");
        Respawn(); // เรียกฟังก์ชันเกิดใหม่
    }

    // --- ⬇️ ระบบเกิดใหม่ (Respawn System) ⬇️ ---

    // ฟังก์ชันสำหรับ Checkpoint เรียกใช้เพื่ออัปเดตจุดเกิด
    public void SetRespawnPoint(Vector3 newPosition)
    {
        currentRespawnPosition = newPosition;
        Debug.Log("Checkpoint updated to: " + newPosition);
    }

    // ฟังก์ชันกลางสำหรับการเกิดใหม่ (เรียกได้ทั้งจาก Die() และ DeathZone)
    public void Respawn()
    {
        Debug.Log("Respawning Player...");

        // 1. ย้ายตำแหน่ง Player (ต้องปิด CharacterController ก่อนชั่วคราว)
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = currentRespawnPosition; // ย้ายไปที่จุด Checkpoint ล่าสุด
            controller.enabled = true;
        }
        else
        {
            // กรณีไม่ได้ใช้ CharacterController (เผื่อไว้)
            transform.position = currentRespawnPosition;
        }

        // 2. รีเซ็ตพลังชีวิต
        currentHealth = maxHealth;
        UpdateHealthUI();

        // 3. รีเซ็ตศัตรูทั้งหมดในฉาก
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