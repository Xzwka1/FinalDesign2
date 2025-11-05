using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private int damageToDeal; // ดาเมจที่จะส่งให้ Enemy

    private bool hasHit = false; // ตัวแปรป้องกันการชนซ้ำ
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // (ทางเลือก) ทำลายตัวเองทิ้ง ถ้าลอยไปนานเกิน 5 วินาที (กันรก Scene)
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// ฟังก์ชันนี้จะถูกเรียกโดย PlayerShoot.cs
    /// </summary>
    public void Initialize(Vector3 force, int damage)
    {
        damageToDeal = damage;

        // ใช้ ForceMode.Impulse เพื่อให้แรงกระแทกทันที
        rb.AddForce(force, ForceMode.Impulse);
    }

    /// <summary>
    /// ทำงานเมื่อกระสุนชนกับอะไรบางอย่าง
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // 1. ป้องกันการชนซ้ำ
        if (hasHit) return;
        hasHit = true;

        // --- (ส่วนที่แก้ไข) ---

        // 2. พยายามดึงสคริปต์ "EnemyAI" (ตัวธรรมดา/ประชิด)
        EnemyAI enemy_Melee = collision.gameObject.GetComponent<EnemyAI>();

        // 3. พยายามดึงสคริปต์ "EnemyAI_Ranged" (ตัวยิงไกล)
        EnemyAI_Ranged enemy_Ranged = collision.gameObject.GetComponent<EnemyAI_Ranged>();

        // 4. ตรวจสอบว่าเจอตัวไหน
        if (enemy_Melee != null)
        {
            // 4a. ถ้าเจอตัวธรรมดา
            Debug.Log("Bullet hit Melee Enemy!");

            // (สำคัญ: ต้องมั่นใจว่าสคริปต์ EnemyAI.cs ของคุณ
            //         ก็มีฟังก์ชัน public void TakeDamage(int damage) เหมือนกัน)
            enemy_Melee.TakeDamage(damageToDeal);
        }
        else if (enemy_Ranged != null)
        {
            // 4b. ถ้าเจอตัวยิงไกล
            Debug.Log("Bullet hit Ranged Enemy!");
            enemy_Ranged.TakeDamage(damageToDeal);
        }
        else
        {
            // 4c. ถ้าไม่เจอทั้งคู่ (ชนกำแพง, พื้น ฯลฯ)
            Debug.Log("Bullet hit a wall or something else.");
        }

        // --- (จบส่วนแก้ไข) ---

        // 5. ไม่ว่าจะชนอะไรก็ตาม ให้ทำลายกระสุนทิ้ง
        Destroy(gameObject);
    }
}