using Sound;
using UnityEngine;

namespace Enemies
{
    public class EnemySoundFXs : MonoBehaviour
    {
        public AudioClip[] hurts;
        public AudioClip[] deaths;
        public AudioClip[] attacks;

        public void OnEnemyHurt() => AudioManager.Instance.PlayRandomSfx(hurts);

        public void OnEnemyDeath() => AudioManager.Instance.PlayRandomSfx(deaths);

        private void OnFootStep() { }

        private void OnAttack() => AudioManager.Instance.PlayRandomSfx(attacks);
    }
}