using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]

    public sealed class StompEnemy : MonoBehaviour
    {
        [SerializeField] Sprite[] walkFrames;
        [SerializeField] Sprite stoppedSprite;


        [SerializeField] float moveSpeed = 1.5f;
        [SerializeField] float patrolDistance = 2f;
        [SerializeField] float frameDuration = 0.16f;
        [SerializeField] float bounceSpeed = 8f;
        [SerializeField] bool canBeKilled = true;
        [SerializeField] bool damagesOnStomp;
        [SerializeField] bool avoidLedges = true;
        [SerializeField, Min(0)] float stoppedDuration = 3f;

        float resumeTime;

        Rigidbody2D body;
        SpriteRenderer visual;
        BoxCollider2D hitbox;

        float originX;
        float animationTime;
        float nextStompTime;
        int direction = -1;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];
        ContactFilter2D groundFilter;

        

        public bool IsStopped { get; private set; }

        public bool IsDying { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponent<SpriteRenderer>();
            hitbox = GetComponent<BoxCollider2D>();
            originX = body.position.x;
            groundFilter = new ContactFilter2D { useTriggers = false };
        }

        void Start() => originX = body.position.x;

        void FixedUpdate()
        {

            if (IsDying)
            {
                return;
            }

            if (IsStopped && !canBeKilled && Time.time >= resumeTime)
            {
                IsStopped = false;
                animationTime = 0;
            }

            if (body.position.x <= originX - patrolDistance) direction = 1;

            else if (body.position.x >= originX + patrolDistance) direction = -1;

            if (avoidLedges && !IsStopped && Mathf.Abs(body.linearVelocity.y) < 0.1f)
            {
                Bounds bounds = hitbox.bounds;
                Vector2 foot = new Vector2(bounds.center.x, bounds.min.y + 0.1f);
                float ahead = bounds.extents.x * 0.65f + moveSpeed * Time.fixedDeltaTime + 0.03f;

                if (HasGround(foot) && !HasGround(foot + Vector2.right * direction * ahead))
                {

                    if (HasGround(foot - Vector2.right * direction * ahead)) direction = -direction;

                    else
                    {
                        body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                        return;
                    }

                }
            }

            body.linearVelocity = new Vector2(IsStopped ? 0 : direction * moveSpeed, body.linearVelocity.y);
        }




        // AiCode -------
        bool HasGround(Vector2 origin)
        {
            int count = Physics2D.Raycast(origin, Vector2.down, groundFilter, groundHits, 0.3f);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == hitbox || hit.normal.y < 0.5f)
                {
                    continue;
                }
                if (hit.rigidbody == null || hit.rigidbody.bodyType == RigidbodyType2D.Static)
                {
                    return true;
                }
            }
            return false;
        }
        //--------------




        void Update()
        {
            if (IsStopped || IsDying)
            {
                return;
            }

            animationTime += Time.deltaTime;


            if (walkFrames.Length > 0)
            {
                visual.sprite = walkFrames[(int)(animationTime / Mathf.Max(0.01f, frameDuration)) % walkFrames.Length];
            }

            visual.flipX = direction > 0;
        }


        void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);


        void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);


        void HandleCollision(Collision2D collision)
        {
            if (IsDying)
            {
                return;
            }


            var player = collision.gameObject.GetComponent<PlayerController2D>();

            if (player != null)
            {
                if (damagesOnStomp)
                {
                    player.ReceiveEnemyHit(transform.position);
                    return;
                }


                for (int i = 0; i < collision.contactCount; i++)
                {
                    // 밟기 판정
                    if (collision.GetContact(i).normal.y < -0.5f && player.transform.position.y > transform.position.y)
                    {
                        if (!canBeKilled)
                        {

                            if (!IsStopped && collision.relativeVelocity.y < -0.1f)
                            {
                                GameAudio.Play(GameSound.Stomp);
                                IsStopped = true;
                                resumeTime = Time.time + stoppedDuration;
                                visual.sprite = stoppedSprite;
                                body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                            }

                            return;
                        }


                        if (Time.time < nextStompTime)
                        {
                            return;
                        }

                        nextStompTime = Time.time + 0.2f;
                        GameAudio.Play(GameSound.Stomp);
                        player.Bounce(bounceSpeed);


                        if (IsStopped) {
                            Die(player.transform.position.x);
                        }

                        else
                        {
                            IsStopped = true;
                            visual.sprite = stoppedSprite;
                            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
                        }

                        return;
                    }
                }

                if (!IsStopped) player.ReceiveEnemyHit(transform.position);

                return;

            }


            for (int i = 0; i < collision.contactCount; i++)
            {

                if (Mathf.Abs(collision.GetContact(i).normal.x) > 0.5f)
                {
                    direction = collision.GetContact(i).normal.x > 0 ? 1 : -1;
                    break;
                }

            }
        }



        public void DefeatByFireball(float sourceX)
        {
            if (!IsDying) Die(sourceX);
        }



        void Die(float playerX)
        {
            IsDying = true;
            GameAudio.Play(GameSound.EnemyDeath);
            hitbox.enabled = false;

            body.constraints = RigidbodyConstraints2D.None;

            float launchDirection = transform.position.x >= playerX ? 1 : -1;

            body.linearVelocity = new Vector2(launchDirection * 3f, 7f);
            body.angularVelocity = -launchDirection * 540f;

            visual.sortingOrder = 20;
            
            Destroy(gameObject, 4f);
        }
    }
}
