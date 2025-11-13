using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // สร้าง 2 ลิสต์แยกสำหรับศัตรูแต่ละประเภท
    public List<EnemyAI> meleeEnemies = new List<EnemyAI>();
    public List<EnemyAI_Ranged> rangedEnemies = new List<EnemyAI_Ranged>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ค้นหาศัตรู "ทุกตัว" ที่ Active อยู่ในฉากตอนเริ่มเกม

        // 1. ค้นหาและเก็บศัตรูประเภท "ตีใกล้"
        // (ใช้ FindObjectsByType ดีกว่าแบบเก่า)
        EnemyAI[] meleeFound = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        meleeEnemies.AddRange(meleeFound);

        // 2. ค้นหาและเก็บศัตรูประเภท "ยิงไกล"
        EnemyAI_Ranged[] rangedFound = FindObjectsByType<EnemyAI_Ranged>(FindObjectsSortMode.None);
        rangedEnemies.AddRange(rangedFound);

        Debug.Log($"GameManager: พบศัตรูตีใกล้ {meleeEnemies.Count} ตัว, ยิงไกล {rangedEnemies.Count} ตัว");
    }

    public void ResetAllEnemies()
    {
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
}