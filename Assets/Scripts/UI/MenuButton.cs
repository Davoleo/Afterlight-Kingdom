using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // Button that also recolours its label per interaction state, on top of the
    // normal Target Graphic colour tint. The label is the button's child TMP text
    // (resolved automatically); the two colours are the fixed menu palette.
    public class MenuButton : Button
    {
        // F3E5C8 at rest / disabled, 261A15 when highlighted, pressed or selected.
        private static readonly Color NormalTextColor = new Color32(0xF3, 0xE5, 0xC8, 0xFF);
        private static readonly Color ActiveTextColor = new Color32(0x26, 0x1A, 0x15, 0xFF);

        private Graphic _label;
        
        private Graphic Label => _label != null
            ? _label
            : _label = GetComponentInChildren<TMP_Text>(true);

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (Label == null) return;

            Color target = state is SelectionState.Normal or SelectionState.Disabled
                ? NormalTextColor
                : ActiveTextColor;

            Label.CrossFadeColor(target, instant ? 0f : colors.fadeDuration, true, true);
        }
    }
}
