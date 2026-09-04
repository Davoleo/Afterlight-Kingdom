using Core;
using Gameplay;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MenuActions : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        private GameObject _optionsPanel;
        private CheckpointManager _cpManager;
        private MenuManager _menuManager;
        private HealthManager _healthManager;
        
        private void Start()
        {
            if (!gameObject.CompareTag("GameManager")) return;
            _cpManager = gameObject.GetComponent<CheckpointManager>();
            _menuManager = gameObject.GetComponent<MenuManager>();

            var player = GameObject.FindGameObjectWithTag("Player");
            _healthManager = player.GetComponent<HealthManager>();
        }

        private void Update()
        {
            if (mainPanel && !_optionsPanel)
            {
                mainPanel.SetActive(true);
            }
        }

        public void RestartFromCheckpoint()
        {
            _cpManager.Respawn();
            _healthManager.ResetAfterRespawn();
            _menuManager.CloseAllMenus(); // resumes play (sets GameState back to Playing)
        }

        // Opens the shared options UI over the current menu and restores button focus
        // when it closes. Used by the main menu; the in-game pause menu routes through
        // MenuManager.OpenOptions instead so it can hide/restore the pause panel.
        public void OpenOptions()
        {
            var previous = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            _optionsPanel = OptionsMenu.Open(onClosed: () =>
            {
                if (previous && EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(previous);
            });
            mainPanel.SetActive(false);
        }

        public void ReturnToMenu()
        {
            GameStateManager.SetState(GameState.Playing);
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        public void NewGame()
        {
            SaveManager.Delete();
            LoadingScreen.Instance.StartCoroutine(SceneTransitions.EnterGame());
        }

        public void Continue()
        {
            LoadingScreen.Instance.StartCoroutine(SceneTransitions.EnterGame());
        }

        public void QuitGame()
        {
            SceneLoader.QuitGame();
        }
    }
}