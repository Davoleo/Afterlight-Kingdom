using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseEnemyController : MonoBehaviour
    {
        [Header("Base References")]
        [SerializeField] protected NavMeshAgent navMeshAgent;

        [Header("Base Components")]
        [SerializeField] protected EnemyTarget target = new EnemyTarget();
        [SerializeField] protected EnemyPatrol patrol = new EnemyPatrol();

        [Header("Base Activation")]
        [SerializeField] protected bool startsActive = true;

        [Header("Base Movement")]
        [SerializeField] protected float patrolSpeed = 2f;
        [SerializeField] protected float rotationSpeed = 12f;

        [Header("Base Grid Navigation")]
        [SerializeField] private float gridCenterTolerance = 0.02f;
        [SerializeField] private int gridResolveSearchRadius = 3;
        [SerializeField] private int gridMaxExpandedNodes = 2048;

        [Header("Base NavMesh")]
        [SerializeField] protected float navMeshSampleRadius = 2f;

        [Header("Animation")]
        [SerializeField] protected Animator animator;

        protected Vector3 MovementDirection;
        protected Vector3 LookDirection;

        protected Vector3 SpawnPosition { get; private set; }
        protected EnemyTarget Target => target;
        public Animator EnemyAnimator => animator;

        private EnemyGridNavigation gridNavigation;

        private Vector2Int currentGridCell;
        private Vector2Int spawnGridCell;

        private Vector2Int gridStepCell;
        private Vector3 gridStepCenter;
        private Vector3 spawnGridCenter;

        private bool hasGridStep;
        private bool hasResetAfterPlayerDeath;

        private readonly List<Vector2Int> pathBuffer = new List<Vector2Int>();

        protected virtual void Start()
        {
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = false;

            target.Initialize();

            if (!TryPlaceAgentOnNavMesh(transform.position) || !InitializeGridNavigation())
            {
                Debug.LogError($"{name}: impossibile inizializzare la navigazione del nemico.", this);
                enabled = false;
                return;
            }

            SpawnPosition = transform.position;
            patrol.Initialize(SpawnPosition);
            SetInitialState();
        }

        protected virtual void Update()
        {
            HandlePlayerDeathReset();
        }

        public abstract void Activate();
        protected abstract void SetInitialState();
        protected virtual void OnResetToSpawn() { }
        protected virtual float GetCurrentSpeed() => patrolSpeed;

        private bool InitializeGridNavigation()
        {
            gridNavigation = new EnemyGridNavigation(navMeshAgent, gridResolveSearchRadius, gridMaxExpandedNodes);

            Vector2Int requestedCell = gridNavigation.WorldToCell(transform.position);

            if (!gridNavigation.TryFindNearestWalkableCell(requestedCell, out Vector2Int cell)) return false;
            if (!gridNavigation.TryGetCellCenter(cell, out Vector3 center)) return false;
            if (!navMeshAgent.Warp(center)) return false;

            currentGridCell = cell;
            spawnGridCell = cell;
            spawnGridCenter = center;

            hasGridStep = false;
            gridStepCell = cell;
            gridStepCenter = center;

            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = false;

            return true;
        }

        protected void MoveAndRotate(float deltaTime)
        {
            MoveCharacterOnGrid(deltaTime);
            RotateTowardsLookDirection(deltaTime);
        }

        private void MoveCharacterOnGrid(float deltaTime)
        {
            if (!IsNavMeshAgentReady() || !hasGridStep) return;

            Vector3 direction = GetCurrentGridStepDirection();

            if (direction.sqrMagnitude < 0.01f)
            {
                CompleteGridStep();
                return;
            }

            float remainingDistance = GetGridStepRemainingDistance();

            if (remainingDistance <= gridCenterTolerance)
            {
                CompleteGridStep();
                return;
            }

            float movement = Mathf.Min(GetCurrentSpeed() * deltaTime, remainingDistance);
            navMeshAgent.Move(direction * movement);

            if (GetGridStepRemainingDistance() <= gridCenterTolerance) CompleteGridStep();
        }

        private Vector3 GetCurrentGridStepDirection()
        {
            if (!hasGridStep) return Vector3.zero;

            Vector2Int difference = gridStepCell - currentGridCell;

            if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1) return Vector3.zero;

            return new Vector3(difference.x, 0f, difference.y);
        }

        private float GetGridStepRemainingDistance()
        {
            Vector3 direction = GetCurrentGridStepDirection();

            if (direction.x != 0f) return Mathf.Abs(gridStepCenter.x - transform.position.x);
            if (direction.z != 0f) return Mathf.Abs(gridStepCenter.z - transform.position.z);

            return 0f;
        }

        private void CompleteGridStep()
        {
            if (!hasGridStep) return;

            if (!navMeshAgent.Warp(gridStepCenter))
            {
                Debug.LogWarning($"{name}: impossibile completare lo step {currentGridCell} -> {gridStepCell}.", this);
                hasGridStep = false;
                return;
            }

            currentGridCell = gridStepCell;
            hasGridStep = false;
            gridStepCenter = Vector3.zero;
        }

        //pathfinding

        protected Vector3 GetNavMeshDirectionTo(Vector3 destination)
        {
            if (gridNavigation == null) return Vector3.zero;

            // Always find a complete cell
            if (hasGridStep) return GetCurrentGridStepDirection();

            Vector2Int targetCell = gridNavigation.WorldToCell(destination);

            if (targetCell == currentGridCell) return Vector3.zero;

            pathBuffer.Clear();

            if (!gridNavigation.TryFindPath(currentGridCell, targetCell, pathBuffer)) return Vector3.zero;
            if (pathBuffer.Count < 2) return Vector3.zero;

            Vector2Int nextCell = pathBuffer[1];
            Vector2Int difference = nextCell - currentGridCell;

            if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
            {
                Debug.LogError($"{name}: A* ha restituito uno step non adiacente {currentGridCell} -> {nextCell}.", this);
                return Vector3.zero;
            }

            if (!gridNavigation.TryGetCellCenter(nextCell, out Vector3 nextCenter)) return Vector3.zero;

            gridStepCell = nextCell;
            gridStepCenter = nextCenter;
            hasGridStep = true;

            return GetCurrentGridStepDirection();
        }

        protected Vector3 GetNavMeshPatrolDirection()
        {
            patrol.UpdateTargetIfReached(transform.position);
            return GetNavMeshDirectionTo(patrol.GetCurrentTargetPosition());
        }

        protected bool IsCenteredOnGrid()
        {
            if (gridNavigation == null || hasGridStep) return false;
            if (!gridNavigation.TryGetCellCenter(currentGridCell, out Vector3 center)) return false;

            return Mathf.Abs(transform.position.x - center.x) <= gridCenterTolerance &&
                   Mathf.Abs(transform.position.z - center.z) <= gridCenterTolerance;
        }

        protected Vector2Int GetCurrentGridCell() => currentGridCell;

        protected Vector2Int GetGridCell(Vector3 worldPosition)
        {
            return gridNavigation != null ? gridNavigation.WorldToCell(worldPosition) : Vector2Int.zero;
        }

        protected bool TryGetGridCellCenter(Vector2Int cell, out Vector3 center)
        {
            center = Vector3.zero;
            return gridNavigation != null && gridNavigation.TryGetCellCenter(cell, out center);
        }

        private void RotateTowardsLookDirection(float deltaTime)
        {
            Vector3 direction = LookDirection;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-rotationSpeed * deltaTime));
        }

        private void HandlePlayerDeathReset()
        {
            if (target == null || !target.HasPlayer()) return;

            if (!target.IsPlayerDead())
            {
                hasResetAfterPlayerDeath = false;
                return;
            }

            if (hasResetAfterPlayerDeath) return;

            ResetToSpawn();
            hasResetAfterPlayerDeath = true;
        }
        protected void ResetToSpawn()
        {
            currentGridCell = spawnGridCell;
            gridStepCell = spawnGridCell;
            gridStepCenter = Vector3.zero;
            hasGridStep = false;

            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = false;

            patrol.Reset();

            MovementDirection = Vector3.zero;
            LookDirection = Vector3.zero;

            OnResetToSpawn();
            SetInitialState();
        }
        protected bool TrySampleNavMeshPosition(Vector3 desiredPosition, out Vector3 sampledPosition)
        {
            return TrySampleNavMeshPosition(desiredPosition, navMeshSampleRadius, out sampledPosition);
        }

        protected bool TrySampleNavMeshPosition(Vector3 desiredPosition, float sampleRadius, out Vector3 sampledPosition)
        {
            sampledPosition = desiredPosition;

            int areaMask = navMeshAgent != null ? navMeshAgent.areaMask : NavMesh.AllAreas;

            if (!NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, sampleRadius, areaMask)) return false;

            sampledPosition = hit.position;
            return true;
        }

        private bool TryPlaceAgentOnNavMesh(Vector3 desiredPosition)
        {
            if (navMeshAgent.isOnNavMesh) return true;
            if (!TrySampleNavMeshPosition(desiredPosition, out Vector3 sampledPosition)) return false;

            return navMeshAgent.Warp(sampledPosition);
        }

        private bool IsNavMeshAgentReady()
        {
            return navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh;
        }

        protected bool HasPlayer() => target.HasPlayer();
        protected bool IsPlayerDead() => target.IsPlayerDead();
        protected float GetDistanceFromPlayer() => target.DistanceFrom(transform.position);
        protected virtual bool IsPlayerInsideDetection() => target.IsInsideDetection(transform.position);
        protected virtual bool IsPlayerOutsideLoseRange() => target.IsOutsideLoseRange(transform.position);
        protected Vector3 GetPlayerDirection() => target.DirectionFrom(transform.position);
        protected Vector3 GetPlayerAimPosition(float verticalOffset = 1f) => target.AimPosition(verticalOffset);

        protected bool TryDamagePlayer(Collider hitCollider, int damage, Vector3 knockbackDirection, bool useKnockback, float knockbackDistance = 0f)
        {
            return EnemyPlayerDamage.TryDamage(hitCollider, damage, knockbackDirection, useKnockback, knockbackDistance);
        }

        protected bool IsOwnCollider(Collider colliderToCheck)
        {
            return colliderToCheck != null && (colliderToCheck.transform == transform || colliderToCheck.transform.IsChildOf(transform));
        }

        protected virtual void OnDrawGizmosSelected()
        {
            target.DrawGizmos(transform.position);
            patrol.DrawGizmos();
        }
    }
}