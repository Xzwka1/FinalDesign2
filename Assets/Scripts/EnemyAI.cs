using UnityEngine;
using UnityEngine.AI; // ❗️ (สำคัญมาก) ต้องมีบรรทัดนี้สำหรับ NavMesh

[RequireComponent(typeof(NavMeshAgent))] // บังคับให้มี NavMeshAgent
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player; // ลาก Player มาใส่
    private PlayerHealth playerHealth; // สคริปต์พลังชีวิตของ Player
    private NavMeshAgent agent;

    [Header("AI State")]
    public float sightRange = 15f;  // ระยะมองเห็น
    public float attackRange = 2f;  // ระยะโจมตี
    private AIState currentState;
    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Patrolling")]
    public float patrolRadius = 10f; // รัศมีการเดินสุ่ม
    private Vector3 startPosition;

    [Header("Attacking")]
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f; // หน่วงเวลาโจมตี (เช่น 2 วินาที)
    private float attackTimer = 0f;

    [Header("Health (Enemy)")]
    public int maxHealth = 50;
    private int currentHealth;

    // Enum สำหรับจัดการสถานะ
    private enum AIState
    {
        Patrolling,
        Chasing,
        Attacking
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        startPosition = transform.position; // บันทึกจุดเกิด

        // --- ค้นหา Player อัตโนมัติ ---
        // (สำคัญ: Player ของคุณต้องติด Tag "Player" ใน Inspector)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }
        else
        {
            Debug.LogError("EnemyAI: ไม่พบ Player! กรุณาตรวจสอบว่า Player มี Tag 'Player'");
        }

        currentState = AIState.Patrolling;
        SetNewPatrolDestination();
    }

    void Update()
    {
        // ถ้าไม่มี Player หรือ Player ตายแล้ว ก็ไม่ต้องทำอะไร
        if (player == null || playerHealth == null) return;

        // --- 1. ตรวจสอบระยะห่าง ---
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        // --- 2. อัปเดตสถานะ AI ---
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
            currentState = AIState.Patrolling;
        }

        // --- 3. ทำงานตามสถานะ ---
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Chasing:
                Chase();
                break;
            case AIState.Attacking:
                Attack();
                break;
        }

        // --- 4. อัปเดตตัวจับเวลาโจมตี ---
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// สถานะ: เดินลาดตระเวน
    /// </summary>
    void Patrol()
    {
        agent.isStopped = false;
        // ถ้าเดินถึงจุดหมายแล้ว หรือยังไม่มีจุดหมาย
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewPatrolDestination();
        }
    }

    /// <summary>
    /// หาจุดเดินสุ่มใหม่
    /// </summary>
    void SetNewPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition; // บวกกับจุดเริ่มต้น

        NavMeshHit hit;
        // หาตำแหน่งที่ใกล้เคียงที่สุดบน NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// สถานะ: ไล่ล่า Player
    /// </summary>
    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    /// <summary>
    /// สถานะ: โจมตี Player
    /// </summary>
    void Attack()
    {
        agent.isStopped = true; // หยุดเดินเพื่อโจมตี

        // หันหน้าหา Player
        transform.LookAt(player.position);

        if (attackTimer <= 0f)
        {
            // --- ทำการโจมตี ---
            Debug.Log("Enemy โจมตี Player!");
            if (playerHealth != null)
            {
                // เรียกใช้ฟังก์ชันใน PlayerHealth.cs
                playerHealth.TakeDamage(attackDamage);
            }

            // รีเซ็ตตัวจับเวลา
            attackTimer = timeBetweenAttacks;
        }
    }

    // ---------------------------------------------
    // --- (ส่วนนี้สำหรับให้ Player โจมตี Enemy) ---
    // ---------------------------------------------

    /// <summary>
    /// ฟังก์ชันรับดาเมจ (สำหรับให้ Player เรียกใช้)
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Enemy Health: " + currentHealth);

        // (ทางเลือก) เมื่อถูกโจมตี ให้ไล่ล่า Player ทันที
        currentState = AIState.Chasing;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy ตายแล้ว");
        // (สามารถเพิ่ม Animation หรือ Particle Effect ตรงนี้ได้)

        Destroy(gameObject); // ทำลายตัวเอง
    }

    // (ทางเลือก) วาดวงกลมแสดงระยะให้เห็นใน Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}