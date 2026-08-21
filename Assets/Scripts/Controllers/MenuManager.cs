using Core;
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

        private void Start()
        {
            GameStateManager.SetState(GameState.Playing);
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
        }

        private void OnEnable()  => pauseAction.action.performed += OnPausePressed;
        private void OnDisable() => pauseAction.action.performed -= OnPausePressed;

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            if (GameStateManager.Current == GameState.Dead) return; //Can't open pause if player is dead
            if (GameStateManager.Current == GameState.Paused) HidePauseMenu(); else ShowPauseMenu();
        }

        public void ShowDeathScreen()
        {
            deathPanel.SetActive(true);
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Dead);
            deathFirstSelected.Select();
        }

        public void CloseAllMenus()
        {
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Playing);
        }

        private void ShowPauseMenu()
        {
            pausePanel.SetActive(true);
            GameStateManager.SetState(GameState.Paused);
            pauseFirstSelected.Select();
        }

        private void HidePauseMenu()
        {
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Playing);
        }
    }
}
