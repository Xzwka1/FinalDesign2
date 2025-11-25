using UnityEngine;
using UnityEngine.SceneManagement; // ❗️ จำเป็นสำหรับการเปลี่ยนฉาก
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ใส่ชื่อฉาก Main Menu ของคุณให้ตรงเป๊ะๆ")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("จะโชว์หน้าชนะกี่วินาที ก่อนจะตัดเข้าหน้าเมนู")]
    public float delayBeforeExit = 3f;

    [Header("UI References")]
    public GameObject winScreenPanel; // ลากภาพ Win UI มาใส่
    public GameObject warningText;    // ลากข้อความเตือนมาใส่

    private bool levelCompleted = false;

    private void OnTriggerEnter(Collider other)
    {
        // ถ้าจบด่านไปแล้ว หรือไม่ใช่ Player ให้ข้ามไป
        if (levelCompleted || !other.CompareTag("Player")) return;

        // 1. ถาม GameManager ว่าฆ่าหมดหรือยัง?
        if (GameManager.instance != null)
        {
            if (GameManager.instance.AreAllEnemiesDefeated())
            {
                // --- ✅ เงื่อนไขครบ (ชนะ) ---
                StartCoroutine(WinSequence(other.gameObject));
            }
            else
            {
                // --- ❌ ยังฆ่าไม่หมด ---
                Debug.Log("GameManager บอกว่าศัตรูยังไม่หมด!");
                if (warningText != null) StartCoroutine(ShowWarning());
            }
        }
        else
        {
            Debug.LogError("ไม่พบ GameManager ในฉาก! กรุณาวาง GameManager ลงใน Hierarchy");
        }
    }

    // ลำดับการทำงานตอนชนะ
    IEnumerator WinSequence(GameObject player)
    {
        levelCompleted = true;
        Debug.Log("Level Complete!");

        // 1. โชว์หน้าต่างชนะ
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        // 2. (Optional) ปิดการขยับตัวผู้เล่น เพื่อไม่ให้เดินไปเดินมาระหว่างรอ
        // var moveScript = player.GetComponent<SimplePlayerMovement>();
        // if (moveScript != null) moveScript.enabled = false;

        // 3. ปลดล็อคเมาส์ (เผื่อต้องใช้คลิกในหน้าเมนู)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 4. รอเวลาสักพัก ให้คนเล่นดูหน้าชนะ
        yield return new WaitForSeconds(delayBeforeExit);

        // 5. โหลดกลับหน้า Main Menu
        Debug.Log("Loading Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // โชว์ข้อความเตือนแล้วซ่อน
    IEnumerator ShowWarning()
    {
        warningText.SetActive(true);
        yield return new WaitForSeconds(2f);
        warningText.SetActive(false);
    }
}