using System.Collections;
using UnityEngine;

namespace Player
{
    public class BowVisualsController : MonoBehaviour
    {
        [Header("BOW")]
        public LineRenderer bowstringLine;
        
        public Transform limb01;
        public Transform limb02;
        
        public Transform tip01;
        public Transform tip02;
        public Transform nockPoint;
        
        public Transform bowstringAnchorPoint; 
        
        public AnimationCurve bowReleaseCurve;

        [Header("Bow Flexing")]
        public Vector3 limb01FlexOffset = new Vector3(0, 0, -15f);
        public Vector3 limb02FlexOffset = new Vector3(0, 0, -15f);
        
        [Header("ARROW VISUALS")]
        [Tooltip("The arrow to hold in character's hand during animations")]
        public GameObject arrowInHand;
        
        private Vector3 _nockPointRestLocalPosition;
        private Vector3 _initialLimb01LocalEulerAngles;
        private Vector3 _initialLimb02LocalEulerAngles;
        
        private IEnumerator _bowAnimation;

        private void OnEnable()
        {
            if(nockPoint) _nockPointRestLocalPosition = nockPoint.localPosition;

            if (limb01 && limb02) 
            {
                _initialLimb01LocalEulerAngles = limb01.localEulerAngles;
                _initialLimb02LocalEulerAngles = limb02.localEulerAngles;
            }
            
            HideArrow();
        }

        // Called by BowController.LateUpdate() to synchronize the bow string with the spine transformation
        public void UpdateBowstring()
        {
            if(!bowstringLine || !tip01 || !tip02 || !nockPoint) return;
            
            bowstringLine.positionCount = 3;
            bowstringLine.SetPosition(0, tip01.position);
            bowstringLine.SetPosition(1, nockPoint.position);
            bowstringLine.SetPosition(2, tip02.position);
        }

        public void LoadBow(float delay, float duration)
        {
            if(_bowAnimation != null) StopCoroutine(_bowAnimation);
            _bowAnimation = LoadBowCoroutine(delay, duration);
            StartCoroutine(_bowAnimation);
        }

        public void CancelLoadBow(float delay, float cancelDuration)
        {
            if(_bowAnimation != null) StopCoroutine(_bowAnimation);
            _bowAnimation = CancelLoadBowCoroutine(delay, cancelDuration);
            StartCoroutine(_bowAnimation);
        }
        
        private IEnumerator LoadBowCoroutine(float delay, float duration)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            
            Vector3 limb01LoadLocalEulerAngles = _initialLimb01LocalEulerAngles + limb01FlexOffset;
            Vector3 limb02LoadLocalEulerAngles = _initialLimb02LocalEulerAngles + limb02FlexOffset;
            
            float t = 0;
            while(t < 1)
            {
                if (duration <= 0) duration = 0.1f; 
                
                t += Time.deltaTime / duration;
                float clampedT = Mathf.Clamp01(t);
                
                limb01.localEulerAngles = Vector3.Lerp(_initialLimb01LocalEulerAngles, limb01LoadLocalEulerAngles, clampedT);
                limb02.localEulerAngles = Vector3.Lerp(_initialLimb02LocalEulerAngles, limb02LoadLocalEulerAngles, clampedT);
                
                nockPoint.position = bowstringAnchorPoint.position;
                
                yield return null;
            }
            
            while (true)
            {
                nockPoint.position = bowstringAnchorPoint.position;
                yield return null;
            }
        }
        
        private IEnumerator CancelLoadBowCoroutine(float delay, float duration)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            
            Vector3 initialNockRestLocalPosition = nockPoint.localPosition;
            Vector3 startLimb01Angles = limb01.localEulerAngles;
            Vector3 startLimb02Angles = limb02.localEulerAngles;
            
            float t = 0;
            while(t < 1)
            {
                if (duration <= 0) duration = 0.1f;
                t += Time.deltaTime / duration;
                float clampedT = Mathf.Clamp01(t);

                limb01.localEulerAngles = Vector3.LerpUnclamped(startLimb01Angles, _initialLimb01LocalEulerAngles, clampedT);
                limb02.localEulerAngles = Vector3.LerpUnclamped(startLimb02Angles, _initialLimb02LocalEulerAngles, clampedT);
                nockPoint.localPosition = Vector3.LerpUnclamped(initialNockRestLocalPosition, _nockPointRestLocalPosition, clampedT);
                
                yield return null;
            }

            nockPoint.localPosition = _nockPointRestLocalPosition;
            limb01.localEulerAngles = _initialLimb01LocalEulerAngles;
            limb02.localEulerAngles = _initialLimb02LocalEulerAngles;
        }
        
        public void ShowArrow() => arrowInHand.SetActive(true);

        public void HideArrow() => arrowInHand.SetActive(false);
        
        public void ShootArrow(float delay, float duration)
        {
            if(_bowAnimation != null) StopCoroutine(_bowAnimation);
            _bowAnimation = ShootArrowCoroutine(delay, duration);
            StartCoroutine(_bowAnimation);
        }
        
        private IEnumerator ShootArrowCoroutine(float delay, float duration)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            
            HideArrow();
            
            Vector3 initialNockRestLocalPosition = nockPoint.localPosition;
            Vector3 startLimb01Angles = limb01.localEulerAngles;
            Vector3 startLimb02Angles = limb02.localEulerAngles;
            
            float t = 0;
            while(t < 1)
            {
                if (duration <= 0) duration = 0.1f;
                t += Time.deltaTime / duration;
                float curveValue = (bowReleaseCurve != null && bowReleaseCurve.keys.Length > 0) ? bowReleaseCurve.Evaluate(t) : t;

                limb01.localEulerAngles = Vector3.LerpUnclamped(startLimb01Angles, _initialLimb01LocalEulerAngles, curveValue);
                limb02.localEulerAngles = Vector3.LerpUnclamped(startLimb02Angles, _initialLimb02LocalEulerAngles, curveValue);
                nockPoint.localPosition = Vector3.LerpUnclamped(initialNockRestLocalPosition, _nockPointRestLocalPosition, curveValue);
                
                yield return null;
            }

            nockPoint.localPosition = _nockPointRestLocalPosition;
            limb01.localEulerAngles = _initialLimb01LocalEulerAngles;
            limb02.localEulerAngles = _initialLimb02LocalEulerAngles;
        }
    }
}