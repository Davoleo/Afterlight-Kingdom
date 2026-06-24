using System.Collections;
using Controllers;
using Core;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(0, 2, 0);

        private GameObject _player;
        private PlayerCharacterController _playerController;
        [SerializeField] private TextMeshProUGUI savedMessage;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerController = _player.GetComponent<PlayerCharacterController>();

            var save = SaveManager.Load();
            if (save == null) return;
            lastCheckPoint = new Vector3(save.checkpointX, save.checkpointY, save.checkpointZ);
            Respawn();
        }

        public void Respawn()
        {
            _playerController.motor.SetPosition(lastCheckPoint);
        }
        
        public void ShowSavedMessage() => StartCoroutine(FadeSavedMessage());
        
        private IEnumerator FadeSavedMessage()
        {
            // Fade In
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                savedMessage.alpha = Mathf.Clamp01(t);
                yield return null;
            }
            
            // Hold
            yield return new WaitForSeconds(1.5f);
            
            // Fade Out
            t = 1f;
            while (t > 0f)
            {
                t -= Time.deltaTime / 0.5f;
                savedMessage.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }
    }
}