using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class DroppingEnemy : MonoBehaviour
    {
        public enum AttackState
        {
            Waiting,
            Falling,
            Resting,
            Returning,
            Shaking
        }

        [SerializeField] Sprite idleSprite;
        [SerializeField] Sprite attackSprite;
        [SerializeField] PlayerController2D player;
        [SerializeField, Min(0.1f)] float detectionDistance = 7f;
        [SerializeField, Min(0.1f)] float detectionHalfWidth = 0.8f;
        [SerializeField, Min(0.1f)] float fallSpeed = 12f;
        [SerializeField, Min(0.1f)] float maxDropDistance = 7f;
        [SerializeField, Min(0)] float bottomWait = 1.5f;
        [SerializeField, Min(0.1f)] float returnSpeed = 2f;
        [SerializeField, Min(0)] float rearmDelay = 1f;
        [SerializeField, Min(0.01f)] float shakeDuration = 0.6f;
        [SerializeField, Min(0)] float shakeAmount = 0.06f;
        [SerializeField, Min(0)] float shakeFrequency = 18f;

        Rigidbody2D body;
        BoxCollider2D hitbox;
        SpriteRenderer visual;
        Vector2 home;
        float timer;
        ContactFilter2D solidFilter;
        readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        public AttackState State { get; private set; }
        public bool IsDying { get; private set; }

        public void DefeatByFireball(float sourceX)
        {
            if (IsDying)
            {
                return;
            }
            IsDying = true;
            GameAudio.Play(GameSound.EnemyDeath);
            hitbox.enabled = false;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 3;
            body.constraints = RigidbodyConstraints2D.None;
            float away = transform.position.x >= sourceX ? 1 : -1;
            body.linearVelocity = new Vector2(away * 3, 5);
            body.angularVelocity = -away * 540;
            visual.sortingOrder = 20;
            Destroy(gameObject, 4);
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<BoxCollider2D>();
            visual = GetComponent<SpriteRenderer>();
            home = body.position;
            solidFilter = new ContactFilter2D { useTriggers = false };
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
            body.gravityScale = 0;
            visual.sprite = idleSprite;
        }

        void Start()
        {
            home = body.position;
            if (player == null) player = FindFirstObjectByType<PlayerController2D>();
        }

        // AiCode -------
        void FixedUpdate()
        {
            if (IsDying)
            {
                return;
            }
            timer = Mathf.Max(0, timer - Time.fixedDeltaTime);
            switch (State)
            {
                case AttackState.Waiting:
                    if (timer <= 0 && PlayerBelow())
                    {
                        State = AttackState.Shaking;
                        timer = shakeDuration;
                        visual.sprite = attackSprite;
                    }
                    break;
                case AttackState.Shaking:
                    float offset = Mathf.Sin((shakeDuration - timer) * shakeFrequency * Mathf.PI * 2) * shakeAmount;
                    body.MovePosition(home + Vector2.right * offset);
                    if (timer <= 0)
                    {
                        body.MovePosition(home);
                        // 낙하 공격은 트리거로 판정해서 플레이어를 바닥으로 밀지 않는다.
                        hitbox.isTrigger = true;
                        State = AttackState.Falling;
                    }
                    break;
                case AttackState.Falling:
                    float distance = Mathf.Min(fallSpeed * Time.fixedDeltaTime,
                        Mathf.Max(0, body.position.y - (home.y - maxDropDistance)));
                    bool landed = false;
                    int count = hitbox.Cast(Vector2.down, solidFilter, hits, distance + 0.02f);
                    for (int i = 0; i < count; i++)
                    {
                        if (hits[i].collider.GetComponentInParent<PlayerController2D>() != null || hits[i].normal.y < 0.5f)
                        {
                            continue;
                        }
                        distance = Mathf.Min(distance, Mathf.Max(0, hits[i].distance - 0.01f));
                        landed = true;
                    }
                    body.MovePosition(body.position + Vector2.down * distance);
                    if (landed || body.position.y - distance <= home.y - maxDropDistance + 0.01f)
                    {
                        State = AttackState.Resting;
                        timer = bottomWait;
                    }
                    break;
                case AttackState.Resting:
                    if (timer <= 0) State = AttackState.Returning;
                    break;
                case AttackState.Returning:
                    Vector2 next = Vector2.MoveTowards(body.position, home, returnSpeed * Time.fixedDeltaTime);
                    body.MovePosition(next);
                    if ((next - home).sqrMagnitude < 0.000001f)
                    {
                        State = AttackState.Waiting;
                        timer = rearmDelay;
                        visual.sprite = idleSprite;
                        hitbox.isTrigger = false;
                    }
                    break;
            }
        }
        //-----------------

        bool PlayerBelow()
        {
            if (player == null || player.CurrentHealth <= 0)
            {
                return false;
            }
            Vector2 delta = (Vector2)player.transform.position - body.position;
            if (delta.y >= 0 || -delta.y > detectionDistance || Mathf.Abs(delta.x) > detectionHalfWidth)
            {
                return false;
            }
            int count = Physics2D.Linecast(body.position, player.transform.position, solidFilter, hits);
            for (int i = 0; i < count; i++)
                if (hits[i].collider != hitbox && hits[i].collider.GetComponentInParent<PlayerController2D>() == null)
                    return false;
            return true;
        }

        void OnTriggerEnter2D(Collider2D other) => Hit(other);
        void OnTriggerStay2D(Collider2D other) => Hit(other);

        void Hit(Collider2D other)
        {
            if (IsDying)
            {
                return;
            }
            if (State != AttackState.Falling)
            {
                return;
            }
            var target = other.GetComponentInParent<PlayerController2D>();
            if (target == null)
            {
                return;
            }
            target.ReceiveEnemyHit(transform.position);
        }
    }
}
