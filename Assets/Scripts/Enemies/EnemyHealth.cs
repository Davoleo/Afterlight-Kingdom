using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 3;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float hitAnimationDuration = 0.5f;
        [SerializeField] private float deathAnimationDuration = 1.5f;

        private int currentHealth;

        private BaseEnemyController enemyController;
        private NavMeshAgent navMeshAgent;

        // All enemy colliders are stored so they can be disabled
        // immediately when the enemy dies.
        private Collider[] enemyColliders;

        private Coroutine hitAnimationCoroutine;

        public int CurrentHealth => currentHealth;
        public bool IsDead => currentHealth <= 0;

        private void Start()
        {
            enemyController = GetComponent<BaseEnemyController>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            // Store all colliders belonging to the enemy,
            // including possible colliders placed on child objects.
            enemyColliders = GetComponentsInChildren<Collider>(true);

            // Search the Animator automatically if it has not
            // been assigned manually in the Inspector.
            animator = GetComponentInChildren<Animator>();

            ResetEnemy();
        }

        public void TakeDamage(int damage)
        {
            if (IsDead)
                return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
                return;
            }

            PlayHitAnimation();
        }

        private void PlayHitAnimation()
        {
            if (animator == null)
                return;

            // If another hit arrives while the previous hit animation
            // is still active, restart its duration.
            if (hitAnimationCoroutine != null)
            {
                StopCoroutine(hitAnimationCoroutine);
                hitAnimationCoroutine = null;
            }

            animator.SetBool("IsHit", true);

            hitAnimationCoroutine = StartCoroutine(HitAnimationRoutine());
        }

        private IEnumerator HitAnimationRoutine()
        {
            // Keep IsHit active for at least one Animator update.
            yield return null;

            // Keep the Hit animation active for the required duration.
            yield return new WaitForSeconds(hitAnimationDuration);

            if (!IsDead && animator != null)
                animator.SetBool("IsHit", false);

            hitAnimationCoroutine = null;
        }

        private void Die()
        {
            // Stop a possible Hit animation before starting Death.
            if (hitAnimationCoroutine != null)
            {
                StopCoroutine(hitAnimationCoroutine);
                hitAnimationCoroutine = null;
            }

            // Disable the enemy controller immediately so the enemy
            // cannot move or perform actions while dying.
            enemyController.enabled = false;

            // Stop the NavMeshAgent immediately.
            if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }

            /*
             * Disable all enemy colliders immediately.
             * This prevents arrows and other projectiles from colliding
             * with the enemy while the Death animation is playing.
             */
            SetCollidersEnabled(false);
            animator.SetBool("IsHit", false);
            animator.SetBool("IsDead", true);
            

            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            // Keep the GameObject active so the Death animation
            // remains visible even though its colliders are disabled.
            yield return null;

            yield return new WaitForSeconds(deathAnimationDuration);

            gameObject.SetActive(false);
        }

        /*
         * Enables or disables every Collider belonging to the enemy.
         */
        private void SetCollidersEnabled(bool enabled)
        {

            foreach (Collider enemyCollider in enemyColliders)
            {
                enemyCollider.enabled = enabled;
            }
        }

        public void ResetEnemy()
        {
            currentHealth = maxHealth;

            if (hitAnimationCoroutine != null)
            {
                StopCoroutine(hitAnimationCoroutine);
                hitAnimationCoroutine = null;
            }


            animator.SetBool("IsHit", false);
            animator.SetBool("IsDead", false);
            
            // Re-enable colliders when the enemy is reset/respawned.
            SetCollidersEnabled(true);

            enemyController.enabled = true;

            if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = false;
        }
    }
}