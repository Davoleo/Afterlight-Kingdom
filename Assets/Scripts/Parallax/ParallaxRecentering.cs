using UnityEngine;

namespace Parallax
{
    public class ParallaxRecentering : MonoBehaviour
    {
        //target to center to
        public Transform target;

        public bool lockX;
        public bool lockY = true;
        public bool lockZ;

        private Vector3 fixedOffset;

        private void Start()
        {
            if (target != null)
                fixedOffset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            Vector3 pos = transform.position;
            Debug.Log(pos);
            // if the lock is not locked, move the background together with the character movement along the axis
            if (!lockX) pos.x = target.position.x + fixedOffset.x;
            if (!lockY) pos.y = target.position.y + fixedOffset.y;
            if (!lockZ) pos.z = target.position.z + fixedOffset.z;

            transform.position = pos;
        }
    }
}