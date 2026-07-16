using UnityEngine;

namespace Enemies
{
    public class ChargingEnemyController : BaseEnemyController
    {
        private enum EnemyState
        {
            Sleeping,
            Patrolling,
            PreparingCharge,
            Charging,
            Cooldown
        }

        [Header("Charge References")]
        [SerializeField] private Transform chargeHitPoint;

        [Header("Charge Movement")]
        [SerializeField] private float chargeSpeed = 9f;

        [Header("Charge")]
        [SerializeField] private float chargeWindup = 0.45f;
        [SerializeField] private float chargeDuration = 0.75f;
        [SerializeField] private float chargeCooldown = 1.2f;
        [SerializeField] private float chargeHitRadius = 1.1f;
        [SerializeField] private int chargeDamage = 1;

        // Stronger knockback for the charging enemy
        [SerializeField] private float chargeKnockbackDistance = 5.5f;

        [SerializeField] private float maxChargeHeightDifference = 0.5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckDistance = 0.35f;

        private EnemyState currentState;
        private Vector3 chargeDirection;
        private bool hasHitPlayerDuringCharge;
        private float stateStartTime;

        protected override void Update()
        {
            base.Update();

            if (!HasPlayer() || IsPlayerDead())
                return;

            UpdateState();
            UpdateMovementDirection();

            MoveAndRotate(Time.deltaTime);
        }

        public override void Activate()
        {
            if (currentState == EnemyState.Sleeping)
                ChangeState(EnemyState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? EnemyState.Patrolling : EnemyState.Sleeping);
        }

        protected override void OnResetToSpawn()
        {
            chargeDirection = Vector3.zero;
            hasHitPlayerDuringCharge = false;
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == EnemyState.Charging ? chargeSpeed : patrolSpeed;
        }

        private bool IsPlayerAtChargeHeight()
        {
            if (!HasPlayer())
                return false;

            float heightDifference = Mathf.Abs(Target.Player.position.y - transform.position.y);

            return heightDifference <= maxChargeHeightDifference;
        }

        private void UpdateState()
        {
            if (currentState == EnemyState.Sleeping)
                return;

            switch (currentState)
            {
                case EnemyState.Patrolling:
                    if (IsPlayerInsideDetection() && IsPlayerAtChargeHeight())
                        PrepareCharge();
                    break;

                case EnemyState.PreparingCharge:
                    if (IsPlayerOutsideLoseRange() || !IsPlayerAtChargeHeight())
                    {
                        ChangeState(EnemyState.Patrolling);
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeWindup)
                        StartCharge();

                    break;

                case EnemyState.Charging:
                    //check obstacle before hit, otherwise the enemy could damage the player
                    //in the same frame where it should stop against a block
                    if (IsObstacleInChargeDirection())
                    {
                        StopChargeAgainstObstacle();
                        return;
                    }

                    CheckChargeHit();

                    if (Time.time >= stateStartTime + chargeDuration)
                        ChangeState(EnemyState.Cooldown);

                    break;

                case EnemyState.Cooldown:
                    if (Time.time < stateStartTime + chargeCooldown)
                        return;

                    if (IsPlayerInsideDetection() && IsPlayerAtChargeHeight())
                        PrepareCharge();
                    else
                        ChangeState(EnemyState.Patrolling);

                    break;
            }
        }

        private void PrepareCharge()
        {
            chargeDirection = GetPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
                return;

            MovementDirection = Vector3.zero;
            LookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            ChangeState(EnemyState.PreparingCharge);
        }

        private void StartCharge()
        {
            if (!IsPlayerAtChargeHeight())
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            chargeDirection = GetPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            if (IsObstacleInChargeDirection())
            {
                StopChargeAgainstObstacle();
                return;
            }

            MovementDirection = chargeDirection;
            LookDirection = chargeDirection;
            hasHitPlayerDuringCharge = false;

            ChangeState(EnemyState.Charging);
        }

