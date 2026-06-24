using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        private GameObject _continueButton;

        private void Start()
        {
            ShowMainPanel();
            _continueButton = GameObject.Find("ContinueButton");
            _continueButton.SetActive(SaveManager.HasSave);
        }

        public void OnOptionsPressed() => ShowOptionsPanel();
        public void OnBackPressed() => ShowMainPanel();

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true); 
            optionsPanel.SetActive(false);
            // Select the first button in the panel                                                                                                                                   
            EventSystem.current.SetSelectedGameObject(mainPanel.GetComponentInChildren<Button>().gameObject);
        }

        private void ShowOptionsPanel()
        {
            optionsPanel.SetActive(true);
            mainPanel.SetActive(false);
            // Select the first button in the panel                                                                                                                                   
            EventSystem.current.SetSelectedGameObject(mainPanel.GetComponentInChildren<Button>().gameObject);
        }
    }
}
