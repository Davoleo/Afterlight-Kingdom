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
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
        }

        private void OnEnable()  => pauseAction.action.performed += OnPausePressed;
        private void OnDisable() => pauseAction.action.performed -= OnPausePressed;

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            if (deathPanel.activeSelf) return; //Can't open pause if player is dead
            if (_isPaused) HidePauseMenu(); else ShowPauseMenu();
        }

        public void ShowDeathScreen()
        {
            deathPanel.SetActive(true);
            pausePanel.SetActive(false);
            _isPaused = false;
            Time.timeScale = 0f;
            deathFirstSelected.Select();
        }

        public void CloseAllMenus()
        {
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1f;
        }

        private void ShowPauseMenu()
        {
            _isPaused = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            pauseFirstSelected.Select();
        }

        private void HidePauseMenu()
        {
            _isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
