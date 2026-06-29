using System.Collections;
using UnityEngine;

namespace Gameplay
{
    public enum TrapdoorSide
    {
        Left,
        Right,
        Front,
        Back
    }

    /// Trapdoor platform that creates its hinge automatically on the selected side.
    public class BreakablePlatform : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform platformPart;
        [SerializeField] private BoxCollider platformCollider;

        [Header("Timing")]
        [SerializeField] private float openDelay = 0.35f;
        [SerializeField] private float openDuration = 0.2f;
        [SerializeField] private float stayOpenTime = 1.2f;
        [SerializeField] private float closeDuration = 1.5f;

        [Header("Trapdoor")]
        [SerializeField] private TrapdoorSide hingeSide = TrapdoorSide.Left;
        [SerializeField, Range(0f, 180f)] private float openAngle = 90f;

        [Header("Collision")]
        [SerializeField] private string playerTag = "Player";

        private Transform _hingePivot;
        private Quaternion _closedRotation;
        private Quaternion _openedRotation;
        private Collider[] _colliders;
        private bool _isRunning;

        private void Start()
        {
            _colliders = platformPart.GetComponentsInChildren<Collider>();

            CreateHingePivot();
            RegisterCollisionRelays();

            _closedRotation = _hingePivot.rotation;
            _openedRotation = GetDownwardOpenRotation();
        }

        private void OnValidate()
        {
            openDelay = Mathf.Max(0f, openDelay);
            openDuration = Mathf.Max(0.01f, openDuration);
            stayOpenTime = Mathf.Max(0f, stayOpenTime);
            closeDuration = Mathf.Max(0.01f, closeDuration);
        }

        private void CreateHingePivot()
        {
            Vector3 hingePosition = GetHingePosition();

            GameObject pivotObject = new GameObject($"{name}_HingePivot");
            _hingePivot = pivotObject.transform;

            _hingePivot.position = hingePosition;
            _hingePivot.rotation = platformPart.rotation;
            _hingePivot.SetParent(transform, true);

            platformPart.SetParent(_hingePivot, true);
        }

        private void RegisterCollisionRelays()
        {
            foreach (Collider platformCollider in _colliders)
            {
                TrapdoorCollisionRelay relay = platformCollider.gameObject.AddComponent<TrapdoorCollisionRelay>();
                relay.Initialize(this);
            }
        }

        public void HandleCollision(Collision collision)
        {
            if (_isRunning)
                return;

            if (!collision.collider.CompareTag(playerTag))
                return;

            StartCoroutine(TrapdoorRoutine());
        }

        private IEnumerator TrapdoorRoutine()
        {
            _isRunning = true;

            yield return new WaitForSeconds(openDelay);

            SetCollision(false);

            yield return RotateHinge(_closedRotation, _openedRotation, openDuration);
            yield return new WaitForSeconds(stayOpenTime);
            yield return RotateHinge(_openedRotation, _closedRotation, closeDuration);

            _hingePivot.rotation = _closedRotation;
            SetCollision(true);

            _isRunning = false;
        }

        private IEnumerator RotateHinge(Quaternion from, Quaternion to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                _hingePivot.rotation = Quaternion.Slerp(from, to, t);

                yield return null;
            }

            _hingePivot.rotation = to;
        }

        private void SetCollision(bool enabled)
        {
            foreach (Collider platformCollider in _colliders)
                platformCollider.enabled = enabled;
        }

        private Quaternion GetDownwardOpenRotation()
        {
            Vector3 axis = GetHingeAxis();
            Vector3 center = platformCollider.bounds.center;
            Vector3 hinge = _hingePivot.position;

            Quaternion positiveRotation = Quaternion.AngleAxis(openAngle, axis);
            Quaternion negativeRotation = Quaternion.AngleAxis(-openAngle, axis);

            Vector3 positiveCenter = hinge + positiveRotation * (center - hinge);
            Vector3 negativeCenter = hinge + negativeRotation * (center - hinge);

            Quaternion chosenRotation = positiveCenter.y < negativeCenter.y
                ? positiveRotation
                : negativeRotation;

            return chosenRotation * _closedRotation;
        }

        private Vector3 GetHingePosition()
        {
            Vector3 localOffset = platformCollider.center;

            switch (hingeSide)
            {
                case TrapdoorSide.Left:
                    localOffset.x -= platformCollider.size.x * 0.5f;
                    break;

                case TrapdoorSide.Right:
                    localOffset.x += platformCollider.size.x * 0.5f;
                    break;

                case TrapdoorSide.Front:
                    localOffset.z += platformCollider.size.z * 0.5f;
                    break;

                case TrapdoorSide.Back:
                    localOffset.z -= platformCollider.size.z * 0.5f;
                    break;
            }

            return platformCollider.transform.TransformPoint(localOffset);
        }

        private Vector3 GetHingeAxis()
        {
            return hingeSide switch
            {
                TrapdoorSide.Left => platformCollider.transform.forward,
                TrapdoorSide.Right => platformCollider.transform.forward,
                TrapdoorSide.Front => platformCollider.transform.right,
                TrapdoorSide.Back => platformCollider.transform.right,
                _ => platformCollider.transform.right
            };
        }
    }

    public class TrapdoorCollisionRelay : MonoBehaviour
    {
        private BreakablePlatform _trapdoor;

        public void Initialize(BreakablePlatform trapdoor)
        {
            _trapdoor = trapdoor;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _trapdoor?.HandleCollision(collision);
        }
    }
}