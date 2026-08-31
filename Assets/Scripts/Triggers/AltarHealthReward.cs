using System.Collections;
using UnityEngine;

namespace Triggers
{
    /// <summary>
    /// Coin-offering altar. Same system as chest ability reward.
    /// </summary>
    public class AltarHealthReward : ChestAbilityReward
    {

        [Header("Cost")] [SerializeField] private int coinCost = 40;

        [Header("Coin Animation")]
        [SerializeField] private GameObject coinVisualPrefab;
        [SerializeField] [Range(1, 10)] private int coinVisualCount = 4;
        [SerializeField] private float coinStartHeight = 1.2f;
        [SerializeField] private float coinAcceptScaleMult = 1.4f;

        protected override bool CanActivate() => Collectibles.Coins >= coinCost;

        private void ActivationPrice() => Collectibles.SpendCoins(coinCost);

        protected override void PlayActivationEffect()
        {
            ActivationPrice();
            StartCoroutine(CoinSlideRoutine());
            Opened = true;
        }

        private IEnumerator CoinSlideRoutine()
        {
            if (!coinVisualPrefab || !rewardSpawnPoint || !CharacterController)
                yield break;

            Vector3 endPosition = rewardSpawnPoint.position + Vector3.up * 0.3f;

            var coins = new Transform[coinVisualCount];
            var startDelays = new float[coinVisualCount];
            var startPosition = CharacterController.transform.position + Vector3.up *  coinStartHeight;

            for (int i = 0; i < coins.Length; i++)
            {
                startDelays[i] = rewardSpawnDelay / coinVisualCount * (i*0.4f);
                GameObject coin = Instantiate(coinVisualPrefab, startPosition, Quaternion.identity);
                coins[i] = coin.transform;
            }

            float elapsed = 0f;

            while (elapsed < rewardSpawnDelay)
            {
                elapsed += Time.deltaTime;

                for (int i = 0; i < coins.Length; i++)
                {
                    if (!coins[i]) continue;

                    float localElapsed = Mathf.Max(0f, elapsed - startDelays[i]);
                    float duration = Mathf.Max(0f, rewardSpawnDelay - startDelays[i]);

                    float t = Mathf.Clamp01(localElapsed / duration);
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);

                    coins[i].position = Vector3.Lerp(startPosition, endPosition, smoothT);

                    //lerp scale in the last 30% of animation
                    float scaleT = Mathf.Clamp01((t - 0.70f) / 0.30f);
                    coins[i].localScale = Vector3.one * Mathf.Lerp(1f, coinAcceptScaleMult, scaleT);
                }
                Debug.Log(coins[0].localScale);

                yield return null;
            }

            foreach (var coin in coins)
            {
                if (coin) Destroy(coin.gameObject);
            }
        }


    }
}