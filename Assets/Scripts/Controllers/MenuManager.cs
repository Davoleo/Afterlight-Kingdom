using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Controllers
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Death Screen")]
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private Button deathFirstSelected;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseFirstSelected;
        [SerializeField] private InputActionReference pauseAction;

        private bool _isPaused;

        private void Start()
        {
            Time.timeScale = 1f;

            if (deathPanel != null)
                deathPanel.SetActive(false);

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (pauseAction != null)
                pauseAction.action.performed += OnPausePressed;
        }

        private void OnDisable()
        {
            if (pauseAction != null)
                pauseAction.action.performed -= OnPausePressed;
        }

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            if (deathPanel != null && deathPanel.activeSelf)
                return;

            if (_isPaused)
                HidePauseMenu();
            else
                ShowPauseMenu();
        }

        public void ShowDeathScreen()
        {
            if (deathPanel != null)
                deathPanel.SetActive(true);

            if (pausePanel != null)
                pausePanel.SetActive(false);

            _isPaused = false;
            Time.timeScale = 0f;

            if (deathFirstSelected != null)
                deathFirstSelected.Select();
        }

        public void CloseAllMenus()
        {
            if (deathPanel != null)
                deathPanel.SetActive(false);

            if (pausePanel != null)
                pausePanel.SetActive(false);

            _isPaused = false;
            Time.timeScale = 1f;
        }

        private void ShowPauseMenu()
        {
            _isPaused = true;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            Time.timeScale = 0f;

            if (pauseFirstSelected != null)
                pauseFirstSelected.Select();
        }

        private void HidePauseMenu()
        {
            _isPaused = false;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            Time.timeScale = 1f;
        }
    }
}