using UnityEngine;
using UnityEngine.InputSystem;

namespace Midterm
{
    [RequireComponent(typeof(PlayerController2D))]
    public sealed class PlayerFireAttack : MonoBehaviour
    {
        [SerializeField] FireballProjectile projectilePrefab;
        [SerializeField, Min(0.05f)] float shotInterval = 0.3f;
        PlayerController2D player;
        Collider2D hitbox;
        float nextShot;
        public bool HasFireFlower { get; private set; }

        void Awake()
        {
            player = GetComponent<PlayerController2D>();
            hitbox = GetComponent<Collider2D>();
        }

        public void GrantFireFlower() => HasFireFlower = true;

        void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) TryShoot();
        }

        public bool TryShoot()
        {
            if (!HasFireFlower || projectilePrefab == null || !player.isActiveAndEnabled || player.IsControlLocked ||
                player.CurrentHealth <= 0 || Time.timeScale <= 0 || Time.time < nextShot) return false;
            nextShot = Time.time + shotInterval;
            int direction = player.FacingDirection;
            // 바로 앞의 벽이나 적도 감지하도록 발사 위치를 플레이어 가장자리 안쪽에 둔다.
            Vector2 origin = hitbox.bounds.center;
            origin.x += direction * hitbox.bounds.extents.x * 0.5f;
            var shot = Instantiate(projectilePrefab, origin, Quaternion.identity);
            shot.Launch(player, direction);
            GameAudio.Play(GameSound.Fireball);
            return true;
        }
    }
}
