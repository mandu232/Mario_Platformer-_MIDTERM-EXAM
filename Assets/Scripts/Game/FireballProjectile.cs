using UnityEngine;
using UnityEngine.SceneManagement;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class FireballProjectile : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float speed = 8f;
        [SerializeField, Min(0)] float launchUpwardSpeed = 4f;
        [SerializeField, Min(0.1f)] float bounceSpeed = 7f;
        [SerializeField, Min(0.1f)] float gravity = 24f;
        [SerializeField, Min(0.1f)] float lifetime = 5f;
        Rigidbody2D body;
        CircleCollider2D hitbox;
        PlayerController2D owner;
        Vector2 velocity;
        float remainingLife;
        bool spent;
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        ContactFilter2D filter;
        public int BounceCount { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<CircleCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            hitbox.isTrigger = true;
            filter = new ContactFilter2D { useTriggers = true };
            remainingLife = lifetime;
        }

        public void Launch(PlayerController2D player, int direction)
        {
            owner = player;
            float horizontalSpeed = speed;
            if (direction < 0)
            {
                horizontalSpeed = -speed;
            }
            velocity = new Vector2(horizontalSpeed, launchUpwardSpeed);
        }

        // AiCode ---------
        void FixedUpdate()
        {
            if (spent)
            {
                return;
            }
            remainingLife -= Time.fixedDeltaTime;
            if (remainingLife <= 0)
            {
                Despawn();
                return;
            }
            velocity.y -= gravity * Time.fixedDeltaTime;
            float remainingStep = Time.fixedDeltaTime;
            Vector2 position = body.position;
            for (int pass = 0; pass < 3 && remainingStep > 0.00001f; pass++)
            {
                float distance = velocity.magnitude * remainingStep;
                int count = gameObject.scene.GetPhysicsScene2D().CircleCast(position, hitbox.radius * Mathf.Abs(transform.lossyScale.x),
                    velocity.normalized, distance, filter, hits);
                int nearest = -1;
                for (int i = 0; i < count; i++)
                {
                    var collider = hits[i].collider;
                    if (collider == hitbox || collider.GetComponentInParent<PlayerController2D>() != null ||
                        collider.GetComponentInParent<FireballProjectile>() != null) continue;
                    bool enemy = collider.GetComponentInParent<StompEnemy>() != null ||
                        collider.GetComponentInParent<FlyingBoss>() != null ||
                        collider.GetComponentInParent<FlyingEnemy>() != null || collider.GetComponentInParent<DroppingEnemy>() != null;
                    if (collider.isTrigger && !enemy)
                    {
                        continue;
                    }
                    if (nearest < 0 || hits[i].distance < hits[nearest].distance) nearest = i;
                }
                if (nearest < 0)
                {
                    position += velocity * remainingStep;
                    break;
                }
                var hit = hits[nearest];
                var stomp = hit.collider.GetComponentInParent<StompEnemy>();
                var flying = hit.collider.GetComponentInParent<FlyingEnemy>();
                var dropping = hit.collider.GetComponentInParent<DroppingEnemy>();
                var boss = hit.collider.GetComponentInParent<FlyingBoss>();
                if (stomp != null || flying != null || dropping != null || boss != null)
                {
                    float sourceX = owner != null ? owner.transform.position.x : position.x;
                    if (stomp != null) stomp.DefeatByFireball(sourceX);
                    if (flying != null) flying.DefeatByFireball(sourceX);
                    if (dropping != null) dropping.DefeatByFireball(sourceX);
                    if (boss != null) boss.ReceiveFireball(sourceX);
                    Despawn();
                    return;
                }
                if (hit.normal.y < 0.5f || velocity.y >= 0 || BounceCount >= 2)
                {
                    Despawn();
                    return;
                }
                position += velocity.normalized * Mathf.Max(0, hit.distance - 0.005f);
                position += Vector2.up * 0.01f;
                remainingStep *= 1 - Mathf.Clamp01(hit.distance / Mathf.Max(0.00001f, distance));
                BounceCount++;
                GameAudio.Play(GameSound.FireballBounce);
                velocity.y = bounceSpeed;
            }
            body.MovePosition(position);
        }
        //-------

        void Despawn()
        {
            spent = true;
            hitbox.enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
