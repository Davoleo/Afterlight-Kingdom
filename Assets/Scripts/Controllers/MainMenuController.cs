using Core;
using UnityEngine;

namespace Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ShowMainPanel();
        }
        // Called from Menu buttons through OnClick()
        public void OnPlayPressed()   => SceneLoader.LoadScene("MainScene");
        public void OnOptionsPressed() => ShowOptionsPanel();
        public void OnQuitPressed()   => SceneLoader.QuitGame();
        public void OnBackPressed()   => ShowMainPanel();

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }

        private void ShowOptionsPanel()
        {
            optionsPanel.SetActive(true);
            mainPanel.SetActive(false);
        }
    }
}

