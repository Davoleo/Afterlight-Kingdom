using Player;
using UnityEngine;

namespace Enemies
{
    [System.Serializable]
    public class EnemyTarget
    {
        [SerializeField] private Transform player;
        [SerializeField] private float detectionRange = 6f;
        [SerializeField] private float losePlayerRange = 8f;

        public Transform Player => player;
        public float DetectionRange => detectionRange;
        public float LosePlayerRange => losePlayerRange;

        public void FindPlayer()
        {
            if (player != null)
                return;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        public bool HasPlayer()
        {
            return player != null;
        }

        public bool IsPlayerDead()
        {
            if (player == null)
                return true;

            HealthManager health = player.GetComponentInParent<HealthManager>();

            return health != null && health.Health <= 0;
        }

        public float DistanceFrom(Vector3 enemyPosition)
        {
            if (player == null)
                return Mathf.Infinity;

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
            if (player == null)
                return Vector3.zero;

            Vector3 direction = player.position - enemyPosition;
            direction.y = 0f;

            return direction.normalized;
        }

        public Vector3 AimPosition(float verticalOffset = 1f)
        {
            if (player == null)
                return Vector3.zero;

            return player.position + Vector3.up * verticalOffset;
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