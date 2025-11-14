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

    // --- ⬇️ (อัปเดตส่วนนี้) ⬇️ ---
    [Header("Audio")]
    [Tooltip("ลากไฟล์เสียงยิงเวท (MP3/WAV) มาใส่")]
    public AudioClip magicShootSound;

    [Tooltip("ความดังของเสียงยิง (0.0 = เบา, 1.0 = ดังสุด)")]
    [Range(0.0f, 1.0f)] // ❗️ (เพิ่ม) ทำให้เป็นสไลเดอร์ 0-1
    public float shootVolume = 1.0f; // ❗️ (เพิ่ม) ตัวแปรปรับความดัง (ค่าเริ่มต้น 1 คือดังสุด)

    private AudioSource audioSource;
    // --- ⬆️ (สิ้นสุดส่วนอัปเดต) ⬆️ ---


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("PlayerShoot: ไม่พบ AudioSource, กำลังเพิ่ม Component...");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime && !PauseMenu.GameIsPaused)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. สร้างกระสุน
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. หาสคริปต์ Bullet.cs
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // 3. ส่งค่าความแรงและดาเมจ
            bulletScript.Initialize(firePoint.forward * bulletForce, bulletDamage);
        }

        // --- ⬇️ (แก้ไขส่วนนี้) ⬇️ ---
        // สั่งเล่นเสียง โดยส่ง "ความดัง (Volume)" ที่เราตั้งค่าไว้เข้าไปด้วย
        if (magicShootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicShootSound, shootVolume);
        }
        // --- ⬆️ (สิ้นสุดส่วนแก้ไข) ⬆️ ---
    }
}