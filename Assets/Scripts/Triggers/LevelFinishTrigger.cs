using Gameplay;
using TMPro;
using UnityEngine;

namespace Triggers
{
    [RequireComponent(typeof(Collider))]
    public class LevelFinishTrigger : MonoBehaviour
    {
        private CollectiblesManager _collectibles;
        private TextMeshProUGUI _messageText;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
            _collectibles = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CollectiblesManager>();

            var canvas = GameObject.Find("MainUIOverlay");
            _messageText = CreateLabel(canvas.transform);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            _messageText.text = $"Finished!\nCoins collected: {_collectibles.Coins} / {_collectibles.Collectibles.Coins.Count}";
            _messageText.gameObject.SetActive(true);
        }

        private static TextMeshProUGUI CreateLabel(Transform canvasRoot)
        {
            var go = new GameObject("FinishLabel");
            go.transform.SetParent(canvasRoot, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 200f);
            rect.anchoredPosition = Vector2.zero;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize  = 48f;

            go.SetActive(false);
            return text;
        }
    }
}
