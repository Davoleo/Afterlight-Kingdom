using UnityEngine;

namespace Core
{
    public enum SurfaceType
    {
        Grass,
        Gravel,
        Stone,
        Wood,
        Metal,
    }

    public class SurfaceTypeAttachment : MonoBehaviour
    {
        public SurfaceType material;
    }
}