using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    // ... (ตัวแปรเดิมทั้งหมดคงเดิม) ...
    [Header("References")]
    public Transform player;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;

    [Header("AI State")]
    public float sightRange = 15f;
    public float attackRange = 2f;
    private AIState currentState;
    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Patrolling")]
    public float patrolRadius = 10f;
    private Vector3 startPosition;
    private Quaternion startRotation; // เพิ่ม: เก็บค่าการหมุนเริ่มต้นด้วย

    [Header("Attacking")]
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

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

        // >>> 1. จดจำค่าเริ่มต้น <<<
        startPosition = transform.position;
        startRotation = transform.rotation;
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        // เริ่มต้นด้วยการลาดตระเวน
        currentState = AIState.Patrolling;
        SetNewPatrolDestination();
    }

    // ... (ฟังก์ชัน Update, Patrol, Chase, Attack, TakeDamage เหมือนเดิม) ...
    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        if (playerInAttackRange) currentState = AIState.Attacking;
        else if (playerInSightRange) currentState = AIState.Chasing;
        else currentState = AIState.Patrolling;

        switch (currentState)
        {
            case AIState.Patrolling: Patrol(); break;
            case AIState.Chasing: Chase(); break;
            case AIState.Attacking: Attack(); break;
        }

        if (attackTimer > 0) attackTimer -= Time.deltaTime;
    }

    void Patrol()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f) SetNewPatrolDestination();
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
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.isStopped = true;
        transform.LookAt(player.position);
        if (attackTimer <= 0f)
        {
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
            attackTimer = timeBetweenAttacks;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentState = AIState.Chasing;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // >>> 2. เปลี่ยนจาก Destroy เป็น Disable <<<
        gameObject.SetActive(false);
    }

    // >>> 3. เพิ่มฟังก์ชันรีเซ็ต <<<
    public void ResetEnemy()
    {
        currentHealth = maxHealth;              // คืนเลือดเต็ม
        transform.position = startPosition;     // ย้ายกลับที่เดิม
        transform.rotation = startRotation;     // หันหน้าทางเดิม
        gameObject.SetActive(true);             // เปิดใช้งานอีกครั้ง

        currentState = AIState.Patrolling;      // กลับสู่สถานะลาดตระเวน
        if (agent != null && agent.isOnNavMesh) // ป้องกัน error กรณี agent ยังไม่พร้อม
        {
            agent.Warp(startPosition);         // ใช้ Warp เพื่อย้ายตำแหน่งบน NavMesh ทันที
            agent.ResetPath();                 // ล้างเส้นทางเก่าที่ค้างอยู่
            SetNewPatrolDestination();         // เริ่มเดินลาดตระเวนใหม่
        }
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