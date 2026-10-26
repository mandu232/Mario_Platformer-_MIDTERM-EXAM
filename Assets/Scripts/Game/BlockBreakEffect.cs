using UnityEngine;

namespace Midterm
{
    public sealed class BlockBreakEffect : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] fragments;
        [SerializeField, Min(.01f)] float lifetime = 2f;

        public void Play(SpriteRenderer source)
        {
            foreach (var fragment in fragments)
            {
                fragment.sharedMaterial = source.sharedMaterial;
                fragment.sortingLayerID = source.sortingLayerID;
                fragment.sortingOrder = source.sortingOrder + 1;
            }
            Destroy(gameObject, lifetime);
        }
    }
}
