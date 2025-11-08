using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("ลากจุด RespawnPoint มาใส่ตรงนี้")]
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าสิ่งที่ตกลงมาคือ Player ใช่ไหม
        if (other.CompareTag("Player"))
        {
            // ย้ายตำแหน่งผู้เล่นไปที่จุด Respawn
            other.transform.position = respawnPoint.position;

            // (แถม) ถ้าตัวผู้เล่นใช้ Rigidbody ต้องรีเซ็ตความเร็วด้วย 
            // ไม่งั้นเวลาย้ายไปจุดเกิด ตัวอาจจะยังพุ่งด้วยความเร็วเดิม
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // (แถม 2) ถ้าเกมคุณใช้ CharacterController บางทีแค้ย้าย transform มันจะไม่ไป
            // อาจต้องปิด CharacterController ชั่วคราวก่อนย้าย (ถ้าเจอปัญหาย้ายไม่ได้ค่อยมาแก้ตรงนี้)
            /*
            CharacterController cc = other.GetComponent<CharacterController>();
            if(cc != null) {
                cc.enabled = false;
                other.transform.position = respawnPoint.position;
                cc.enabled = true;
            }
            */
        }
        // ถ้าอยากให้ของอื่นๆ ตกมาแล้วทำลายทิ้ง (เช่น กล่อง, ศัตรู)
        else
        {
            Destroy(other.gameObject);
        }
    }
}