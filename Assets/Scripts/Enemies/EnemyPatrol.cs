using UnityEngine;

namespace Enemies
{
    [System.Serializable]
    public class EnemyPatrol
    {
        [SerializeField] private Transform leftPoint;
        [SerializeField] private Transform rightPoint;
        [SerializeField] private float pointTolerance = 0.25f;

        private Vector3 leftPosition;
        private Vector3 rightPosition;
        private Vector3 currentTarget;
        private bool hasValidPoints;

        public void Initialize(Vector3 fallbackPosition)
        {
            hasValidPoints = leftPoint != null && rightPoint != null;

            if (!hasValidPoints)
            {
                leftPosition = fallbackPosition;
                rightPosition = fallbackPosition;
                currentTarget = fallbackPosition;
                return;
            }

            leftPosition = leftPoint.position;
            rightPosition = rightPoint.position;
            currentTarget = rightPosition;
        }

        public void Reset()
        {
            currentTarget = rightPosition;
        }

        public Vector3 GetDirection(Vector3 currentPosition)
        {
            if (!hasValidPoints)
                return Vector3.zero;

            Vector3 direction = currentTarget - currentPosition;
            direction.y = 0f;

            if (direction.magnitude <= pointTolerance)
                SwitchTarget();

            return direction.normalized;
        }

        private void SwitchTarget()
        {
            float distanceToRight = Vector3.Distance(currentTarget, rightPosition);

            currentTarget = distanceToRight <= 0.01f
                ? leftPosition
                : rightPosition;
        }

        public void DrawGizmos()
        {
            if (leftPoint == null || rightPoint == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
            Gizmos.DrawSphere(leftPoint.position, 0.15f);
            Gizmos.DrawSphere(rightPoint.position, 0.15f);
        }
    }
}