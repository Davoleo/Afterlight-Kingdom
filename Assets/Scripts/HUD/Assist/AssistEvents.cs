using Player.State;
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
            PlayerState.OnShoot += OnPlayerShoot;
        }

        private void OnPlayerJump()
        {
            TutorialAssistManager.Instance.DisableFeatureAssist(jumpFeature);
        }

        private void OnPlayerShoot()
        {
            TutorialAssistManager.Instance.DisableFeatureAssist(shootFeature);
        }

        public void OnPlayerDash()
        {
            TutorialAssistManager.Instance.DisableFeatureAssist(dashFeature);
        }
    }
}