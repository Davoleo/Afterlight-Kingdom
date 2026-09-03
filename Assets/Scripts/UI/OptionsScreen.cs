using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    // Lives on the root of the Resources/OptionsMenu prefab. Owns the open/close
    // lifecycle: grabs UI focus when shown, and closes on the Back button or the
    // bound cancel/pause input. Persists settings on the way out.
    public class OptionsScreen : MonoBehaviour
    {
        [SerializeField] private OptionsController options;
        [SerializeField] private Selectable firstSelected;
        [SerializeField] private InputActionReference cancelAction;

        private void OnEnable()
        {
            if (firstSelected && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);

            if (cancelAction == null) return;
            cancelAction.action.performed -= OnCancelPressed;
            cancelAction.action.performed += OnCancelPressed;
            cancelAction.action.Enable();
        }

        private void OnDisable()
        {
            if (cancelAction != null)
                cancelAction.action.performed -= OnCancelPressed;
        }

        private void OnCancelPressed(InputAction.CallbackContext _) => Close();

        // Also wired to the Back button's OnClick in the prefab.
        public void Close()
        {
            if (options) options.SaveChanges();
            OptionsMenu.Close();
        }
    }
}
