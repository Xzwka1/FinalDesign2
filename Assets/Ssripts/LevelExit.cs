using System.Collections.Generic; // ❗️ ต้องมี เพื่อใช้ List<>
using TMPro; // (ทางเลือก: ถ้าคุณอยากโชว์ข้อความเตือน)
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("ลาก Panel (หน้าต่าง) UI ที่จะโชว์ตอนชนะมาใส่")]
    public GameObject winScreenPanel;

    [Header("Warning Message (Optional)")]
    [Tooltip("ลาก Text (TMP) ที่จะโชว์คำเตือน 'ฆ่าศัตรูให้หมด' มาใส่")]
    public TextMeshProUGUI warningText;
    public float warningDisplayTime = 2f;

    // --- ส่วนนับศัตรู ---
    private List<EnemyAI> enemiesInLevel; // List สำหรับ "จำ" ศัตรูทั้งหมด
    private bool allEnemiesDefeated = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        // 1. ซ่อน UI ตอนเริ่ม
        if (winScreenPanel != null) winScreenPanel.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);

        // 2. ค้นหาและ "จำ" ศัตรูทั้งหมดในด่านตอนเริ่มเกม
        // 2. ค้นหาและ "จำ" ศัตรูทั้งหมดในด่านตอนเริ่มเกม
        enemiesInLevel = new List<EnemyAI>(FindObjectsByType<EnemyAI>(FindObjectsSortMode.None)); // ✅ แก้เป็นบรรทัดนี้
        Debug.Log($"Level started with {enemiesInLevel.Count} enemies.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. เช็คว่าเป็น Player หรือไม่
        if (allEnemiesDefeated || !other.CompareTag("Player")) return;

        // 2. ตรวจสอบเงื่อนไข (ว่าศัตรูตายหมดหรือยัง)
        CheckWinCondition();

        if (allEnemiesDefeated)
        {
            // --- ชนะแล้ว ---
            Debug.Log("LEVEL COMPLETE!");
            ShowWinScreen(other.GetComponent<SimplePlayerMovement>());
        }
        else
        {
            // --- ยังไม่ชนะ ---
            Debug.Log("Player reached exit, but enemies remain.");
            if (warningText != null)
            {
                StartCoroutine(ShowWarning());
            }
        }
    }

    /// <summary>
    /// วนลูปเช็ค List ศัตรูที่ "จำ" ไว้ ว่าถูก Destroy (เป็น null) หมดหรือยัง
    /// </summary>
    private void CheckWinCondition()
    {
        int enemiesAlive = 0;
        foreach (EnemyAI enemy in enemiesInLevel)
        {
            // ถ้า "enemy" ยังไม่เป็น "null" 
            // (หมายความว่า GameObject นั้นยังไม่ถูก Destroy)
            if (enemy != null)
            {
                enemiesAlive++; // นับว่ายังเหลือ
            }
        }

        Debug.Log($"Checking win condition... Enemies remaining: {enemiesAlive}");

        // ถ้าไม่เหลือศัตรู (เป็น 0)
        if (enemiesAlive == 0)
        {
            allEnemiesDefeated = true;
        }
    }


    // ฟังก์ชันโชว์หน้าจอ "ชนะ"
    private void ShowWinScreen(SimplePlayerMovement playerScript)
    {
        if (winScreenPanel != null) winScreenPanel.SetActive(true);

        Time.timeScale = 0f; // หยุดเวลา
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ปิดสคริปต์ Player
        if (playerScript != null) playerScript.enabled = false;
    }

    // (ทางเลือก) ฟังก์ชันโชว์คำเตือน
    private IEnumerator ShowWarning()
    {
        warningText.gameObject.SetActive(true);
        warningText.text = "You must kill all enemies first!";
        yield return new WaitForSeconds(warningDisplayTime);
        warningText.gameObject.SetActive(false);
    }
}