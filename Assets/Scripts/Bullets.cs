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
        // (!!!) 1. ตรวจสอบว่าเคยชนไปแล้วหรือยัง
        if (hasHit) return; // ถ้าเคยชนแล้ว (เป็น true) ให้ออกจากฟังก์ชันนี้ทันที

        // (!!!) 2. ตั้งค่าว่า "ชนแล้ว" (กันการชนซ้ำในเฟรมถัดไป)
        hasHit = true;

        // --- 3. ตรวจสอบว่าชน Enemy หรือไม่ ---
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();

        if (enemy != null)
        {
            Debug.Log("Bullet hit an Enemy!");
            enemy.TakeDamage(damageToDeal);
        }
        else
        {
            Debug.Log("Bullet hit a wall or something else.");
        }

        // --- 4. ไม่ว่าจะชนอะไรก็ตาม ให้ทำลายกระสุนทิ้ง ---
        Destroy(gameObject);
    }
}