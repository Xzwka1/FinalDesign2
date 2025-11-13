using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // สร้าง 2 ลิสต์แยกสำหรับศัตรูแต่ละประเภท
    public List<EnemyAI> meleeEnemies = new List<EnemyAI>();
    public List<EnemyAI_Ranged> rangedEnemies = new List<EnemyAI_Ranged>();

    // --- ⬇️ (เพิ่มส่วนนี้) ⬇️ ---
    private int totalEnemyCount = 0;
    private int enemiesKilledCount = 0;
    // --- ⬆️ (สิ้นสุดส่วนที่เพิ่ม) ⬆️ ---


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. ค้นหาและเก็บศัตรูประเภท "ตีใกล้"
        EnemyAI[] meleeFound = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        meleeEnemies.AddRange(meleeFound);

        // 2. ค้นหาและเก็บศัตรูประเภท "ยิงไกล"
        EnemyAI_Ranged[] rangedFound = FindObjectsByType<EnemyAI_Ranged>(FindObjectsSortMode.None);
        rangedEnemies.AddRange(rangedFound);

        // --- ⬇️ (เพิ่มส่วนนี้) ⬇️ ---
        // นับจำนวนศัตรูทั้งหมดตอนเริ่มเกม
        totalEnemyCount = meleeEnemies.Count + rangedEnemies.Count;
        enemiesKilledCount = 0; // เริ่มต้นที่ 0
        Debug.Log($"GameManager: พบศัตรูทั้งหมด {totalEnemyCount} ตัว");
        // --- ⬆️ (สิ้นสุดส่วนที่เพิ่ม) ⬆️ ---
    }

    public void ResetAllEnemies()
    {
        // --- ⬇️ (เพิ่ม) ⬇️ ---
        enemiesKilledCount = 0; // ❗️ สำคัญ: รีเซ็ตตัวนับเมื่อเกิดใหม่
        Debug.Log("GameManager: รีเซ็ตจำนวนศัตรูที่ถูกฆ่าเป็น 0");
        // --- ⬆️ (สิ้นสุด) ⬆️ ---

        Debug.Log("GameManager: กำลังสั่งรีเซ็ตศัตรูทั้งหมด...");

        // วนลูปสั่งรีเซ็ตพวกตีใกล้
        foreach (EnemyAI enemy in meleeEnemies)
        {
            if (enemy != null)
            {
                enemy.ResetEnemy();
            }
        }

        // วนลูปสั่งรีเซ็ตพวกยิงไกล
        foreach (EnemyAI_Ranged rangedEnemy in rangedEnemies)
        {
            if (rangedEnemy != null)
            {
                rangedEnemy.ResetEnemy();
            }
        }
    }

    // --- ⬇️ (เพิ่มฟังก์ชันใหม่ 2 ฟังก์ชันนี้) ⬇️ ---

    /// <summary>
    /// ถูกเรียกโดยศัตรู เมื่อมันตาย
    /// </summary>
    public void ReportEnemyKilled()
    {
        enemiesKilledCount++;
        Debug.Log($"GameManager: ศัตรูตาย! จำนวนที่ฆ่า: {enemiesKilledCount}/{totalEnemyCount}");
    }

    /// <summary>
    /// ถูกเรียกโดย Teleporter เพื่อเช็กว่าตายหมดหรือยัง
    /// </summary>
    /// <returns>จริง (True) ถ้าฆ่าหมดแล้ว</returns>
    public bool AreAllEnemiesDefeated()
    {
        // ถ้าจำนวนที่ฆ่า >= จำนวนทั้งหมด (และมีศัตรูในฉากอย่างน้อย 1 ตัว)
        return (enemiesKilledCount >= totalEnemyCount) && (totalEnemyCount > 0);
    }
    // --- ⬆️ (สิ้นสุดส่วนที่เพิ่ม) ⬆️ ---
}