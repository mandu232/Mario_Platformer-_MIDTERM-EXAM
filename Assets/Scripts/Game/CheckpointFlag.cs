using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class CheckpointFlag : MonoBehaviour
    {
        [SerializeField] Transform spawnPoint;
        [SerializeField] SpriteRenderer flagVisual;
        [SerializeField] SpriteRenderer poleVisual;
        [SerializeField] Sprite[] flagFrames;
        [SerializeField, Min(0.01f)] float frameDuration = 0.2f;

        float timer;

        GameManager manager;

        public Transform SpawnPoint => spawnPoint;
        public bool IsActivated { get; private set; }

        void Awake()
        {
            manager = FindAnyObjectByType<GameManager>();

            SetActivated(false);
        }

        public void SetActivated(bool active)
        {
            IsActivated = active;
            flagVisual.color = poleVisual.color = Color.white;
            poleVisual.enabled = active;
            Vector3 position = flagVisual.transform.localPosition;
            position.y = active ? 1f : 0f;
            flagVisual.transform.localPosition = position;
        }

        void Update()
        {
            // AiCode ------
            if (flagFrames.Length == 0)
            {
                return;
            }

            timer = (timer + Time.deltaTime) % (Mathf.Max(0.01f, frameDuration) * flagFrames.Length);
            flagVisual.sprite = flagFrames[(int)(timer / Mathf.Max(0.01f, frameDuration))];
            // -------
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController2D>();

            if (manager != null && player != null && player == manager.Player && spawnPoint != null)
            {
                manager.ActivateCheckpoint(this);
            }
        }
    }
}
