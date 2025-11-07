using UnityEngine;
using UnityEngine.AI; // ❗️ (สำคัญมาก) ต้องมีบรรทัดนี้สำหรับ NavMesh
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))] // ❗️ บังคับให้มี NavMeshAgent
public class TwoPhaseBossAI : MonoBehaviour
{
    // --- (ส่วนที่เพิ่มเข้ามา) ---
    [Header("Movement & State")]
    public float sightRange = 15f;  // ระยะมองเห็น
    public float attackRange = 2f;  // ระยะโจมตี (ต้องใกล้กว่า sightRange)
    private NavMeshAgent agent;
    private AIState currentState;
    private bool playerInSightRange;
    private bool playerInAttackRange;
    private float attackTimer = 0f; // ❗️ ใช้ตัวจับเวลาแทน Coroutine
    private PlayerHealth playerHealthScript; // ❗️ เก็บสคริปต์ Player

    // ❗️ Enum สำหรับจัดการสถานะ (เพิ่มเข้ามา)
    private enum AIState
    {
        Patrolling, // (เราจะยังไม่ทำส่วนนี้ก่อน ให้มันไล่ล่าเลย)
        Chasing,
        Attacking
    }
    // --- (จบส่วนที่เพิ่มเข้ามา) ---

    // สร้าง enum เพื่อจัดการสถานะของบอสได้ง่ายขึ้น
    public enum BossPhase
    {
        Phase1,
        Phase2
    }

    [Header("Boss Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;
    [Tooltip("บอสจะเข้า Phase 2 เมื่อ HP ต่ำกว่ากี่เปอร์เซ็นต์ (เช่น 0.5 = 50%)")]
    public float phase2ThresholdPercentage = 0.5f;

    [Header("Target")]
    public Transform playerTarget;

    [Header("Phase 1 Settings")]
    [Tooltip("เวลาหน่วงระหว่างการโจมตีใน Phase 1 (ยิ่งมากยิ่งช้า)")]
    public float phase1AttackSpeed = 4.0f; // โจมตีทุก 4 วินาที
    public int phase1Damage = 35;

    [Header("Phase 2 Settings")]
    [Tooltip("เวลาหน่วงระหว่างการโจมตีใน Phase 2 (ยิ่งน้อยยิ่งเร็ว)")]
    public float phase2AttackSpeed = 2.0f; // โจมตีทุก 2 วินาที
    public int phase2Damage = 55;
    public float phase2MoveSpeedMultiplier = 1.5f; // (Optional) ความเร็วเคลื่อนที่ใน Phase 2

    // ตัวแปรภายใน
    private BossPhase currentPhase;
    // ❗️ private bool canAttack = true; // (ไม่จำเป็นแล้ว เราจะใช้ attackTimer)
    private float originalMoveSpeed; // (เพิ่มเข้ามา) เก็บความเร็วเดิม


    void Start()
    {
        // --- (ส่วนที่เพิ่มเข้ามา) ---
        agent = GetComponent<NavMeshAgent>();
        originalMoveSpeed = agent.speed; // เก็บความเร็วปกติไว้
        // --- (จบส่วนที่เพิ่มเข้ามา) ---

        currentHealth = maxHealth;
        currentPhase = BossPhase.Phase1; // เริ่มที่ Phase 1

        // ถ้าไม่ได้ลาก Player มาใส่ใน Inspector ให้พยายามหาอัตโนมัติ
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            else
            {
                Debug.LogError("Boss หา Player ไม่เจอ! กรุณาลาก Player ใส่ในช่อง 'Player Target'");
                this.enabled = false;
                return; // ❗️ ออกจาก Start() เลย
            }
        }

        // ❗️ (เพิ่มเข้ามา) ค้นหา Script PlayerHealth
        playerHealthScript = playerTarget.GetComponent<PlayerHealth>();
        if (playerHealthScript == null)
        {
            Debug.LogError("Boss หา Script 'PlayerHealth' บนตัว Player ไม่เจอ!");
            this.enabled = false;
        }

        // ❗️ (ลบออก) เราจะไม่ใช้ Coroutine Loop แล้ว
        // StartCoroutine(AttackLoop());
    }

    // ❗️ (เพิ่มเข้ามา) เพิ่มฟังก์ชัน Update() เพื่อจัดการสถานะและการเคลื่อนที่
    void Update()
    {
        if (playerTarget == null)
        {
            // (ถ้า Player ตายหรือหายไป) ให้บอสหยุดนิ่ง
            agent.isStopped = true;
            currentState = AIState.Patrolling; // (หรือจะให้กลับไปเดินสุ่มก็ได้)
            return;
        }

        // --- 1. ตรวจสอบระยะห่าง ---
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        // --- 2. อัปเดตสถานะ AI ---
        // ❗️ ตรรกะ: ถ้าอยู่ในระยะโจมตี -> โจมตี
        // ❗️       ถ้านอกระยะโจมตี แต่มองเห็น -> ไล่ล่า
        // ❗️       (ในโค้ดนี้ เราจะไม่ทำ Patrolling)
        if (playerInAttackRange)
        {
            currentState = AIState.Attacking;
        }
        else if (playerInSightRange)
        {
            currentState = AIState.Chasing;
        }
        else
        {
            // ถ้าหลุดระยะมองเห็น (เราจะให้มันหยุดไล่ล่า)
            currentState = AIState.Patrolling; // (ในที่นี้ Patrolling = หยุดนิ่ง)
        }

        // --- 3. ทำงานตามสถานะ ---
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol(); // (หยุดนิ่ง)
                break;
            case AIState.Chasing:
                Chase(); // ไล่ล่า
                break;
            case AIState.Attacking:
                Attack(); // โจมตี
                break;
        }

        // --- 4. อัปเดตตัวจับเวลาโจมตี ---
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    // ❗️ (ฟังก์ชันใหม่) หยุดนิ่ง (หรือจะใส่โค้ดเดินสุ่มทีหลังก็ได้)
    void Patrol()
    {
        agent.isStopped = true;
        // (ถ้าอยากให้เดินสุ่ม ค่อยเพิ่มโค้ด SetNewPatrolDestination() เหมือนใน EnemyAI)
    }

    // ❗️ (ฟังก์ชันใหม่) ไล่ล่า Player
    void Chase()
    {
        agent.isStopped = false; // สั่งให้เดิน
        agent.SetDestination(playerTarget.position); // ไปที่ตำแหน่ง Player
    }

    // ❗️ (ฟังก์ชันใหม่) โจมตี Player (แทนที่ AttackLoop)
    void Attack()
    {
        agent.isStopped = true; // หยุดเดินเพื่อโจมตี
        transform.LookAt(playerTarget); // หันหน้าหา

        // ถ้า attackTimer หมดเวลา (พร้อมโจมตี)
        if (attackTimer <= 0f)
        {
            // ตรวจสอบว่าอยู่ Phase ไหน
            float attackWaitTime;
            int damageAmount;

            if (currentPhase == BossPhase.Phase1)
            {
                attackWaitTime = phase1AttackSpeed;
                damageAmount = phase1Damage;
            }
            else // (currentPhase == BossPhase.Phase2)
            {
                attackWaitTime = phase2AttackSpeed;
                damageAmount = phase2Damage;
            }

            // 1. สั่งให้โจมตี
            PerformAttack(damageAmount);

            // 2. รีเซ็ตตัวจับเวลา
            attackTimer = attackWaitTime;
        }
    }


    // ❗️ (ลบออก) เราไม่ใช้ AttackLoop() แล้ว
    // IEnumerator AttackLoop() { ... }


    void PerformAttack(int damage)
    {
        if (playerTarget == null) return;

        // ❗️ (ลบออก) เราย้าย transform.LookAt(playerTarget) ไปไว้ใน Attack() แล้ว

        Debug.Log($"บอสโจมตี! (Phase: {currentPhase}) ด้วยความแรง {damage} DMG");

        // ❗️ (ปรับปรุง) ใช้ตัวแปร playerHealthScript ที่เราเก็บไว้ใน Start()
        if (playerHealthScript != null)
        {
            playerHealthScript.TakeDamage(damage); // เรียกฟังก์ชันรับดาเมจของ Player
        }
    }

    // ฟังก์ชันนี้ต้องถูกเรียกจาก "กระสุน" หรือ "อาวุธ" ของ Player
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"บอสโดนดาเมจ {damageAmount}. พลังชีวิตเหลือ {currentHealth}/{maxHealth}");

