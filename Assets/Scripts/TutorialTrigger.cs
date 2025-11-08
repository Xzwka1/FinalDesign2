using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("UI ที่ต้องการให้แสดง")]
    public GameObject tutorialUiObject; // ช่องสำหรับลาก UI Text มาใส่

    // เมื่อมีอะไรเดินเข้ามาในเขต Trigger
    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าสิ่งที่ชนคือ Player หรือไม่ (ต้องแน่ใจว่าตัวละครของคุณติด Tag ว่า "Player")
        if (other.CompareTag("Player"))
        {
            tutorialUiObject.SetActive(true); // เปิดการแสดงผล UI
        }
    }

    // เมื่อเดินออกจากเขต Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialUiObject.SetActive(false); // ซ่อน UI เมื่อเดินออกไปแล้ว
        }
    }
}