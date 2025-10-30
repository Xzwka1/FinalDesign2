using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private int damageToDeal; // �������������� Enemy

    private bool hasHit = false; // ����û�ͧ�ѹ��ê����
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // (�ҧ���͡) ����µ���ͧ��� ������仹ҹ�Թ 5 �Թҷ� (�ѹá Scene)
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// �ѧ��ѹ���ж١���¡�� PlayerShoot.cs
    /// </summary>
    public void Initialize(Vector3 force, int damage)
    {
        damageToDeal = damage;

        // �� ForceMode.Impulse ��������ç���ᷡ�ѹ��
        rb.AddForce(force, ForceMode.Impulse);
    }

    /// <summary>
    /// �ӧҹ����͡���ع���Ѻ���úҧ���ҧ
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // (!!!) 1. ��Ǩ�ͺ����ª�����������ѧ
        if (hasHit) return; // ����ª����� (�� true) ����͡�ҡ�ѧ��ѹ���ѹ��

        // (!!!) 2. ��駤����� "������" (�ѹ��ê���������Ѵ�)
        hasHit = true;

        // --- 3. ��Ǩ�ͺ��Ҫ� Enemy ������� ---
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();

        if (enemy != null)
        {
            Debug.Log("Bullet hit an Enemy!");
            enemy.TakeDamage(damageToDeal);
        }
        else
        {
            Debug.Log("Bullet hit a wall or something else.");
        }

        // --- 4. �����ҨЪ����á��� ������¡���ع��� ---
        Destroy(gameObject);
    }
}