using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
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
    private Quaternion startRotation; // ❗️ เพิ่ม: เก็บการหมุน

    [Header("Attacking")]
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

    [Header("Health (Enemy)")]
    public int maxHealth = 50;
    private int currentHealth;

    // --- ⬇️ (1. เพิ่มตัวแปรเสียง) ⬇️ ---
    [Header("Audio")]
    [Tooltip("ลากไฟล์เสียงร้องตอนตาย (MP3/WAV) มาใส่")]
    public AudioClip deathSound;
    // --- ⬆️ (สิ้นสุดส่วนที่เพิ่ม) ⬆️ ---

    private bool isDead = false;
    private enum AIState { Patrolling, Chasing, Attacking }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        startPosition = transform.position;
        startRotation = transform.rotation;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        currentState = AIState.Patrolling;
        SetNewPatrolDestination();
    }

    // ... (Update, Patrol, SetNewPatrolDestination, Chase, Attack คงเดิม) ...
    #region Standard AI Behaviour
    void Update()
    {
        if (isDead) return; // ❗️ (เพิ่ม) ถ้าตายแล้ว หยุดทำงาน Update
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
    #endregion

    public void TakeDamage(int damage)
    {
        if (isDead) return; // ถ้าตายแล้ว ไม่ต้องรับดาเมจซ้ำ
        currentHealth -= damage;
        currentState = AIState.Chasing;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return; // ป้องกันการตายซ้ำ
        isDead = true;      // ตั้งค่าว่าตายแล้ว

        // --- ⬇️ (2. เพิ่มโค้ดเล่นเสียง) ⬇️ ---
        // เล่นเสียง ณ ตำแหน่งปัจจุบัน (วิธีนี้เสียงจะเล่นต่อจนจบ แม้ Object จะถูกปิด)
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        // --- ⬆️ (สิ้นสุดส่วนที่เพิ่ม) ⬆️ ---

        Debug.Log(gameObject.name + " ตายแล้ว");
        gameObject.SetActive(false); // ซ่อนตัว

        // รายงาน GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.ReportEnemyKilled();
        }
    }

    public void ResetEnemy()
    {
        isDead = false; // ❗️ รีเซ็ตสถานะการตาย
        currentHealth = maxHealth;
        transform.position = startPosition;
        transform.rotation = startRotation;
        gameObject.SetActive(true);

        currentState = AIState.Patrolling;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(startPosition);
            agent.ResetPath();
            SetNewPatrolDestination();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}