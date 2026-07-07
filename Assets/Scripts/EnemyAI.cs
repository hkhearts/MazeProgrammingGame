using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Navigation Settings")]
    public float chaseRange = 8f;
    public float wanderRadius = 10f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;       // How close to get before jumping
    public int attackDamage = 10;          // How much health the player loses
    public float attackCooldown = 1.5f;    // Time to wait between jumps
    
    [Header("Jump Animation Settings")]
    public float jumpHeight = 1.5f;        // How high the enemy leaps
    public float jumpSpeed = 2f;           // How fast the jump happens

    private Transform player;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // This is how the enemy finds you! It searches the scene for the name "Player"
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        // Check the math distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Player is close enough to hit! Start the jump attack.
            StartCoroutine(JumpAttackRoutine());
        }
        else if (distanceToPlayer <= chaseRange)
        {
            // Player is in sight, chase them!
            agent.SetDestination(player.position);
        }
        else
        {
            // Player is far away. Wander around the maze randomly.
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                Vector3 randomDestination = GetRandomNavMeshPoint(transform.position, wanderRadius);
                agent.SetDestination(randomDestination);
            }
        }
    }

    IEnumerator JumpAttackRoutine()
    {
        isAttacking = true;
        
        // 1. Turn off the NavMeshAgent so we can lift the enemy off the floor for the jump
        agent.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position; 
        
        // Look at the player
        transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));

        float jumpProgress = 0f;

        // 2. Physical jump using a Math Sine Wave to create a smooth arc
        while (jumpProgress < 1f)
        {
            jumpProgress += Time.deltaTime * jumpSpeed;
            float heightOffset = Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;
            
            transform.position = Vector3.Lerp(startPos, targetPos, jumpProgress) + new Vector3(0, heightOffset, 0);
            yield return null; 
        }

        // 3. We landed! Did we hit the player?
        float distanceAfterLanding = Vector3.Distance(transform.position, player.position);
        
        // If the player didn't run away in time...
        if (distanceAfterLanding <= attackRange + 1.5f) 
        {
            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(attackDamage);
            }
        }

        // 4. Wait for the cooldown
        yield return new WaitForSeconds(attackCooldown);

        // 5. Turn the NavMeshAgent back on to resume walking
        agent.enabled = true;
        isAttacking = false;
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas);
        
        return navHit.position;
    }
}