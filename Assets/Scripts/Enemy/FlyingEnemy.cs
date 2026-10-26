using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class FlyingEnemy : MonoBehaviour
    {

        [SerializeField] Sprite[] flyFrames;
        [SerializeField] Sprite defeatedSprite;



        [SerializeField, Min(0)] float moveSpeed = 2f;
        [SerializeField, Min(0.1f)] float patrolDistance = 2.5f;
        [SerializeField, Min(0)] float bobHeight = 0.3f;
        [SerializeField, Min(0)] float bobSpeed = 3f;
        [SerializeField, Min(0.01f)] float frameDuration = 0.14f;
        [SerializeField, Min(0)] float bounceSpeed = 8f;
        [SerializeField] PlayerController2D player;
        [SerializeField, Min(.1f)] float detectionRange = 5f;
        [SerializeField, Min(.1f)] float diveSpeed = 8f;
        [SerializeField, Min(.1f)] float returnSpeed = 4f;
        [SerializeField, Min(.1f)] float attackCooldown = 2f;

        public enum AttackState { Patrolling, Diving, Returning }
        public AttackState State { get; private set; }

        Vector2 diveTarget;
        float attackTimer;

        Rigidbody2D body;
        BoxCollider2D hitbox;
        SpriteRenderer visual;

        Vector2 origin;

        float flightTime;
        int direction = -1;



        public bool IsDying { get; private set; }



        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<BoxCollider2D>();
            visual = GetComponent<SpriteRenderer>();
            origin = body.position;
            body.gravityScale = 0;
        }



        void Start()
        {
            origin = body.position;
            if (player == null)
                foreach (var root in gameObject.scene.GetRootGameObjects())
                {
                    player = root.GetComponentInChildren<PlayerController2D>();
                    if (player != null) break;
                }
            attackTimer = attackCooldown;
        }



        void FixedUpdate()
        {
            if (IsDying)
            {
                return;
            }


            flightTime += Time.fixedDeltaTime;
            attackTimer = Mathf.Max(0, attackTimer - Time.fixedDeltaTime);
            if (State == AttackState.Diving)
            {
                body.linearVelocity = Vector2.ClampMagnitude((diveTarget - body.position) / Time.fixedDeltaTime, diveSpeed);
                direction = diveTarget.x > body.position.x ? 1 : -1;
                if ((diveTarget - body.position).sqrMagnitude < .04f || attackTimer <= 0 ||
                    player == null || player.IsDead || !player.enabled)
                    BeginReturn();
                return;
            }
            if (State == AttackState.Returning)
            {
                body.linearVelocity = Vector2.ClampMagnitude((origin - body.position) / Time.fixedDeltaTime, returnSpeed);
                if ((origin - body.position).sqrMagnitude < .01f)
                {
                    State = AttackState.Patrolling;
                    attackTimer = attackCooldown;
                }
                return;
            }
            if (attackTimer <= 0 && player != null && player.enabled && !player.IsDead && !player.IsControlLocked)
            {
                Vector2 delta = (Vector2)player.transform.position - body.position;
                if (Mathf.Abs(delta.x) <= detectionRange && delta.y < -.2f && delta.y > -detectionRange * 2)
                {
                    diveTarget = player.transform.position;
                    State = AttackState.Diving;
                    attackTimer = 2f;
                    return;
                }
            }

            if (body.position.x <= origin.x - patrolDistance) direction = 1;

            else if (body.position.x >= origin.x + patrolDistance) direction = -1;

            float targetY = origin.y + Mathf.Sin(flightTime * bobSpeed) * bobHeight;

            body.linearVelocity = new Vector2(direction * moveSpeed,
                (targetY - body.position.y) / Time.fixedDeltaTime);

        }

        void Update()
        {
            if (IsDying || flyFrames == null || flyFrames.Length == 0)
            {
                return;
            }

            visual.sprite = flyFrames[(int)(flightTime / Mathf.Max(0.01f, frameDuration)) % flyFrames.Length];
            visual.flipX = direction > 0;
        }


        void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);


        public void DefeatByFireball(float sourceX)
        {
            Die(sourceX);
        }

        void Die(float sourceX)
        {
            if (IsDying)
            {
                return;
            }

            IsDying = true;
            GameAudio.Play(GameSound.EnemyDeath);
            hitbox.enabled = false;

            if (defeatedSprite != null) visual.sprite = defeatedSprite;

            body.gravityScale = 3f;
            body.constraints = RigidbodyConstraints2D.None;

            float away = transform.position.x >= sourceX ? 1 : -1;

            body.linearVelocity = new Vector2(away * 3f, 5f);
            body.angularVelocity = -away * 540f;
            visual.sortingOrder = 20;

            Destroy(gameObject, 4f);
        }

        void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);

        void BeginReturn()
        {
            State = AttackState.Returning;
            body.linearVelocity = Vector2.zero;
        }

        void HandleCollision(Collision2D collision)
        {
            if (IsDying)
            {
                return;
            }

            var player = collision.gameObject.GetComponent<PlayerController2D>();
            if (State == AttackState.Diving) BeginReturn();

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 normal = collision.GetContact(i).normal;

                if (player != null && normal.y < -0.5f &&
                    player.transform.position.y > transform.position.y && collision.relativeVelocity.y < -0.1f)
                {
                    player.Bounce(bounceSpeed);
                    GameAudio.Play(GameSound.Stomp);
                    Die(player.transform.position.x);
                    return;
                }

                if (player == null && Mathf.Abs(normal.x) > 0.5f)
                    direction = normal.x > 0 ? 1 : -1;
            }

            if (player != null) 
            {
                player.ReceiveEnemyHit(transform.position);
            }
        }
    }
}
