using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Midterm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class FlyingBoss : MonoBehaviour
    {


        public enum BossState
        {
            Flying,
            WindingUp,
            Charging,
            Falling,
            Stunned,
            Recovering,
            Defeated,
            Emerging
        }


        [SerializeField] PlayerController2D player;
        [SerializeField] BossHealthHud bossHud;
        [SerializeField] FollowCamera impactCamera;

        [SerializeField] bool waitForRoomTrigger;
        [SerializeField, Min(0.1f)] float emergenceDuration = 1.2f;
        [SerializeField, Min(0)] float emergenceShakeStrength = 0.2f;
        [SerializeField] Sprite flyingSprite;
        [SerializeField] Sprite stunnedSprite;
        [SerializeField, Tooltip("Tiles removed when this boss is defeated.")] Tilemap defeatBlocks;
        [SerializeField, Min(1)] int fireballHitsToDefeat = 10;
        [SerializeField, Min(0.1f)] float flightSpeed = 2f;
        [SerializeField, Min(0.1f)] float patrolRadius = 3f;
        [SerializeField, Min(0.1f)] float detectionRange = 12f;
        [SerializeField, Min(0.1f)] float attackInterval = 2.5f;
        [SerializeField, Min(0.1f)] float windupDuration = 1f;
        [SerializeField] float spinSpeed = 1080f;
        [SerializeField, Min(0.1f)] float chargeSpeed = 14f;
        [SerializeField, Min(0.1f)] float chargeTimeout = 2f;
        [SerializeField, Min(0.1f)] float stunDuration = 3f;
        [SerializeField, Min(0.1f)] float recoverySpeed = 4f;
        [SerializeField, Min(0.1f)] float fallGravity = 3f;
        [SerializeField, Min(0)] float stompBounceSpeed = 10f;
        [SerializeField, Range(0.1f, 1f)] float airborneColliderScale = 0.8f;
        [SerializeField, Range(0.1f, 1f)] float stunnedColliderScale = 0.55f;
        [SerializeField, Min(0)] float groundEmbedDepth = 0.2f;
        [SerializeField, Min(0)] float landingShakeDuration = 0.25f;
        [SerializeField, Min(0)] float landingShakeStrength = 0.22f;
        [SerializeField, Min(0)] float deathShakeDuration = 0.55f;
        [SerializeField, Min(0)] float deathShakeStrength = 0.65f;
        [SerializeField, Min(0.01f)] float damageShakeDuration = 0.22f;
        [SerializeField, Min(0)] float damageShakeAmount = 0.06f;
        [SerializeField, Min(0.1f)] float damageShakeFrequency = 24f;

        Rigidbody2D body;
        CircleCollider2D hitbox;
        SpriteRenderer visual;
        Vector2 home;
        Vector2 chargeDirection;
        Vector2 emergenceStart;
        GameObject emergenceMask;
        Sprite emergenceMaskSprite;

        int combatSortingLayer;
        int combatSortingOrder;
        bool entranceLocksPlayer;

        //스프라이트 없을때 사용
        //사용안함
        Color normalColor;

        float timer;
        float flightTime;
        float normalColliderRadius;
        float shakeRemaining;
        float stunnedColliderRadius;
        float embedAmount;
        int direction = -1;

        ContactFilter2D groundFilter;

        readonly RaycastHit2D[] groundHits = new RaycastHit2D[16];

        public BossState State { get; private set; }
        public int FireballHits { get; private set; }
        public int RemainingHits => Mathf.Max(0, fireballHitsToDefeat - FireballHits);
        public int MaxFireballHits => fireballHitsToDefeat;
        public event System.Action<int, int> HealthChanged;
        public bool IsDying => State == BossState.Defeated;
        public bool IsActivated { get; private set; }



        public void ActivateEncounter(PlayerController2D enteringPlayer)
        {
            if (IsActivated || IsDying || enteringPlayer == null || enteringPlayer.CurrentHealth <= 0)
            {
                return;
            }

            player = enteringPlayer;
            IsActivated = true;
            BeginEmergence();

        }




        // AiCode -------
        void BeginEmergence()
        {
            GameAudio.Play(GameSound.BossAppear);
            player.SetControlLocked(true);
            entranceLocksPlayer = true;

            if (bossHud != null) bossHud.ShowEntrance(this);

            float floorY = FindEmergenceFloor();
            emergenceStart = new Vector2(home.x, floorY - visual.bounds.extents.y - 0.1f);
            State = BossState.Emerging;
            timer = emergenceDuration;
            body.simulated = false;
            visual.enabled = true;

            var playerVisual = player.GetComponent<SpriteRenderer>();

            if (playerVisual != null)
            {
                visual.sortingLayerID = playerVisual.sortingLayerID;
                visual.sortingOrder = playerVisual.sortingOrder - 1;
            }

            SetEmergencePosition(emergenceStart);
            CreateEmergenceMask(floorY);

            if (impactCamera != null)
            {
                impactCamera.FocusOn(transform);
                impactCamera.Shake(emergenceDuration, emergenceShakeStrength);
            }
        }
        // --------------



        // AiCode -------
        float FindEmergenceFloor()
        {
            float floorY = home.y - 4;
            int count = gameObject.scene.GetPhysicsScene2D().Raycast(home, Vector2.down, 30, groundFilter, groundHits);
            float closest = float.PositiveInfinity;

            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];


                if (hit.collider == hitbox || hit.normal.y < 0.5f ||
                    (hit.rigidbody != null && hit.rigidbody.bodyType != RigidbodyType2D.Static) || hit.distance >= closest) continue;


                closest = hit.distance;
                floorY = hit.point.y;
            }

            return floorY;
        }
        //---------------



        // AiCode -----
        void CreateEmergenceMask(float floorY)
        {
            emergenceMaskSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
            emergenceMask = new GameObject("BossEmergenceMask", typeof(SpriteMask));

            SceneManager.MoveGameObjectToScene(emergenceMask, gameObject.scene);

            float height = Mathf.Max(10, home.y - floorY + 8);

            emergenceMask.transform.position = new Vector3(home.x, floorY + height * 0.5f, transform.position.z);
            emergenceMask.transform.localScale = new Vector3(visual.bounds.size.x + 4, height, 1);

            emergenceMask.GetComponent<SpriteMask>().sprite = emergenceMaskSprite;
            visual.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
        //--------------



        void SetEmergencePosition(Vector2 position)
        {
            body.position = position;

            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }


        void ClearEmergenceMask()
        {
            if (impactCamera != null) impactCamera.ClearFocus(transform);

            if (visual != null)
            {
                visual.maskInteraction = SpriteMaskInteraction.None;
                visual.sortingLayerID = combatSortingLayer;
                visual.sortingOrder = combatSortingOrder;
            }

            if (emergenceMask != null) Destroy(emergenceMask);

            if (emergenceMaskSprite != null) Destroy(emergenceMaskSprite);
        }

        // AiCode ----
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<CircleCollider2D>();

            var sourceVisual = GetComponent<SpriteRenderer>();
            var graphics = new GameObject("BossVisual", typeof(SpriteRenderer));

            graphics.transform.SetParent(transform, false);

            visual = graphics.GetComponent<SpriteRenderer>();

            visual.sprite = sourceVisual.sprite;
            visual.sharedMaterial = sourceVisual.sharedMaterial;
            visual.sortingLayerID = sourceVisual.sortingLayerID;
            visual.sortingOrder = sourceVisual.sortingOrder;

            combatSortingLayer = sourceVisual.sortingLayerID;
            combatSortingOrder = sourceVisual.sortingOrder;

            visual.color = sourceVisual.color;
            visual.flipX = sourceVisual.flipX;
            visual.flipY = sourceVisual.flipY;

            sourceVisual.enabled = false;

            stunnedColliderRadius = hitbox.radius * stunnedColliderScale;

            normalColliderRadius = hitbox.radius * airborneColliderScale;

            hitbox.radius = normalColliderRadius;

            normalColor = visual.color;

            home = body.position;


            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            IsActivated = !waitForRoomTrigger;

            body.simulated = IsActivated;

            visual.enabled = IsActivated;

            groundFilter = new ContactFilter2D { useTriggers = false };

            timer = attackInterval;

            if (flyingSprite != null) visual.sprite = flyingSprite;
        }
        //----------------

        void Start()
        {
            home = body.position;

            if (player == null) player = FindAnyObjectByType<PlayerController2D>();

            var mainCamera = Camera.main;

            if (impactCamera == null && mainCamera != null && mainCamera.gameObject.scene == gameObject.scene)
            {
                impactCamera = mainCamera.GetComponent<FollowCamera>();
            }
        }

        void FixedUpdate()
        {

            if (!IsActivated || IsDying)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            flightTime += dt;
            timer -= dt;

            switch (State)
            {
                case BossState.Emerging:
                    UpdateEmerging();
                    break;
                case BossState.Flying:
                    UpdateFlying();
                    break;
                case BossState.WindingUp:
                    UpdateWindingUp(dt);
                    break;
                case BossState.Charging:
                    UpdateCharging(dt);
                    break;
                case BossState.Falling:
                    UpdateFalling();
                    break;
                case BossState.Stunned:
                    UpdateStunned();
                    break;
                case BossState.Recovering:
                    UpdateRecovering();
                    break;
            }
        }

        void UpdateEmerging()
        {
            float progress = Mathf.SmoothStep(0, 1, 1 - Mathf.Max(0, timer) / emergenceDuration);

            SetEmergencePosition(Vector2.Lerp(emergenceStart, home, progress));

            if (timer <= 0)
            {
                ClearEmergenceMask();
                ReleaseEntranceLock();
                body.simulated = true;

                State = BossState.Flying;
                timer = attackInterval;
            }
        }

        void UpdateFlying()
        {
            if (body.position.x <= home.x - patrolRadius) direction = 1;
            else if (body.position.x >= home.x + patrolRadius) direction = -1;

            float targetY = home.y + Mathf.Sin(flightTime * 2) * 0.35f;

            body.linearVelocity = new Vector2(direction * flightSpeed, Mathf.Clamp((targetY - body.position.y) * 4, -3, 3));

            if (timer <= 0 && PlayerInRange())
            {
                State = BossState.WindingUp;
                timer = windupDuration;

                body.linearVelocity = Vector2.zero;
            }
        }

        void UpdateWindingUp(float dt)
        {
            body.linearVelocity = Vector2.zero;

            visual.transform.rotation = Quaternion.Euler(0, 0, visual.transform.eulerAngles.z + spinSpeed * dt);

            if (timer <= 0)
            {
                Vector2 aim = player != null ? (Vector2)player.transform.position - body.position : Vector2.down;

                // 플레이어 방향 돌진
                aim.y = Mathf.Min(-1f, aim.y);
                chargeDirection = aim.normalized;
                State = BossState.Charging;
                timer = chargeTimeout;

                body.linearVelocity = chargeDirection * chargeSpeed;
            }
        }

        void UpdateCharging(float dt)
        {
            visual.transform.Rotate(0, 0, spinSpeed * dt, Space.Self);
            body.linearVelocity = chargeDirection * chargeSpeed;

            if (timer <= 0) FallStunned();
        }

        void UpdateFalling()
        {
            if (OnFloor()) LandStunned();
            // 땅에 떨어지면 다시 원위치

            else if (body.position.y < home.y - 18f)
            {
                body.position = home;

                BeginRecovery();
            }
        }

        void UpdateStunned()
        {
            if (!OnFloor())
            {
                FallStunned();

                return;
            }

            body.linearVelocity = new Vector2(0, body.linearVelocity.y);

            if (timer <= 0) BeginRecovery();
        }

        void UpdateRecovering()
        {
            body.linearVelocity = Vector2.ClampMagnitude((home - body.position) * 3, recoverySpeed);

            if ((home - body.position).sqrMagnitude < 0.04f)
            {
                State = BossState.Flying;
                timer = attackInterval;
            }
        }

        bool PlayerInRange() => player != null && player.isActiveAndEnabled && player.CurrentHealth > 0 &&
            ((Vector2)player.transform.position - body.position).sqrMagnitude <= detectionRange * detectionRange;

        //AiCode -------
        bool OnFloor()
        {
            int count = gameObject.scene.GetPhysicsScene2D().CircleCast(body.position,
                hitbox.radius * Mathf.Abs(transform.lossyScale.x), Vector2.down, 0.09f, groundFilter, groundHits);
            for (int i = 0; i < count; i++)
            {
                var hit = groundHits[i];
                if (hit.collider == hitbox || hit.normal.y < 0.5f || hit.collider.GetComponentInParent<PlayerController2D>() != null)
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
        // -------------

        public void ReceiveFireball(float sourceX)
        {
            // 채공 상태 무적 활성화
            if (State != BossState.Stunned || !OnFloor())
            {
                return;
            }
            FireballHits++;
            HealthChanged?.Invoke(RemainingHits, MaxFireballHits);
            shakeRemaining = damageShakeDuration;
            if (FireballHits >= fireballHitsToDefeat)
            {
                Defeat(sourceX);
                return;
            }
        }

        void FallStunned()
        {
            State = BossState.Falling;
            hitbox.radius = stunnedColliderRadius;

            body.gravityScale = fallGravity;
            body.linearVelocity = new Vector2(0, Mathf.Min(0, body.linearVelocity.y));

            visual.sprite = stunnedSprite != null ? stunnedSprite : flyingSprite;
            visual.color = stunnedSprite != null ? normalColor : new Color(0.65f, 0.8f, 1f, normalColor.a);
        }

        void LandStunned()
        {
            if (State == BossState.Stunned)
            {
                return;
            }

            State = BossState.Stunned;
            GameAudio.Play(GameSound.BossImpact);
            timer = stunDuration;

            body.linearVelocity = Vector2.zero;

            if (impactCamera != null) impactCamera.Shake(landingShakeDuration, landingShakeStrength);
        }

        // AiCode -------
        void BeginRecovery()
        {
            bool grounded = OnFloor();
            float growth = (normalColliderRadius - hitbox.radius) * Mathf.Abs(transform.lossyScale.y);
            hitbox.radius = normalColliderRadius;

            if (grounded && growth > 0) body.position += Vector2.up * (growth + 0.01f);

            State = BossState.Recovering;
            visual.transform.localRotation = Quaternion.identity;
            body.gravityScale = 0;
            visual.color = normalColor;

            if (flyingSprite != null) visual.sprite = flyingSprite;
        }
        //----------

        void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);

        void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);

        void HandleCollision(Collision2D collision)
        {


            if (IsDying)
            {
                return;
            }



            var target = collision.gameObject.GetComponentInParent<PlayerController2D>();



            if (target != null)
            {
                if (State == BossState.Stunned)
                {
                    for (int i = 0; i < collision.contactCount; i++)
                        if (collision.GetContact(i).normal.y < -0.5f && collision.relativeVelocity.y < -0.1f &&
                            target.transform.position.y > transform.position.y)
                        {
                            target.Bounce(stompBounceSpeed);
                            GameAudio.Play(GameSound.Stomp);
                            return;
                        }
                    return;
                }
                target.ReceiveEnemyHit(transform.position);
                return;
            }



            if (collision.collider.isTrigger || (collision.rigidbody != null && collision.rigidbody.bodyType != RigidbodyType2D.Static))
            {
                return;
            }



            if (State == BossState.Charging) FallStunned();



            if (State == BossState.Falling)
                for (int i = 0; i < collision.contactCount; i++)
                    if (collision.GetContact(i).normal.y > 0.5f && OnFloor())
                    {
                        LandStunned();
                        return;
                    }



            if (State == BossState.Flying)
                for (int i = 0; i < collision.contactCount; i++)
                    if (Mathf.Abs(collision.GetContact(i).normal.x) > 0.5f) direction = collision.GetContact(i).normal.x > 0 ? 1 : -1;
        }


        void Defeat(float sourceX)
        {

            if (IsDying)
            {
                return;
            }


            shakeRemaining = damageShakeDuration;
            State = BossState.Defeated;
            GameAudio.Play(GameSound.BossDeath);

            if (impactCamera != null) impactCamera.Shake(deathShakeDuration, deathShakeStrength);

            OpenDefeatBlocks();


            hitbox.enabled = false;

            body.gravityScale = fallGravity;
            body.constraints = RigidbodyConstraints2D.None;

            float away = body.position.x >= sourceX ? 1 : -1;

            body.linearVelocity = new Vector2(away * 3, 7);
            body.angularVelocity = -away * 360;

            visual.color = normalColor;
            visual.sortingOrder = 20;

            Destroy(gameObject, 4);
        }


        // AiCode -------
        void OpenDefeatBlocks()
        {
            if (defeatBlocks == null)
            {
                return;
            }
            
            defeatBlocks.ClearAllTiles();

            var tileCollider = defeatBlocks.GetComponent<TilemapCollider2D>();

            if (tileCollider == null)
            {
                return;
            }

            tileCollider.ProcessTilemapChanges();

            var composite = tileCollider.attachedRigidbody != null
                ? tileCollider.attachedRigidbody.GetComponent<CompositeCollider2D>() : null;

            if (composite != null) composite.GenerateGeometry();
        }
        //---------------




        // AiCode -------
        void LateUpdate()
        {
            float dt = Time.deltaTime;

            embedAmount = Mathf.MoveTowards(embedAmount, State == BossState.Stunned ? 1 : 0, dt / 0.12f);

            Vector3 offset = Vector3.down * (groundEmbedDepth * embedAmount);

            visual.transform.localScale = Vector3.one;


            if (shakeRemaining > 0)
            {
                shakeRemaining = Mathf.Max(0, shakeRemaining - dt);

                float elapsed = damageShakeDuration - shakeRemaining;
                float strength = damageShakeAmount * (shakeRemaining / damageShakeDuration);

                offset += new Vector3(Mathf.Sin(elapsed * damageShakeFrequency * Mathf.PI * 2) * strength,
                    Mathf.Cos(elapsed * damageShakeFrequency * Mathf.PI * 2) * strength * 0.4f, 0);
            }


            visual.transform.localPosition = offset;
        }
        // ----------------




        void OnDisable()
        {
            ReleaseEntranceLock();

            if (impactCamera != null) impactCamera.ClearFocus(transform);


            shakeRemaining = embedAmount = 0;


            if (visual != null)
            {
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one;
            }

        }

        void ReleaseEntranceLock()
        {
            if (!entranceLocksPlayer)
            {
                return;
            }


            entranceLocksPlayer = false;

            
            if (player != null) player.SetControlLocked(false);
        }

        void OnDestroy()
        {
            ReleaseEntranceLock();
            ClearEmergenceMask();
        }
    }
}
