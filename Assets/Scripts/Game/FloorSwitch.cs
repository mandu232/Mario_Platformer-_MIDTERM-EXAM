using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class FloorSwitch : MonoBehaviour
    {
        [SerializeField, Tooltip("All tiles on this tilemap are removed when the player steps on the switch.")]
        Tilemap targetTilemap;
        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite pressedSprite;

        SpriteRenderer visual;
        BoxCollider2D trigger;
        public bool IsPressed { get; private set; }

        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            trigger = GetComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            visual.sprite = idleSprite;
        }

        void OnTriggerEnter2D(Collider2D other) => TryPress(other);
        void OnTriggerStay2D(Collider2D other) => TryPress(other);

        // AiCode -------
        void TryPress(Collider2D other)
        {
            if (IsPressed || targetTilemap == null)
            {
                return;
            }
            var player = other.GetComponentInParent<PlayerController2D>();
            if (player == null || player.CurrentHealth <= 0)
            {
                return;
            }
            var body = player.GetComponent<Rigidbody2D>();
            float previousFootY = other.bounds.min.y - Mathf.Min(0, body.linearVelocity.y) * Time.fixedDeltaTime;
            if (body.linearVelocity.y > 0.1f || previousFootY < trigger.bounds.center.y - 0.05f)
            {
                return;
            }

            IsPressed = true;
            visual.sprite = pressedSprite;
            targetTilemap.ClearAllTiles();

            //콜라이더 갱신
            var tileCollider = targetTilemap.GetComponent<TilemapCollider2D>();
            if (tileCollider != null)
            {
                tileCollider.ProcessTilemapChanges();
                var composite = tileCollider.attachedRigidbody != null
                    ? tileCollider.attachedRigidbody.GetComponent<CompositeCollider2D>()
                    : null;
                if (composite != null) composite.GenerateGeometry();
            }
        }
        //-------
    }
}
