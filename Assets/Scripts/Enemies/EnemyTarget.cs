using Player;
using UnityEngine;

namespace Enemies
{
    [System.Serializable]
    public class EnemyTarget
    {
        [Header("Target")]
        [SerializeField] private Transform player;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float losePlayerRange = 8f;

        private HealthManager playerHealth;

        public Transform Player => player;
        public float DetectionRange => detectionRange;
        public float LosePlayerRange => losePlayerRange;

        public void Initialize()
        {
            FindPlayerIfMissing();
            CachePlayerHealth();
        }

        public void FindPlayerIfMissing()
        {
            if (player != null)
                return;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject.transform;
        }

        private void CachePlayerHealth()
        {
            playerHealth = player.GetComponentInParent<HealthManager>();
        }

        public bool HasPlayer()
        {
            return player != null;
        }

        public bool IsPlayerDead()
        {
            return playerHealth.Health <= 0;
        }

        public float DistanceFrom(Vector3 enemyPosition)
        {
            return Vector3.Distance(enemyPosition, player.position);
        }

        public bool IsInsideDetection(Vector3 enemyPosition)
        {
            return DistanceFrom(enemyPosition) <= detectionRange;
        }

        public bool IsOutsideLoseRange(Vector3 enemyPosition)
        {
            return DistanceFrom(enemyPosition) >= losePlayerRange;
        }

        public Vector3 DirectionFrom(Vector3 enemyPosition)
        {
            Vector3 direction = player.position - enemyPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return Vector3.zero;

            return direction.normalized;
        }

        public Vector3 AimPosition(float verticalOffset = 1f)
        {
            return player.position + Vector3.up * verticalOffset;
        }

        public bool IsPlayerCollider(Collider colliderToCheck)
        {
            return colliderToCheck.transform == player
                   || colliderToCheck.transform.IsChildOf(player)
                   || player.IsChildOf(colliderToCheck.transform);
        }

        public void DrawGizmos(Vector3 enemyPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(enemyPosition, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(enemyPosition, losePlayerRange);
        }
    }
}