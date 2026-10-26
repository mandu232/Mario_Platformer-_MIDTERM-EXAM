using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BlockFragmentMotion : MonoBehaviour
    {
        [SerializeField] Vector2 launchVelocity;
        [SerializeField] float angularVelocity;

        void Start()
        {
            var body = GetComponent<Rigidbody2D>();
            body.linearVelocity = launchVelocity;
            body.angularVelocity = angularVelocity;
        }
    }
}
