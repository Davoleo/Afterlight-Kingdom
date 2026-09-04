using Sound;
using UnityEngine;

namespace Enemies
{
    public class EnemySoundFXs : MonoBehaviour
    {
        public AudioClip[] hurts;
        public AudioClip[] deaths;
        public AudioClip[] attacks;
        public AudioClip[] footsteps;
        public AudioClip aggro;

        public void OnEnemyHurt() => AudioManager.Instance.PlayRandomSfx(hurts);

        public void OnEnemyDeath() => AudioManager.Instance.PlayRandomSfx(deaths);

        private void OnFootStep() => AudioManager.Instance.PlayRandomSfx(footsteps);

        private void OnAttack() => AudioManager.Instance.PlayRandomSfx(attacks);

        private void OnAggro()
        {
            if (aggro is not null)
                AudioManager.Instance.PlaySfx(aggro);
        }
    }
}