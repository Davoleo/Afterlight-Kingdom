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
            // GameState starts (and returns to) Loading/Playing under CoreLoader's control -
            // this only needs to make sure no leftover panel is showing.
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
        }

        private void OnEnable()  => pauseAction.action.performed += OnPausePressed;
        private void OnDisable() => pauseAction.action.performed -= OnPausePressed;

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            // Can't pause while loading, and can't open pause if the player is dead
            if (GameStateManager.Current == GameState.Paused) HidePauseMenu();
            else if (GameStateManager.Current == GameState.Playing) ShowPauseMenu();
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
