using KinematicCharacterController;
using UnityEngine;

namespace Enemies
{
    public class ChargingEnemyController : EnemyController
    {
        private enum ChargeState
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
        [SerializeField] private float chargeKnockbackDistance = 4f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckDistance = 0.25f;

        private ChargeState currentState;
        private Vector3 chargeDirection;
        private bool hasHitPlayerDuringCharge;
        private float stateStartTime;

        public override void Activate()
        {
            if (currentState == ChargeState.Sleeping)
                ChangeState(ChargeState.Patrolling);
        }

        protected override void SetInitialState()
        {
            ChangeState(startsActive ? ChargeState.Patrolling : ChargeState.Sleeping);
        }

        protected override void UpdateEnemy()
        {
            UpdateState();
        }

        protected override void ResetSpecificState()
        {
            chargeDirection = Vector3.zero;
            hasHitPlayerDuringCharge = false;
        }

        protected override void SetStateAfterReset()
        {
            ChangeState(startsActive ? ChargeState.Patrolling : ChargeState.Sleeping);
        }

        private void UpdateState()
        {
            if (currentState == ChargeState.Sleeping)
                return;

            if (IsPlayerDead())
                return;

            float distanceFromPlayer = DistanceFromPlayer();

            switch (currentState)
            {
                case ChargeState.Patrolling:
                    if (distanceFromPlayer <= target.DetectionRange)
                        PrepareCharge();
                    break;

                case ChargeState.PreparingCharge:
                    if (distanceFromPlayer >= target.LosePlayerRange)
                    {
                        ChangeState(ChargeState.Patrolling);
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeWindup)
                        StartCharge();
                    break;

                case ChargeState.Charging:
                    CheckChargeHit();

                    if (IsObstacleInChargeDirection())
                    {
                        StopChargeAgainstObstacle();
                        return;
                    }

                    if (Time.time >= stateStartTime + chargeDuration)
                        ChangeState(ChargeState.Cooldown);
                    break;

                case ChargeState.Cooldown:
                    if (Time.time >= stateStartTime + chargeCooldown)
                    {
                        if (distanceFromPlayer <= target.DetectionRange)
                            PrepareCharge();
                        else
                            ChangeState(ChargeState.Patrolling);
                    }
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

            ChangeState(ChargeState.PreparingCharge);
        }

        private void StartCharge()
        {
            chargeDirection = GetPlayerDirection();

            if (chargeDirection.sqrMagnitude < 0.01f)
            {
                ChangeState(ChargeState.Patrolling);
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

            ChangeState(ChargeState.Charging);
        }

        private bool IsObstacleInChargeDirection()
        {
            if (chargeDirection.sqrMagnitude < 0.01f)
                return false;

            Vector3 origin = motor != null
                ? motor.TransientPosition + Vector3.up * 0.6f
                : transform.position + Vector3.up * 0.6f;

            float radius = motor != null ? motor.Capsule.radius * 0.95f : 0.35f;

            return Physics.SphereCast(
                origin,
                radius,
                chargeDirection.normalized,
                out RaycastHit hit,
                obstacleCheckDistance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        private void StopChargeAgainstObstacle()
        {
            MovementDirection = Vector3.zero;
            chargeDirection = Vector3.zero;

            ChangeState(ChargeState.Cooldown);
        }

        private void CheckChargeHit()
        {
            if (hasHitPlayerDuringCharge)
                return;

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + chargeDirection * 0.9f + Vector3.up * 0.7f;

            bool damaged = EnemyPlayerDamage.TryDamageFirstInSphere(
                hitPosition,
                chargeHitRadius,
                playerLayer,
                chargeDamage,
                chargeDirection,
                true,
                chargeKnockbackDistance
            );

            if (damaged)
                hasHitPlayerDuringCharge = true;
        }

        private void ChangeState(ChargeState newState)
        {
            currentState = newState;
            stateStartTime = Time.time;
        }

        protected override void UpdateMovementDirection()
        {
            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            switch (currentState)
            {
                case ChargeState.Sleeping:
                    break;

                case ChargeState.Patrolling:
                    MovementDirection = GetPatrolDirection();
                    LookDirection = MovementDirection;
                    break;

                case ChargeState.PreparingCharge:
                    MovementDirection = Vector3.zero;
                    LookDirection = chargeDirection.sqrMagnitude > 0.01f
                        ? chargeDirection
                        : GetPlayerDirection();
                    break;

                case ChargeState.Charging:
                    MovementDirection = chargeDirection;
                    LookDirection = chargeDirection;
                    break;

                case ChargeState.Cooldown:
                    MovementDirection = Vector3.zero;
                    LookDirection = GetPlayerDirection();
                    break;
            }
        }

        protected override float GetCurrentSpeed()
        {
            return currentState == ChargeState.Charging ? chargeSpeed : patrolSpeed;
        }

        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (currentState == ChargeState.Charging && IsObstacleInChargeDirection())
            {
                currentVelocity = Vector3.zero;
                StopChargeAgainstObstacle();
                return;
            }

            base.UpdateVelocity(ref currentVelocity, deltaTime);
        }

        public override void OnMovementHit(
            Collider hitCollider,
            Vector3 hitNormal,
            Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            if (currentState != ChargeState.Charging)
                return;

            if (((1 << hitCollider.gameObject.layer) & obstacleLayer) == 0)
                return;

            StopChargeAgainstObstacle();
        }

        public override void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            if (currentState != ChargeState.Charging)
                return;

            if (((1 << hitCollider.gameObject.layer) & obstacleLayer) == 0)
                return;

            StopChargeAgainstObstacle();
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Vector3 hitPosition = chargeHitPoint != null
                ? chargeHitPoint.position
                : transform.position + transform.forward * 0.9f + Vector3.up * 0.7f;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitPosition, chargeHitRadius);
        }
    }
}