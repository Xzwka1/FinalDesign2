using UnityEngine;
using UnityEngine.SceneManagement; // ❗️ (สำคัญ) สำหรับการเปลี่ยนฉาก
using System.Collections; // ❗️ (สำคัญ) สำหรับ Coroutine

public class Teleporter : MonoBehaviour
{
    [Header("ตั้งค่า Teleport")]
    [Tooltip("ใส่ชื่อฉาก 'Map 2' ของคุณที่นี่ (ต้องตรงเป๊ะ)")]
    public string sceneToLoad;

    [Header("UI แจ้งเตือน")]
    [Tooltip("ลาก Text 'You must kill...' ที่ซ่อนไว้มาใส่")]
    public GameObject warningTextObject;
    public float warningDisplayTime = 3f; // โชว์ข้อความเตือนนาน 3 วิ

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. ถาม GameManager ว่าฆ่าหมดรึยัง
            bool allKilled = false;
            if (GameManager.instance != null)
            {
                allKilled = GameManager.instance.AreAllEnemiesDefeated();
            }

            // 2. ถ้าฆ่าหมดแล้ว
            if (allKilled)
            {
                Debug.Log("เงื่อนไขครบ! กำลังวาร์ปไป " + sceneToLoad);
                SceneManager.LoadScene(sceneToLoad);
            }
            // 3. ถ้ายังฆ่าไม่หมด
            else
            {
                Debug.Log("ยังฆ่าไม่หมด! แสดงคำเตือน");
                if (warningTextObject != null)
                {
                    StartCoroutine(ShowWarningMessage());
                }
            }
        }
    }

    // ฟังก์ชันโชว์ข้อความเตือนแล้วซ่อนเอง
    private IEnumerator ShowWarningMessage()
    {
        warningTextObject.SetActive(true);
        yield return new WaitForSeconds(warningDisplayTime);
        warningTextObject.SetActive(false);
    }
}