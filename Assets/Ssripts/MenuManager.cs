using UnityEngine;
using UnityEngine.SceneManagement; // 1. ต้องเพิ่มบรรทัดนี้ เพื่อให้เปลี่ยนฉากได้

public class MainMenu : MonoBehaviour
{
    // ฟังก์ชันสำหรับปุ่ม "เริ่มเกม"
    // ต้องเป็น public เท่านั้น ปุ่มถึงจะมองเห็น
    public void StartGame()
    {
        // "MyGameD2" คือ "ชื่อไฟล์" Scene เกมของคุณ (ต้องตรงเป๊ะๆ)
        // (หรือชื่อ DesignProject ตามที่คุณเคยตั้ง)
        SceneManager.LoadScene("InGame");
    }

    // ฟังก์ชันสำหรับปุ่ม "ออกเกม"
    public void QuitGame()
    {
        // คำสั่งนี้จะปิดเกม (จะเห็นผลเฉพาะตอน Build เกมแล้วเท่านั้น)
        Debug.Log("QUIT GAME!");
        Application.Quit();
    }
}