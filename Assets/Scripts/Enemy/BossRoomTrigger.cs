using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class BossRoomTrigger : MonoBehaviour
    {
        [SerializeField] FlyingBoss boss;

        [SerializeField, Min(0.1f)] float centerWidth = 3f;

        public bool IsTriggered 
        { 
            get; private set; 
        }




        void Reset()
        {
            var area = GetComponent<BoxCollider2D>();
            area.isTrigger = true;
            area.size = new Vector2(20, 12);
        }



        void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other) => Detect(other);
        void OnTriggerStay2D(Collider2D other) => Detect(other);

        void Detect(Collider2D other)
        {
            if (IsTriggered || boss == null || boss.IsDying)
            {
                return;
            }
            var player = other.GetComponentInParent<PlayerController2D>();
            if (player == null || !player.isActiveAndEnabled || player.CurrentHealth <= 0)
            {
                return;
            }

            var area = GetComponent<BoxCollider2D>();
            Vector2 local = transform.InverseTransformPoint(player.transform.position);
            local -= area.offset;
            if (Mathf.Abs(local.x) > Mathf.Min(centerWidth, area.size.x) * 0.5f ||
                Mathf.Abs(local.y) > area.size.y * 0.5f) return;
            boss.ActivateEncounter(player);
            IsTriggered = boss.IsActivated;

        }

        void OnDrawGizmos()
        {
            var area = GetComponent<BoxCollider2D>();
            if (area == null)
            {
                return;
            }
            //범위 표기용
            //AiCode ----
            var previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.07f);
            Gizmos.DrawCube(area.offset, area.size);
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.9f);
            Gizmos.DrawWireCube(area.offset, area.size);
            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.85f);
            Gizmos.DrawWireCube(area.offset, new Vector2(Mathf.Min(centerWidth, area.size.x), area.size.y));
            Gizmos.matrix = previousMatrix;
            // ---------
        }
    }
}
