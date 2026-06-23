using Core;
using Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Controllers
{
    public class DeathScreenController :MonoBehaviour
    {
        [SerializeField] private Button firstSelected;
        private GameObject _deathPanel;
        private CheckpointManager _cpManager;

        private void OnEnable()
        {
            // Make the selected button on hover to quickly press it after death
            firstSelected.Select();
        }

        private void Start()
        {
            _deathPanel = GameObject.Find("DeathPanel");
            _cpManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CheckpointManager>();
            _deathPanel.SetActive(false);
        }

        public void ShowDeathScreen()
        {
            _deathPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void RestartFromCheckpoint()
        {
            Time.timeScale = 1f;
            _deathPanel.SetActive(false);
            _cpManager.Respawn();
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneLoader.LoadScene(0);
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}