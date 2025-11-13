using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private int damageToDeal;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, 5f);
    }

    public void Initialize(Vector3 force, int damage)
    {
        damageToDeal = damage;
        rb.AddForce(force, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        // --- ⬇️ (แก้ไขส่วนนี้) ⬇️ ---

        // 1. (ป้องกัน) ถ้าชนตัวเอง (Player) ให้เมินไปก่อน
        if (collision.gameObject.CompareTag("Player"))
        {
            hasHit = false; // รีเซ็ตค่า hasHit เผื่อกระสุนแฉลบ
            return; // ไม่ต้องทำอะไร
        }

        // 2. เช็กว่าเป็น "ศัตรูตีใกล้" หรือไม่
        EnemyAI meleeEnemy = collision.gameObject.GetComponent<EnemyAI>();
        if (meleeEnemy != null)
        {
            Debug.Log("Bullet hit a Melee Enemy!");
            meleeEnemy.TakeDamage(damageToDeal);
            Destroy(gameObject); // ทำลายกระสุน
            return; // จบการทำงาน
        }

        // 3. (เพิ่ม) ถ้าไม่ใช่ตีใกล้, ลองเช็กว่าเป็น "ศัตรูยิงไกล" หรือไม่
        EnemyAI_Ranged rangedEnemy = collision.gameObject.GetComponent<EnemyAI_Ranged>();
        if (rangedEnemy != null)
        {
            Debug.Log("Bullet hit a Ranged Enemy!");
            rangedEnemy.TakeDamage(damageToDeal);
            Destroy(gameObject); // ทำลายกระสุน
            return; // จบการทำงาน
        }

        // --- ⬆️ (สิ้นสุดส่วนแก้ไข) ⬆️ ---

        // 4. ถ้าไม่ใช่ทั้ง Player, ตีใกล้, ยิงไกล (เช่น ชนกำแพง)
        Debug.Log("Bullet hit a wall or something else.");
        Destroy(gameObject); // ทำลายกระสุน
    }
}