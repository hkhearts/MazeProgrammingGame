using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int weaponDamage = 25;
    private bool isAttacking = false;

    // These are called by the PlayerCombat script
    public void EnableHitbox() { isAttacking = true; }
    public void DisableHitbox() { isAttacking = false; }

    void OnTriggerEnter(Collider other)
    {
        // Only do damage IF we are currently swinging AND we hit an Enemy
        if (isAttacking && other.CompareTag("Enemy"))
        {
            Enemy enemyStats = other.GetComponent<Enemy>();
            
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(weaponDamage);
                
                // Optional: Disable hitbox immediately after one hit 
                // so you don't hit the same enemy twice in one swing.
                isAttacking = false; 
            }
        }
    }
}