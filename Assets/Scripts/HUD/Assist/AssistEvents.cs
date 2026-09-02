using Player.State;
using Projectiles;
using UnityEngine;

namespace HUD.Assist
{
    public class AssistEvents : MonoBehaviour
    {
        public FeatureAssistData jumpFeature;
        public FeatureAssistData shootFeature;
        public FeatureAssistData dashFeature;

        private void Start()
        {
            PlayerState.OnJumped += OnPlayerJump;
            ArrowLauncher.OnShoot += OnPlayerShoot;
        }

        private void OnPlayerJump()
        {
            TutorialAssistManager.I.DisableFeatureAssist(jumpFeature);
        }

        private void OnPlayerShoot()
        {
            TutorialAssistManager.I.DisableFeatureAssist(shootFeature);
        }

        public void OnPlayerDash()
        {
            TutorialAssistManager.I.DisableFeatureAssist(dashFeature);
        }
    }
}