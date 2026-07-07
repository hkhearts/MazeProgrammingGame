using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI & Animation")]
    public Slider playerHealthBar;
    public Animator animator;
    
    [Header("Combat")]
    public GameObject swordObject; 
    public float attackCooldown = 1f;
    
    [HideInInspector] 
    public bool isAttacking = false; 
    
    private bool canAttack = true;
    private bool isDead = false; // Prevents taking damage after dying

    void Start()
    {
        currentHealth = maxHealth;
        
        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = maxHealth;
            playerHealthBar.value = currentHealth;
        }
    }

    void Update()
    {
        // Don't allow attacking if we are dead
        if (Input.GetMouseButtonDown(0) && canAttack && !isDead)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        
        // 1. Trigger the animation
        if (animator != null) animator.SetTrigger("Slash");

        // 2. PLAY THE AUDIO HERE!
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySlash();
        
        isAttacking = true; 
        yield return new WaitForSeconds(0.3f); 
        
        isAttacking = false; 
        
        yield return new WaitForSeconds(attackCooldown - 0.3f); 
        canAttack = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        if (playerHealthBar != null) 
            playerHealthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeath());
        }
    }

    // A new coroutine to handle dying so we can hear the sound before restarting
    IEnumerator HandleDeath()
    {
        isDead = true;
        Debug.Log("Player Died! Game Over.");

        // PLAY THE LOSE AUDIO HERE!
        if (SoundManager.Instance != null) SoundManager.Instance.PlayLose();

        // Wait a couple of seconds for the sound to play and let the player realize they lost
        yield return new WaitForSeconds(2f);

        // Now restart the scene
        SceneManager.LoadScene(0);
    }
}