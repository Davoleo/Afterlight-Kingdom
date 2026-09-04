using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        private GameObject _continueButton;

        private void Start()
        {
            ShowMainPanel();
            _continueButton = GameObject.Find("ContinueButton");
            _continueButton.SetActive(SaveManager.HasSave);
        }

        private void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            // Select the first button in the panel
            EventSystem.current.SetSelectedGameObject(mainPanel.GetComponentInChildren<Button>().gameObject);
        }
    }
}
