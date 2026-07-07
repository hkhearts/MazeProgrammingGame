using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Slider healthBar; 

    [Header("Explosion Settings")]
    public int cubesPerAxis = 3; 
    public float cubeSize = 0.3f; 
    public float explosionForce = 300f; 
    public Material enemyMaterial; 

    private PlayerCombat playerCombatScript;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerCombatScript = playerObj.GetComponent<PlayerCombat>();
        }
    }

    void LateUpdate()
    {
        if (healthBar != null)
        {
            Transform cam = Camera.main.transform;
            healthBar.transform.LookAt(healthBar.transform.position + cam.rotation * Vector3.forward, cam.rotation * Vector3.up);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (playerCombatScript != null && other.gameObject == playerCombatScript.swordObject)
        {
            if (playerCombatScript.isAttacking)
            {
                TakeDamage(999);
                
                playerCombatScript.isAttacking = false; 
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthBar != null) 
            healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            ExplodeIntoCubes();
        }
    }

    private void ExplodeIntoCubes()
    {
        Vector3 enemyCenter = transform.position + new Vector3(0, 1f, 0); 
        float halfGridSize = (cubesPerAxis * cubeSize) / 2f;
        Vector3 startCorner = enemyCenter - new Vector3(halfGridSize, halfGridSize, halfGridSize);

        for (int x = 0; x < cubesPerAxis; x++)
        {
            for (int y = 0; y < cubesPerAxis; y++)
            {
                for (int z = 0; z < cubesPerAxis; z++)
                {
                    Vector3 cubePos = startCorner + new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);
                    CreateTinyCube(cubePos, enemyCenter);
                }
            }
        }

        Destroy(gameObject);
    }

    private void CreateTinyCube(Vector3 position, Vector3 explosionCenter)
    {
        GameObject tinyCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tinyCube.transform.position = position;
        tinyCube.transform.localScale = Vector3.one * cubeSize;

        if (enemyMaterial != null) tinyCube.GetComponent<Renderer>().material = enemyMaterial;

        Rigidbody rb = tinyCube.AddComponent<Rigidbody>();
        rb.AddExplosionForce(explosionForce, explosionCenter, 2f);

        Destroy(tinyCube, 3f);
    }
}