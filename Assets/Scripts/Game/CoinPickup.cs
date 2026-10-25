using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class CoinPickup : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;

        [SerializeField, Min(0.01f)] float frameDuration = 0.15f;
        
        SpriteRenderer visual;
        GameManager manager;

        bool collected;
        float timer;

        UnityEngine.Tilemaps.Tilemap sourceTilemap;
        Vector3Int sourceCell;




        public void BindCell(UnityEngine.Tilemaps.Tilemap tilemap, Vector3Int cell)
        {
            sourceTilemap = tilemap;
            sourceCell = cell;
        }




        void Awake()
        {
            visual = GetComponent<SpriteRenderer>();
            manager = FindFirstObjectByType<GameManager>();
        }




        void Update()
        {
            if (frames.Length == 0)
            {
                return;
            }

            timer = (timer + Time.deltaTime) % (Mathf.Max(0.01f, frameDuration) * frames.Length);
            visual.sprite = frames[(int)(timer / Mathf.Max(0.01f, frameDuration))];
        }




        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerController2D>();

            if (collected || manager == null || player == null || player != manager.Player)
            {
                return;
            }

            collected = true;
            GetComponent<Collider2D>().enabled = false;
            visual.enabled = false;

            manager.CollectCoin();

            if (sourceTilemap != null)
            {
                sourceTilemap.SetTile(sourceCell, null);
            }

            Destroy(gameObject);
        }
    }
}
