using Gameplay;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AbilityRewardClaimHandler : MonoBehaviour
    {
        [Header("Reward")]
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Bow;

        [Header("Rotation")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotationSpeed = 120f;
        
        private AbilityManager _abilityManager;
        
        private void Start()
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _abilityManager = gameManager.GetComponent<AbilityManager>();
            
            if (_abilityManager.HasAbility(abilityToUnlock))
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (rotate)
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        public void ClaimReward()
        {
            _abilityManager.UnlockAbility(abilityToUnlock);
            gameObject.SetActive(false);
        }
    }
}
