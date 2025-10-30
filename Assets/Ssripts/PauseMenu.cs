using UnityEngine;
using UnityEngine.SceneManagement; // ❗️(สำคัญ) ต้องมีบรรทัดนี้

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel; // ❗️ ลาก Panel UI มาใส่

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // ❗️ ใส่ชื่อซีน Main Menu

    // ตัวแปร static ให้สคริปต์อื่นรู้ว่าเกมหยุดอยู่
    public static bool GameIsPaused = false;

    private bool isPaused = false;

    void Start()
    {
        // เริ่มเกมโดยการซ่อนเมนู และให้เวลาเดินปกติ
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // ให้เวลาเดินปกติ
        isPaused = false;
        GameIsPaused = false;

        // (เผื่อไว้) ปลดล็อคเมาส์กรณีย้อนมาจากเมนู
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ตรวจสอบการกดปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // ถ้าหยุดอยู่ ให้กลับไปเล่น
                ResumeGame();
            }
            else
            {
                // ถ้าเล่นอยู่ ให้หยุดเกม
                PauseGame();
            }
        }
    }

    /// <summary>
    /// หยุดเกม (เปิดเมนู, หยุดเวลา, โชว์เมาส์)
    /// </summary>
    private void PauseGame()
    {
        pauseMenuPanel.SetActive(true);

        // (!!!) นี่คือคำสั่งหยุดเวลาในเกม (!!!)
        Time.timeScale = 0f;

        isPaused = true;
        GameIsPaused = true; // บอกสคริปต์อื่น

        // ปลดล็อคเมาส์ และโชว์เมาส์
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- ⬇️ ฟังก์ชันสำหรับปุ่ม ⬇️ ---

    /// <summary>
    /// (สำหรับปุ่ม Resume) กลับไปเล่นเกม (ปิดเมนู, ให้เวลาเดิน, ซ่อนเมาส์)
    /// </summary>
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);

        // (!!!) ให้เวลาในเกมเดินต่อ (!!!)
        Time.timeScale = 1f;

        isPaused = false;
        GameIsPaused = false; // บอกสคริปต์อื่น

        // ล็อคเมาส์กลับไปที่กลางจอ และซ่อนเมาส์
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// (สำหรับปุ่ม Exit) ออกไปที่เมนูหลัก
    /// </summary>
    public void ExitToMainMenu()
    {
        // (!!!) สำคัญมาก: ต้องคืนเวลาเป็น 1f ก่อนออกจากซีน (!!!)
        Time.timeScale = 1f;
        GameIsPaused = false;

        Debug.Log("กำลังกลับไปที่ Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}