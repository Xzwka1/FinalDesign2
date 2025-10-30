using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab; // ❗️(สำคัญ) ลาก Prefab กระสุนมาใส่
    [SerializeField] private Transform firePoint;     // ❗️(สำคัญ) ลากจุดยิง (Empty Object) มาใส่
    [SerializeField] private float fireRate = 0.5f;   // ยิงได้ทุก 0.5 วินาที
    [SerializeField] private float bulletForce = 20f; // ความแรงของกระสุน
    [SerializeField] private int bulletDamage = 25;
    private float nextFireTime = 0f; // ตัวนับเวลา

    void Update()
    {
        // 1. ตรวจสอบว่าถึงเวลายิงรึยัง (Time.time > nextFireTime)
        // 2. ตรวจสอบว่ากดคลิกซ้ายหรือไม่ (Input.GetButton("Fire1"))
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            // รีเซ็ตตัวนับเวลา
            nextFireTime = Time.time + fireRate;

            // เรียกฟังก์ชันยิง
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. สร้างกระสุน (Instantiate) จาก Prefab ณ ตำแหน่งและมุมของ firePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. หาส่วนประกอบ Rigidbody จากกระสุนที่เพิ่งสร้าง
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // 3. (ทางเลือก A) ถ้ากระสุนของคุณไม่มีสคริปต์ Bullet.cs
        //    ให้ใช้โค้ดนี้เพื่อยิง (แต่ผมแนะนำวิธี B มากกว่า)
        // rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

        // 3. (ทางเลือก B - แนะนำ)
        //    ให้สคริปต์ Bullet.cs จัดการตัวเอง
        //    เราแค่ส่งค่าความแรงและดาเมจไปให้มัน (ถ้าต้องการ)
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.forward * bulletForce, bulletDamage);
        }
    }
}