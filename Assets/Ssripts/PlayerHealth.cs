using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // ... (�������� � ) ...

    // 1. ��䢺�÷Ѵ���: ����¹ PlayerMove -> SimplePlayerMovement
    private SimplePlayerMovement playerMove;

    void Start()
    {
        // ... (����� � ) ...

        // 2. ��䢺�÷Ѵ���: ����¹ PlayerMove -> SimplePlayerMovement
        playerMove = GetComponent<SimplePlayerMovement>();
        if (playerMove == null)
        {
            Debug.LogError("SimplePlayerMovement script not found on Player!");
        }
    }

    // ... (TakeDamage, UpdateHealthUI) ...

    private void Die()
    {
        Debug.Log("Player has died!");

        if (playerMove != null)
        {
            // 3. ��÷Ѵ�������ᴧ ���е͹��� playerMove �١�к� type �� SimplePlayerMovement ����
            playerMove.Respawn();
        }
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
}