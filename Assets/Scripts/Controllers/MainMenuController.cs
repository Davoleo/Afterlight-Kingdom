using UnityEngine;

namespace Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        void Start() => ShowMainPanel();

        public void OnOptionsPressed() => ShowOptionsPanel();
        public void OnBackPressed()    => ShowMainPanel();

        private void ShowMainPanel()    { mainPanel.SetActive(true);    optionsPanel.SetActive(false); }
        private void ShowOptionsPanel() { optionsPanel.SetActive(true); mainPanel.SetActive(false);    }
    }
}
