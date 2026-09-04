using Gameplay;
using HUD.Assist;
using Sound;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class AbilityRewardClaimHandler : MonoBehaviour
    {
        [SerializeField] private FeatureAssistData dashTutorial;
        
        [Header("Reward")]
        [SerializeField] private AbilityType abilityToUnlock = AbilityType.Bow;

        [Header("Rotation")]
        [SerializeField] private bool rotate = true;
        [SerializeField] private float rotationSpeed = 120f;

        private AudioClip _sfx;
        private AbilityManager _abilityManager;

        private void Start()
        {
            var gameManager = GameObject.FindGameObjectWithTag("GameManager");
            _abilityManager = gameManager.GetComponent<AbilityManager>();
            
            if (_abilityManager.HasAbility(abilityToUnlock))
                gameObject.SetActive(false);

            _sfx = Resources.Load<AudioClip>("Sound/powerup");
        }

        private void Update()
        {
            if (rotate)
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        
        public void ClaimReward()
        {
            _abilityManager.UnlockAbility(abilityToUnlock);
            AudioManager.Instance.PlaySfx(_sfx);
            gameObject.SetActive(false);

            if (abilityToUnlock == AbilityType.Dash)
            {
                TutorialAssistManager.I.ShowAssist(dashTutorial);
            }
        }
    }
}
