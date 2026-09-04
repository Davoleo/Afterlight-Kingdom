using System.Collections;
using Gameplay;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    // Drives the throne-room ending: fade to black, seat the player, pull the camera back
    // for three seconds, fade out again and reveal the coin tally followed by credits and a
    // thanks-for-playing message, then wait for Interact to clear the save and return to the
    // main menu. Runs entirely on unscaled time since GameStateManager.SetState(Cutscene)
    // drops Time.timeScale to 0, same reasoning as LoadingScreen's fades running during
    // GameState.Loading.
    public class EndGameSequence : MonoBehaviour
    {
        [Header("Fade")]
        [SerializeField] private Canvas endGameCanvas;
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private float fadeDuration = 1f;

        [Header("Seat")]
        [SerializeField] private Transform seatPoint;

        [Header("Camera")]
        [SerializeField] private Camera cutsceneCamera;
        [SerializeField] private Transform closeShotPoint;
        [SerializeField] private Transform farShotPoint;
        [SerializeField] private float panDuration = 3f;

        [Header("Results")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text resultsText;
        [SerializeField] private int totalCoins;
        [SerializeField] private CanvasGroup creditsGroup;
        [SerializeField] private CanvasGroup thanksGroup;
        [SerializeField] private CanvasGroup exitGroup;
        [SerializeField] private float resultsStepDelay = 1f;

        [Header("Input")]
        [SerializeField] private InputActionReference continueAction;

        private static readonly int SitDownHash = Animator.StringToHash("SitDown");

        private bool _continuePressed;

        public void Play() => StartCoroutine(Sequence());

        private IEnumerator Sequence()
        {
            GameStateManager.SetState(GameState.Cutscene);
            
            // The canvas GameObject stays active for the whole level (same convention as
            // LoadingScreen) - only its Canvas component is toggled, so this MonoBehaviour's
            // own coroutine can't be short-circuited by a disabled hierarchy.
            endGameCanvas.enabled = true;
            resultsPanel.SetActive(false);

            fadeOverlay.blocksRaycasts = true;
            yield return Fade(fadeOverlay, 0f, 1f);

            SeatPlayer();
            SwapToCutsceneCamera();

            // The pull-back starts the moment the reveal starts, not once it finishes -
            // otherwise the camera just sits at the close shot for the whole fade-in before
            // any panning begins, which reads as a stray pause. Fade and pan run side by
            // side; the pan owns the three-second cutscene beat, so wait on it last.
            Coroutine pan = StartCoroutine(PanCameraBack());
            yield return Fade(fadeOverlay, 1f, 0f);
            yield return pan;

            yield return Fade(fadeOverlay, 0f, 1f);

            yield return ShowResults();
            yield return WaitForContinue();

            SaveManager.Delete();
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        private void SeatPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject gm = GameObject.FindGameObjectWithTag("GameManager");
            var characterController = player.GetComponent<PlayerCharacterController>();
            var abilityManager = gm.GetComponent<AbilityManager>();
            
            characterController.motor.SetPositionAndRotation(seatPoint.position, seatPoint.rotation);
            abilityManager.SheatheBow();
            
            var animator = player.GetComponentInChildren<Animator>();
            
            // GameState.Cutscene drops Time.timeScale to 0; the Animator's default Update
            // Mode advances on scaled time, so without this the SitDown transition would
            // fire but the clip would never actually play - same reasoning as this script's
            // own coroutines running on Time.unscaledDeltaTime.
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger(SitDownHash);
        }

        private void SwapToCutsceneCamera()
        {
            GameObject.FindWithTag("MainCamera").gameObject.SetActive(false);

            Transform cam = cutsceneCamera.transform;
            cam.position = closeShotPoint.position;
            cam.rotation = LookAtSeat(cam.position);

            cutsceneCamera.gameObject.SetActive(true);
        }

        private IEnumerator PanCameraBack()
        {
            Transform cam = cutsceneCamera.transform;
            float t = 0f;

            while (t < panDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerpT = Mathf.Clamp01(t / panDuration);

                cam.position = Vector3.Lerp(closeShotPoint.position, farShotPoint.position, lerpT);
                cam.rotation = LookAtSeat(cam.position);

                yield return null;
            }
        }

        // closeShotPoint/farShotPoint are placed as pure position markers - the camera always
        // derives its facing from the seat point instead of trusting their authored rotation,
        // so it never shows a stray orientation before the pan's own per-frame LookRotation
        // takes over (which is what a marker's raw rotation used to leave on screen).
        private Quaternion LookAtSeat(Vector3 fromPosition) =>
            Quaternion.LookRotation(seatPoint.position - fromPosition);

        // Coins appear immediately with the panel; Credits, Thanks for Playing and the exit
        // prompt then fade in one after another on the "B Normal" button containers, each
        // held back by resultsStepDelay so the reveal reads as a beat rather than a wall of text.
        private IEnumerator ShowResults()
        {
            int collected = GameObject.FindGameObjectWithTag("GameManager")
                .GetComponent<CollectiblesManager>()
                .GetCount(CollectibleType.Coin);

            resultsText.text = $"Coins collected: {collected} / {totalCoins}";
            creditsGroup.alpha = 0f;
            thanksGroup.alpha = 0f;
            exitGroup.alpha = 0f;
            resultsPanel.SetActive(true);

            yield return new WaitForSecondsRealtime(resultsStepDelay);
            yield return Fade(creditsGroup, 0f, 1f);

            yield return new WaitForSecondsRealtime(resultsStepDelay);
            yield return Fade(thanksGroup, 0f, 1f);

            yield return new WaitForSecondsRealtime(resultsStepDelay);
            yield return Fade(exitGroup, 0f, 1f);
        }

        private IEnumerator WaitForContinue()
        {
            _continuePressed = false;
            continueAction.action.performed += OnContinuePressed;
            continueAction.action.Enable();

            while (!_continuePressed)
                yield return null;

            continueAction.action.performed -= OnContinuePressed;
            continueAction.action.Disable();
        }

        private void OnContinuePressed(InputAction.CallbackContext _) => _continuePressed = true;

        private IEnumerator Fade(CanvasGroup group, float from, float to)
        {
            group.alpha = from;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }

            group.alpha = to;
        }


    }
}
