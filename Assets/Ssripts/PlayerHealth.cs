using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f; // หน่วงเวลาก่อนเกิด
    private Vector3 respawnPoint; // จุดเกิด

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("References")]
    private SimplePlayerMovement playerMove; // ❗️ อ้างอิงสคริปต์ Movement ใหม่
    private CharacterController controller;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;

        // ❗️ อ้างอิงสคริปต์ Movement ใหม่
        playerMove = GetComponent<SimplePlayerMovement>();
        controller = GetComponent<CharacterController>();

        respawnPoint = transform.position;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;

        // --- ⬇️ นี่คือ Log ที่คุณต้องการ (เพิ่มบรรทัดนี้) ⬇️ ---
        Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}");
        // --- ⬆️ จบส่วนที่เพิ่ม ⬆️ ---

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ... (ฟังก์ชัน Heal, UpdateHealthUI เหมือนเดิม) ...
    public void Heal(int healAmount) { /* ... (โค้ดเดิม) ... */ }
    private void UpdateHealthUI() { /* ... (โค้ดเดิม) ... */ }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player has died.");

        if (playerMove != null)
        {
            playerMove.enabled = false; // ปิดสคริปต์ Movement
        }

        Invoke(nameof(StartRespawn), respawnDelay);
    }

    private void StartRespawn()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        isDead = false;

        if (playerMove != null)
        {
            playerMove.enabled = true; // เปิดสคริปต์ Movement
        }

        // เรียกฟังก์ชัน Respawn ที่เรา "กำลังจะสร้าง" ใน SimplePlayerMovement
        if (playerMove != null && controller != null)
        {
            // ❗️ เราจะสร้างฟังก์ชัน Respawn นี้ในขั้นตอนต่อไป
            playerMove.Respawn(respawnPoint, controller);
        }
        else
        {
            Debug.LogError("PlayerMove or Controller reference is missing! Cannot respawn.");
        }

        Debug.Log("Player has respawned.");
    }

    // ... (ฟังก์ชัน Update ทดสอบปุ่ม T เหมือนเดิม) ...
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(20);
        }
    }
}