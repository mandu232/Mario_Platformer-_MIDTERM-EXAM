using UnityEngine;
using UnityEngine.InputSystem;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        
        [SerializeField] Sprite[] walkFrames;
        [SerializeField] Sprite[] FireModeFrames;

        [SerializeField, Min(0)] float moveSpeed = 6f;
        [SerializeField, Min(0)] float jumpSpeed = 10f;
        [SerializeField, Min(0)] float maxJumpHoldTime = 0.2f;
        [SerializeField, Min(0.01f)] float frameDuration = 0.12f;
        [SerializeField, Min(1)] int maxHealth = 4;
        [SerializeField, Min(0)] float invulnerabilityDuration = 1.2f;
        [SerializeField, Min(0)] float knockbackDuration = 0.25f;
        [SerializeField] Vector2 knockbackSpeed = new Vector2(6f, 5f);
        [SerializeField, Min(0.01f)] float blinkInterval = 0.08f;

        float invulnerableUntil;
        float knockbackUntil;
        float hitTime;

        Color normalColor;

        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public bool IsControlLocked { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsPoweredUp { get; private set; }
        RigidbodyConstraints2D unlockedConstraints;

        public void GrantFireFlower()
        {
            if (IsDead || IsPoweredUp) return;
            IsPoweredUp = true;
            animationTime = 0;
        }

        // AiCode -------
        public void SetControlLocked(bool locked)
        {
            if (IsControlLocked == locked)
            {
                return;
            }
            IsControlLocked = locked;
            horizontal = jumpTime = 0;
            jumpPressed = jumpHeld = false;
            if (locked)
            {
                unlockedConstraints = body.constraints;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0;
                body.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else body.constraints = unlockedConstraints;
        }
        // AiCode -------
        public int FacingDirection => horizontal != 0 ? (horizontal > 0 ? 1 : -1) : (visual.flipX ? 1 : -1);

        public void ReceiveEnemyHit(Vector2 enemyPosition)
        {
            float direction = transform.position.x < enemyPosition.x ? -1 : 1;
            ReceiveHit(new Vector2(direction * knockbackSpeed.x, knockbackSpeed.y));
        }

        public void ReceiveSpikeHit(float upwardSpeed)
        {
            ReceiveHit(new Vector2(0, upwardSpeed));
        }

        public void ReceiveBossHit(Vector2 bossPosition, Vector2 knockback, float protectionDuration)
        {
            float direction = transform.position.x < bossPosition.x ? -1 : 1;
            ReceiveHit(new Vector2(direction * knockback.x, knockback.y), protectionDuration);
        }

        void ReceiveHit(Vector2 velocity, float protectionDuration = 0)
        {
            if (!isActiveAndEnabled || IsControlLocked || IsInvulnerable || CurrentHealth <= 0)
            {
                return;
            }
            hitTime = Time.time;
            invulnerableUntil = Time.time + Mathf.Max(invulnerabilityDuration, protectionDuration);
            knockbackUntil = Time.time + knockbackDuration;
            if (IsPoweredUp)
            {
                IsPoweredUp = false;
                animationTime = 0;
                GameAudio.Play(GameSound.Hit);
            }
            else TakeDamage(1);
            if (IsDead)
            {
                return;
            }
            body.linearVelocity = velocity;
            jumpPressed = jumpHeld = grounded = false;
            jumpTime = 0;
            bouncing = true;
        }

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public event System.Action<int, int> HealthChanged;

        public void TakeDamage(int amount)
        {
            if (!isActiveAndEnabled || IsDead) return;
            if (amount > 0) SetHealth(CurrentHealth - Mathf.Min(amount, CurrentHealth));
        }

        public void Heal(int amount)
        {
            if (IsDead)
            {
                return;
            }
            if (amount > 0) SetHealth(CurrentHealth + Mathf.Min(amount, MaxHealth - CurrentHealth));
        }

        void SetHealth(int value)
        {
            value = Mathf.Clamp(value, 0, MaxHealth);
            if (CurrentHealth == value)
            {
                return;
            }
            bool tookDamage = value < CurrentHealth;
            if (tookDamage && IsPoweredUp)
            {
                IsPoweredUp = false;
                animationTime = 0;
            }
            CurrentHealth = value;
            if (CurrentHealth == 0) Die();
            else if (tookDamage) GameAudio.Play(GameSound.Hit);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        void Die()
        {
            if (IsDead)
            {
                return;
            }

            SetControlLocked(false);

            IsDead = true;

            GameAudio.Play(GameSound.Death);
            
            horizontal = jumpTime = 0;
            jumpPressed = jumpHeld = false;
            invulnerableUntil = knockbackUntil = 0;
            playerCollider.enabled = false;
            body.simulated = true;
            body.gravityScale = 3;
            body.linearVelocity = Vector2.up * 7;
            visual.color = normalColor;
        }

        public void Respawn(Vector3 position)
        {
            if (IsDead)
            {
                return;
            }

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            body.position = position;
            transform.position = position;
            jumpPressed = jumpHeld = grounded = false;
            jumpTime = animationTime = 0;
            bouncing = false;
            knockbackUntil = 0;
        }

        public void Revive(Vector3 position)
        {
            SetControlLocked(false);
            IsDead = IsPoweredUp = false;
            enabled = true;
            body.simulated = true;
            body.gravityScale = initialGravityScale;
            body.constraints = initialConstraints;
            playerCollider.enabled = true;
            Respawn(position);
            invulnerableUntil = Time.time + invulnerabilityDuration;
            hitTime = Time.time;
            CurrentHealth = MaxHealth;
            visual.color = normalColor;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void Bounce(float speed)
        {
            if (IsControlLocked || IsDead)
            {
                return;
            }
            body.linearVelocity = new Vector2(body.linearVelocity.x, speed);
            jumpTime = 0;
            jumpPressed = grounded = false;
            bouncing = true;
        }

        Rigidbody2D body;
        Collider2D playerCollider;
        SpriteRenderer visual;
        ContactFilter2D groundFilter;
        readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        float horizontal;
        float jumpTime;
        float animationTime;
        bool jumpPressed;
        bool jumpHeld;
        bool grounded;
        bool bouncing;
        float initialGravityScale;
        RigidbodyConstraints2D initialConstraints;

        void Awake()
        {
            CurrentHealth = MaxHealth;
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            visual = GetComponent<SpriteRenderer>();
            initialGravityScale = body.gravityScale;
            initialConstraints = body.constraints;
            normalColor = visual.color;
            groundFilter = new ContactFilter2D { useTriggers = false };
        }

        void OnDisable()
        {
            SetControlLocked(false);
            invulnerableUntil = knockbackUntil = 0;
            if (visual != null) visual.color = normalColor;
        }

        void Update()
        {
            if (IsControlLocked || IsDead)
            {
                return;
            }
            var keyboard = Keyboard.current;
            horizontal = 0;
            jumpHeld = keyboard != null && keyboard.spaceKey.isPressed;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1;
            jumpPressed |= keyboard.spaceKey.wasPressedThisFrame;
        }

        void FixedUpdate()
        {
            if (IsDead)
            {
                return;
            }
            if (IsControlLocked)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }
            if (Time.time < knockbackUntil)
            {
                jumpPressed = false;
                grounded = false;
                return;
            }
            grounded = CheckGrounded();
            Vector2 velocity = body.linearVelocity;
            if (velocity.y <= 0) bouncing = false;
            velocity.x = horizontal * moveSpeed;

            if (jumpPressed && grounded)
            {
                GameAudio.Play(GameSound.Jump);
                velocity.y = jumpSpeed;
                jumpTime = maxJumpHoldTime;
                grounded = false;
            }
            jumpPressed = false;

            // 스페이스를 누르는 동안 더 높이 뛰고, 놓으면 상승 속도를 줄인다.
            if (jumpHeld && jumpTime > 0 && velocity.y > 0)
            {
                velocity.y = jumpSpeed;
                jumpTime -= Time.fixedDeltaTime;
            }
            else
            {
                jumpTime = 0;
                if (!bouncing && !jumpHeld && velocity.y > jumpSpeed * 0.5f)
                    velocity.y = jumpSpeed * 0.5f;
            }
            body.linearVelocity = velocity;
        }

        // AiCode -------
        bool CheckGrounded()
        {
            // 바닥에 서 있어도 물리 계산 오차로 작은 양의 Y 속도가 나올 수 있다.
            if (body.linearVelocity.y > 0.1f)
            {
                return false;
            }

            int count = playerCollider.Cast(Vector2.down, groundFilter, groundHits, 0.08f);
            for (int i = 0; i < count; i++)
                if (groundHits[i].normal.y > 0.5f)
                {
                    return true;
                }

            return false;
        }
        // AiCode -------

        void LateUpdate()
        {
            Color color = normalColor;
            if (IsInvulnerable && (int)((Time.time - hitTime) / Mathf.Max(0.01f, blinkInterval)) % 2 == 0)
                color.a *= 0.2f;
            visual.color = color;
            if (horizontal != 0) visual.flipX = horizontal > 0;
            Sprite[] frames = IsPoweredUp && FireModeFrames != null && FireModeFrames.Length > 0
                ? FireModeFrames : walkFrames;
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            float duration = Mathf.Max(0.01f, frameDuration);
            if (horizontal != 0 && grounded)
            {
                animationTime += Time.deltaTime;
                animationTime %= duration * frames.Length;
            }
            else
            {
                animationTime = 0;
            }

            int frame;
            if (grounded)
            {
                frame = (int)(animationTime / duration);
            }
            else
            {
                frame = Mathf.Min(1, frames.Length - 1);
            }
            Sprite sprite = frames[frame];
            if (visual.sprite != sprite) visual.sprite = sprite;
        }
    }
}
