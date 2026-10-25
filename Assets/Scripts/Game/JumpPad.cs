using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class JumpPad : MonoBehaviour
    {
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite expandedSprite;
        [SerializeField, Min(0)] float launchSpeed = 20f;
        [SerializeField, Min(0.01f)] float expandedDuration = 0.2f;

        SpriteRenderer visual;
        BoxCollider2D trigger;

        float resetAt;

        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            trigger = GetComponent<BoxCollider2D>();

            trigger.isTrigger = true;
            visual.sprite = idleSprite;
        }


        void Update()
        {
            if (Time.time >= resetAt) visual.sprite = idleSprite;
        }


        void OnTriggerEnter2D(Collider2D other) => TryLaunch(other);

        void OnTriggerStay2D(Collider2D other) => TryLaunch(other);


        void TryLaunch(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController2D>();

            if (player == null || player.CurrentHealth <= 0)
            {
                return;
            }

            var body = player.GetComponent<Rigidbody2D>();

            // 플레이어가 위에 있을 때만
            if (body.linearVelocity.y > 0.1f || other.bounds.min.y < trigger.bounds.center.y)
            {
                return;
            }

            player.Bounce(launchSpeed);
            GameAudio.Play(GameSound.JumpPad);
            visual.sprite = expandedSprite;
            resetAt = Time.time + expandedDuration;
        }
    }
}
