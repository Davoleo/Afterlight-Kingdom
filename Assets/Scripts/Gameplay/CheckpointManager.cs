using System.Collections;
using Controllers;
using Core;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class CheckpointManager : MonoBehaviour
    {
        public Vector3 lastCheckPoint = new Vector3(0f, 2f, 0f);

        [SerializeField] private TextMeshProUGUI savedMessage;

        private GameObject _player;
        private PlayerCharacterController _playerController;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null)
                _playerController = _player.GetComponent<PlayerCharacterController>();

            SaveData save = SaveManager.Load();

            if (save == null)
                return;

            lastCheckPoint = new Vector3(
                save.checkpointX,
                save.checkpointY,
                save.checkpointZ
            );

            MovePlayerToCheckpoint();
        }

        private void MovePlayerToCheckpoint()
        {
            if (_playerController == null)
                return;

            _playerController.StopExternalKnockback();

            if (_playerController.motor != null)
                _playerController.motor.SetPosition(lastCheckPoint);
            else
                _player.transform.position = lastCheckPoint;
        }

        public void ShowSavedMessage()
        {
            if (savedMessage != null)
                StartCoroutine(FadeSavedMessage());
        }

        private IEnumerator FadeSavedMessage()
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                savedMessage.alpha = Mathf.Clamp01(t);
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

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