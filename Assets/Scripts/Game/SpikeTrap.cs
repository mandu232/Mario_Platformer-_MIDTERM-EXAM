using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Collider2D))]

    public sealed class SpikeTrap : MonoBehaviour
    {

        [SerializeField, Min(0)] float upwardSpeed = 9f;

        void OnTriggerEnter2D(Collider2D other) => Hit(other);
        void OnTriggerStay2D(Collider2D other) => Hit(other);

        void Hit(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController2D>();
            if (player != null)
            {
                player.ReceiveSpikeHit(upwardSpeed);
            }
        }
    }
}
