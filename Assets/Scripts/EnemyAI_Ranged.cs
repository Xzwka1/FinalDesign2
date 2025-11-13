using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI_Ranged : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;

    [Header("AI State")]
    public float sightRange = 25f;
    public float attackRange = 15f;
    private AIState currentState;
    private bool playerInSightRange;
    private bool playerInAttackRange;

    [Header("Patrolling")]
    public float patrolRadius = 10f;
    private Vector3 startPosition;
    private Quaternion startRotation; // ❗️ เพิ่ม: เก็บการหมุน

    [Header("Attacking")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private float attackTimer = 0f;

    [Header("Health (Enemy)")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;
    private enum AIState { Patrolling, Chasing, Attacking }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // ❗️ 1. จดจำค่าเริ่มต้น
        startPosition = transform.position;
        startRotation = transform.rotation;
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        currentState = AIState.Patrolling;
        SetNewPatrolDestination();
    }

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

    void Patrol() { agent.isStopped = false; if (!agent.pathPending && agent.remainingDistance < 0.5f) SetNewPatrolDestination(); }

    void SetNewPatrolDestination() { Vector3 randomDirection = Random.insideUnitSphere * patrolRadius; randomDirection += startPosition; NavMeshHit hit; if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1)) agent.SetDestination(hit.position); }

    void Chase() { agent.isStopped = false; agent.SetDestination(player.position); }

    void Attack()
    {
        agent.isStopped = true;
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        if (attackTimer <= 0f)
        {
            if (bulletPrefab != null && firePoint != null)
            {
                firePoint.LookAt(player.position + Vector3.up);
                GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                EnemyBullet bulletScript = bulletObject.GetComponent<EnemyBullet>();
                if (bulletScript != null) bulletScript.InitializeBullet(attackDamage);
            }
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
        if (isDead) return; // ❗️ ป้องกันการตายซ้ำ
        isDead = true;      // ❗️ ตั้งค่าว่าตายแล้ว

        Debug.Log(gameObject.name + " (Ranged) ตายแล้ว");
        gameObject.SetActive(false); // ซ่อนตัว

        // ❗️ รายงาน GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.ReportEnemyKilled();
        }
    }

    // ❗️ 3. ฟังก์ชันรีเซ็ต
    public void ResetEnemy()
    {
        isDead = false;
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
}