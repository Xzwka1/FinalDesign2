using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    private int damageToDeal; // เปลี่ยนเป็น private

    [Header("Lifetime")]
    public float lifetime = 5f;

    // ❗️❗️ เพิ่มฟังก์ชันนี้ลงไปใน EnemyBullet.cs ครับ ❗️❗️
    public void InitializeBullet(int damageAmount)
    {
        damageToDeal = damageAmount;
    }
    // -----------------------------------------------------

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // ใช้ตัวแปร damageToDeal ที่รับค่ามาแล้ว
                playerHealth.TakeDamage(damageToDeal);
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}