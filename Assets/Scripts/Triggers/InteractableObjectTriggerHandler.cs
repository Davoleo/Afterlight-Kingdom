using HUD.Assist;
using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Triggers
{
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private string textToDisplay = "Press {Interact} to interact";
        [SerializeField] private bool canInteractForever;
        [SerializeField] private UnityEvent onInteract; 
        
        private GameObject _popup;
        private FadingPrompt _popupFading;
        private PlayerCharacterController _controller;
        private bool _playerClose;
        private bool _alreadyInteracted;

        private void Start()
        {
            _popup = GameObject.FindWithTag("InteractPopup");
            _popupFading = _popup.GetComponentInChildren<FadingPrompt>();
            _controller = GameObject.FindWithTag("Player").GetComponent<PlayerCharacterController>();
        }

        private void Update()
        {
            if (!_playerClose || (_alreadyInteracted && !canInteractForever)) return;
            
            _popupFading.ShowHeld(textToDisplay);
            
            if (!CommandUtils.IsUp(_controller.triggers, PlayerTrigger.Interact)) return;
            
            onInteract.Invoke();
            _popupFading.Hide();
            _alreadyInteracted = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerClose = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            _playerClose = false;
            _popupFading.Hide();
        }
    }
}