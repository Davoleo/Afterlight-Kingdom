using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
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

        private void OnEnable()
        {
            pauseAction.action.performed -= OnPausePressed;
            pauseAction.action.performed += OnPausePressed;
        }

        private void OnDisable() => pauseAction.action.performed -= OnPausePressed;

        private void OnPausePressed(InputAction.CallbackContext _)
        {
            // While the options UI is up, the pause key backs out of it instead.
            if (OptionsMenu.IsOpen)
            {
                OptionsMenu.Close();
                return;
            }

            // Can't pause while loading, and can't open pause if the player is dead
            if (GameStateManager.Current == GameState.Paused) HidePauseMenu();
            else if (GameStateManager.Current == GameState.Playing) ShowPauseMenu();
        }

        // Wired to the pause menu's Options button. Hands off to the shared options UI
        // and brings the pause menu back when it closes; the game stays paused throughout.
        public void OpenOptions()
        {
            pausePanel.SetActive(false);
            OptionsMenu.Open(onClosed: OnOptionsClosed);
        }

        private void OnOptionsClosed()
        {
            if (GameStateManager.Current != GameState.Paused) return;
            pausePanel.SetActive(true);
            if (pauseFirstSelected) pauseFirstSelected.Select();
        }

        public void ShowDeathScreen()
        {
            deathPanel.SetActive(true);
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Dead);
            if (deathFirstSelected) deathFirstSelected.Select();
        }

        public void CloseAllMenus()
        {
            OptionsMenu.Close();
            deathPanel.SetActive(false);
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Playing);
        }

        private void ShowPauseMenu()
        {
            pausePanel.SetActive(true);
            GameStateManager.SetState(GameState.Paused);
            if (pauseFirstSelected) pauseFirstSelected.Select();
        }

        private void HidePauseMenu()
        {
            pausePanel.SetActive(false);
            GameStateManager.SetState(GameState.Playing);
        }
    }
}
