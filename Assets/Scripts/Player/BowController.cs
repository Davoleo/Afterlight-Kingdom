using Gameplay;
using Projectiles;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Controls the bow mechanics for the player character, including drawing, aiming, and shooting.
    /// Manages the upper body Animator layer weight for smooth blending and procedurally corrects 
    /// spine bone rotations to align mismatched animation assets (Mixamo vs. Kevin Iglesias' Unity Assets).
    /// Handles the logic to bend the bow and the bow string through BowVisualsController.cs.
    /// </summary>
    public class BowController : MonoBehaviour
    {
        [SerializeField] public Transform upperSpineBone;
        [SerializeField] public Vector3 aimOffset = new(0, 90, 0);
        
        // Components
        private Animator _animator;
        private PlayerCharacterController _playerCharacterController;
        private ArrowLauncher _arrowLauncher;
        private int _upperBodyLayerIndex;
        private AbilityManager _abilityManager;
        private BowVisualsController _bowVisuals;

        // Utility variables
        private bool _wasDrawingLastFrame;
        private float _targetWeight;
        private float _currentWeight;
        private const float LayerBlendSpeed = 8f;
        private AnimatorStateInfo _stateInfo;
        private const float BowDrawDuration = 0.1f;
        private const float BowShootDuration = 0.01f;

        // Animator Hashes, the others are stored inside PlayerAnimatorController.cs
        private static readonly int DrawHash = Animator.StringToHash("Draw");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        // Drawn animation hash
        private static readonly int DrawnStateHash = Animator.StringToHash("Drawn");

        public bool IsDrawing { get; private set; }
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _playerCharacterController = GetComponent<PlayerCharacterController>();
            _arrowLauncher = GetComponent<ArrowLauncher>();
            _upperBodyLayerIndex = _animator.GetLayerIndex("Bow");
            _abilityManager = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<AbilityManager>();
            _bowVisuals = GetComponent<BowVisualsController>();
        }
        
        private void LateUpdate()
        {
            float currentLayerWeight = _animator.GetLayerWeight(_upperBodyLayerIndex);
            /* Enforce spine rotation for character rig, necessary because Mixamo and Kevin Iglesias animations
            * have two different base orientations due to software exportation discrepancies.
            * Late update ensures that the rotation occurs after the animator applied its transformations. */
            if (_animator.GetLayerWeight(_upperBodyLayerIndex) > 0.1f)
            {
                // Use the layer weight to smoothly rotate the Upper Spine bone through slerp
                Quaternion targetRotationOffset = Quaternion.Euler(aimOffset);
                Quaternion smoothedOffset = Quaternion.Slerp(Quaternion.identity, targetRotationOffset, currentLayerWeight);
                
                upperSpineBone.localRotation *= smoothedOffset;
            }
            
            _bowVisuals.UpdateBowstring();
        }

        public void Update()
        {
            /* The mask needs to be active whenever an animation from the bow layer is active, checking the
             * draw input is not enough.
             * It is necessary to check whether the shoot animation is occurring and let the mask be active,
             * otherwise the shoot animation will be ignored.*/
            _stateInfo = _animator.GetCurrentAnimatorStateInfo(_upperBodyLayerIndex);
            
            float targetWeight =
                (_playerCharacterController.PlayerInputs.DrawInput || _stateInfo.shortNameHash == ShootHash) ? 1f : 0f;
            
            float currentWeight = _animator.GetLayerWeight(_upperBodyLayerIndex);
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * LayerBlendSpeed);
            
            bool canDraw = _abilityManager.HasAbility(AbilityType.Bow);
            bool wantsToDraw = canDraw && _playerCharacterController.PlayerInputs.DrawInput;

            IsDrawing = wantsToDraw;
            
            switch (wantsToDraw)
            {
                case true when !_wasDrawingLastFrame:
                    _bowVisuals.LoadBow(0f, BowDrawDuration);
                    _bowVisuals.ShowArrow();
                    break;
                // Shoot only when the draw button is released AND the bow is drawn
                case false when _wasDrawingLastFrame:
                {
                    // character needs to fully draw the bow to shoot (wait until the animator enters the drawn state)
                    if (_stateInfo.shortNameHash == DrawnStateHash) Shoot();
                    else
                    {
                        // Released too soon (Cancel)
                        _bowVisuals.CancelLoadBow(0f, 0.1f);
                        _bowVisuals.HideArrow();
                    }

                    break;
                }
            }
            _wasDrawingLastFrame = wantsToDraw;

            _animator.SetBool(DrawHash, wantsToDraw);
            _animator.SetLayerWeight(_upperBodyLayerIndex, newWeight);
        }

        private void Shoot()
        {
            _animator.SetTrigger(ShootHash);
            _arrowLauncher.Shoot(_playerCharacterController.DigitalCharacterForward);
            _bowVisuals.ShootArrow(0f, BowShootDuration);
        }
    }
}