using System.Collections;
using UnityEngine;

namespace Player
{

    /// <summary>
    /// Genera "ghost" (snapshot sfumati) del personaggio mentre è in DashingState,
    /// usando il materiale magico blu (SH_DashMagicTrail) per l'effetto scia.
    /// Da agganciare sullo stesso GameObject del PlayerCharacterController.
    /// </summary>
    public class GhostTrailSpawner : MonoBehaviour
    {
        // Shader blackboard alpha property name (look inside shader graph).
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        [Header("References")]
        [Tooltip("Skinned Mesh Renderer to copy")]
        public SkinnedMeshRenderer meshRenderer;
        
        [Tooltip("Material with Ghost Trail shader")]
        public Material ghostMaterial;

        [Header("Trail Settings")]
        [Tooltip("Spawn interval between two ghosts (in seconds)")]
        public float spawnInterval = 0.05f;

        [Tooltip("Life time of a ghost before vanishing (in seconds)")]
        public float ghostLifetime = 0.4f;
        
        private PlayerCharacterController _characterController;

        private const float SpawnAlpha = 1f;
        
        private bool _wasDashing;

        private void Start()
        {
            _characterController = GetComponent<PlayerCharacterController>();
        }

        private void Update()
        {
            if (!_characterController) return;

            bool isDashing = _characterController.StateMachine.CurrentState
                              == _characterController.StateMachine.DashingState;

            // If first dash frame.
            if (isDashing && !_wasDashing)
            {
                StartCoroutine(SpawnGhostsWhileDashing());
            }

            _wasDashing = isDashing;
        }

        private IEnumerator SpawnGhostsWhileDashing()
        {
            // Continue spawning until the dash state lasts.
            while (_characterController.StateMachine.CurrentState
                   == _characterController.StateMachine.DashingState)
            {
                SpawnGhost();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnGhost()
        {
            // Make a snapshot of the mesh in this frame.
            Mesh snapshot = new Mesh();
            this.meshRenderer.BakeMesh(snapshot);
            
            // Create the ghost GameObject and set it in the same position of the snapshot.
            GameObject ghost = new GameObject("DashGhost");
            ghost.transform.SetPositionAndRotation(
                this.meshRenderer.transform.position,
                this.meshRenderer.transform.rotation);

            // Set the mesh to the GameObject.
            MeshFilter filter = ghost.AddComponent<MeshFilter>();
            filter.mesh = snapshot;
            
            // Instantiate the shader material to the GameObject.
            MeshRenderer meshRenderer = ghost.AddComponent<MeshRenderer>();
            Material ghostMaterial = new Material(this.ghostMaterial);
            ghostMaterial.SetFloat(Alpha, SpawnAlpha);
            meshRenderer.material = ghostMaterial;

            StartCoroutine(FadeAndDestroy(ghost, ghostMaterial));
        }

        private IEnumerator FadeAndDestroy(GameObject ghost, Material ghostMaterial)
        {
            float elapsed = 0f;

            while (elapsed < ghostLifetime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(SpawnAlpha, 0f, elapsed / ghostLifetime);
                ghostMaterial.SetFloat(Alpha, alpha);
                yield return null;
            }

            Destroy(ghost);
            Destroy(ghostMaterial); // Avoid memory leak: material instances are not destroyed automatically.
        }
    }
}