        private bool IsObstacleInChargeDirection()
        {
            if (chargeDirection.sqrMagnitude < 0.01f)
                return false;

            if (obstacleLayer.value == 0)
                return false;

            Vector3 direction = chargeDirection.normalized;

            //raycast to detect obstacles between enemy and player
            Vector3 origin = GetBodyCenter();

            return HasBlockingHit(
                origin,
                direction,
                obstacleCheckDistance,
                out Collider blockingCollider
            );
        }

        private void StopChargeAgainstObstacle()
        {
            MovementDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            ChangeState(EnemyState.Cooldown);
        }

        private void StopChargeAfterPlayerHit()
        {
            MovementDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            ChangeState(EnemyState.Cooldown);
        }

        private void CheckChargeHit()
        {
            if (hasHitPlayerDuringCharge)
                return;

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + chargeDirection * 0.9f + Vector3.up * 0.7f;

            Collider[] hitColliders = Physics.OverlapSphere(
                hitPosition,
                chargeHitRadius,
                playerLayer
            );

            foreach (Collider hitCollider in hitColliders)
            {
                //before applying damage we check if an obstacle blocks the hit
                if (IsPlayerBlockedByObstacle(hitCollider))
                {
                    continue;
                }

                bool damageApplied = TryDamagePlayer(
                    hitCollider,
                    chargeDamage,
                    chargeDirection,
                    true,
                    chargeKnockbackDistance
                );

                hasHitPlayerDuringCharge = true;

                StopChargeAfterPlayerHit();

                return;
            }
        }

        private bool IsPlayerBlockedByObstacle(Collider playerCollider)
        {
            if (obstacleLayer.value == 0)
                return false;

            Vector3 origin = GetBodyCenter();
            Vector3 targetPosition = playerCollider.bounds.center;

            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
                return false;

            direction.Normalize();

            bool hasBlockingHit = HasBlockingHit(
                origin,
                direction,
                distance,
                out Collider blockingCollider
            );

            if (!hasBlockingHit)
                return false;

            return true;
        }

        private bool HasBlockingHit(
            Vector3 origin,
            Vector3 direction,
            float distance,
            out Collider blockingCollider)
        {
            blockingCollider = null;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                distance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );

            if (hits.Length == 0)
                return false;

            System.Array.Sort(
                hits,
                (firstHit, secondHit) => firstHit.distance.CompareTo(secondHit.distance)
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (IsOwnCollider(hit.collider))
                    continue;

                if (Target.IsPlayerCollider(hit.collider))
                    return false;

                blockingCollider = hit.collider;
                return true;
            }

            return false;
        }

        private Vector3 GetBodyCenter()
        {
            if (characterController == null)
                return transform.position + Vector3.up;

            return transform.TransformPoint(characterController.center);
        }

        private void ChangeState(EnemyState newState)
        {
            currentState = newState;
            stateStartTime = Time.time;
        }

        private void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case EnemyState.Sleeping:
                    break;

                case EnemyState.Patrolling:
                    if (IsPlayerInsideDetection())
                    {
                        MovementDirection = Vector3.zero;
                        LookDirection = GetPlayerDirection();
                    }
                    else
                    {
                        MovementDirection = GetNavMeshPatrolDirection();
                        LookDirection = MovementDirection;
                    }

                    break;

                case EnemyState.PreparingCharge:
                    MovementDirection = Vector3.zero;
                    LookDirection = chargeDirection.sqrMagnitude > 0.01f
                        ? chargeDirection
                        : GetPlayerDirection();
                    break;

                case EnemyState.Charging:
                    MovementDirection = chargeDirection;
                    LookDirection = chargeDirection;
                    break;

                case EnemyState.Cooldown:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + transform.forward * 0.9f + Vector3.up * 0.7f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitPosition, chargeHitRadius);

            Gizmos.color = Color.red;

            Vector3 debugDirection = Application.isPlaying && chargeDirection.sqrMagnitude > 0.01f
                ? chargeDirection.normalized
                : transform.forward;

            Vector3 debugCenter = characterController != null
                ? transform.TransformPoint(characterController.center)
                : transform.position + Vector3.up;

            Gizmos.DrawLine(
                debugCenter,
                debugCenter + debugDirection * obstacleCheckDistance
            );
        }
    }
}