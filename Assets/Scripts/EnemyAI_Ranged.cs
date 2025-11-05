using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_Ranged : MonoBehaviour // ❗️ เปลี่ยนชื่อคลาส
{
    [Header("References")]
    public Transform player;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;

    [Header("AI State")]
    public float sightRange = 25f;  // (แนะนำให้เพิ่มระยะมองเห็น)
    public float attackRange = 15f; // ❗️ (สำคัญ) ตั้งค่าระยะยิงให้ไกลขึ้น
    private AIState currentState;
    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Patrolling")]
    public float patrolRadius = 10f;
    private Vector3 startPosition;

    [Header("Attacking")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int attackDamage = 10; // ❗️ (เพิ่ม) ดาเมจที่ AI ตัวนี้จะยิง
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

    // (หมายเหตุ: attackDamage ไม่ได้ใช้ในสคริปต์นี้แล้ว เพราะกระสุนจะเป็นตัวกำหนดดาเมจเอง)
    // public int attackDamage = 10; 

    [Header("Health (Enemy)")]
    public int maxHealth = 50;
    private int currentHealth;

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
        startPosition = transform.position;

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
        if (player == null || playerHealth == null) return;

        // 1. ตรวจสอบระยะห่าง
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        // 2. อัปเดตสถานะ AI
        // (ตรรกะนี้ถูกต้องแล้ว: ถ้าอยู่ในระยะยิง ให้ยิง, ถ้าระยะเห็น ให้ไล่, ถ้านอกระยะ ให้ลาดตระเวน)
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

        // 3. ทำงานตามสถานะ
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

        // 4. อัปเดตตัวจับเวลาโจมตี
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void Patrol()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewPatrolDestination();
        }
    }

    void SetNewPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        // (เรายังคงให้มันไล่ตามตำแหน่ง Player แต่ State Machine ใน Update
        // จะสลับเป็น Attacking ทันทีที่เข้า AttackRange)
        agent.SetDestination(player.position);
    }

    /// <summary>
    /// ❗️ (อัปเดต) สถานะ: โจมตี Player (แบบยิงไกล)
    /// </summary>
    /// <summary>
    /// สถานะ: โจมตี Player (แบบยิงไกล)
    /// </summary>
    void Attack()
    {
        agent.isStopped = true; // หยุดเดินเพื่อยิง

        // หันหน้าหา Player แบบสมูท (เฉพาะแกน Y ไม่ก้มเงย)
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (attackTimer <= 0f)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                // บังคับจุดยิงหันหา Player ทันที (เล็งที่กลางตัว Player สูงจากพื้น 1 เมตร)
                // เพื่อให้กระสุนพุ่งไปหาเป้าหมายแม่นยำ ไม่เบี้ยวตามการหมุนของตัวศัตรู
                firePoint.LookAt(player.position + Vector3.up);

                // 1. สร้างกระสุน
                GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

                // 2. ดึงสคริปต์ EnemyBullet และส่งค่าดาเมจ
                EnemyBullet bulletScript = bulletObject.GetComponent<EnemyBullet>();
                if (bulletScript != null)
                {
                    bulletScript.InitializeBullet(attackDamage);
                }
                else
                {
                    Debug.LogWarning("Prefab กระสุนไม่มีสคริปต์ EnemyBullet!");
                }
            }
            else
            {
                Debug.LogWarning("EnemyAI_Ranged: ไม่ได้ตั้งค่า bulletPrefab หรือ firePoint!");
            }

            // รีเซ็ตตัวจับเวลา
            attackTimer = timeBetweenAttacks;
        }
    }

    // --- (ส่วน TakeDamage และ Die เหมือนเดิมทุกประการ) ---

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
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}