        // (ทางเลือก) เมื่อถูกโจมตี ให้ไล่ล่า Player ทันที
        currentState = AIState.Chasing;

        // 1. เช็คว่าตายหรือยัง
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 2. เช็คว่าถึงเวลาเปลี่ยน Phase หรือยัง
        if (currentPhase == BossPhase.Phase1 && currentHealth <= (maxHealth * phase2ThresholdPercentage))
        {
            StartPhase2();
        }
    }

    void StartPhase2()
    {
        currentPhase = BossPhase.Phase2;
        Debug.LogWarning("บอสเข้าสู่ PHASE 2!");

        // --- (ส่วนที่เพิ่มเข้ามา) ---
        // ❗️ ทำให้บอสเคลื่อนที่เร็วขึ้น
        agent.speed = originalMoveSpeed * phase2MoveSpeedMultiplier;
        // --- (จบส่วนที่เพิ่มเข้ามา) ---

        // (ลูกเล่นอื่นๆ)
        // GetComponent<Renderer>().material.color = Color.red; 
        // animator.SetTrigger("Enrage");
    }

    void Die()
    {
        Debug.Log("บอสถูกกำจัดแล้ว!");
        // ❗️ (ลบออก) เราไม่ใช้ Coroutine แล้ว
        // StopAllCoroutines(); 

        agent.isStopped = true; // หยุดเดิน
        this.enabled = false; // ❗️ ปิดการทำงานของสคริปต์นี้ไปเลย (ดีกว่า)

        Destroy(gameObject, 3.0f);
    }

    // ❗️ (ฟังก์ชันใหม่) วาดวงกลมแสดงระยะใน Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}