using System.Collections;
using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class ItemBox : MonoBehaviour
    {
        [SerializeField] FlowerPickup itemPrefab;
        [SerializeField] bool givesCoin;
        [SerializeField] Sprite coinSprite;
        [SerializeField] Sprite usedSprite;


        [SerializeField, Min(0)] float bumpHeight = 0.18f;
        [SerializeField, Min(0.01f)] float bumpDuration = 0.18f;
        [SerializeField, Min(0.01f)] float emergeDuration = 0.4f;

        public bool IsUsed { get; private set; }
        bool bumping;





        void OnCollisionEnter2D(Collision2D collision)
        {
            if (bumping)
            {
                return;
            }
            var player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player == null || player.CurrentHealth <= 0 ||
                player.transform.position.y >= transform.position.y || collision.relativeVelocity.y <= 0.1f) return;
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y > 0.5f)
                {
                    bool releaseItem = !IsUsed && (itemPrefab != null || givesCoin);
                    IsUsed = true;
                    bumping = true;
                    GameAudio.Play(GameSound.Box);
                    StartCoroutine(Release(releaseItem));
                    return;
                }
        }




        // Ai Code -----
        IEnumerator Release(bool releaseItem)
        {
            var visual = GetComponent<SpriteRenderer>();
            
            if (usedSprite != null) visual.sprite = usedSprite;

            // 박스의 충돌 위치는 고정하고 그림
            if (releaseItem && givesCoin)
            {
                var manager = FindAnyObjectByType<GameManager>();

                if (manager != null) manager.CollectCoin();
                if (coinSprite != null) StartCoroutine(PopCoin(visual));
            }

            else if (releaseItem && itemPrefab != null)
            {
                var item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                var itemVisual = item.GetComponent<SpriteRenderer>();

                itemVisual.sortingLayerID = visual.sortingLayerID;
                itemVisual.sortingOrder = visual.sortingOrder - 1;

                float rise = GetComponent<Collider2D>().bounds.extents.y + itemVisual.bounds.extents.y + 0.02f;

                item.Emerge(transform.position + Vector3.up * rise, emergeDuration);
            }



            // 임시 자식 오브젝트로 박스가 튀는 모습을 표시
            //수정필요
            var bump = new GameObject("BoxBump", typeof(SpriteRenderer));
            bump.transform.SetParent(transform, false);
            var bumpVisual = bump.GetComponent<SpriteRenderer>();

            bumpVisual.sprite = visual.sprite;
            bumpVisual.sharedMaterial = visual.sharedMaterial;
            bumpVisual.sortingLayerID = visual.sortingLayerID;
            bumpVisual.sortingOrder = visual.sortingOrder;
            visual.enabled = false;

            float elapsed = 0;

            while (elapsed < bumpDuration)
            {
                elapsed += Time.deltaTime;
                bump.transform.localPosition = Vector3.up * Mathf.Sin(Mathf.Clamp01(elapsed / bumpDuration) * Mathf.PI) * bumpHeight;
                yield return null;
            }

            visual.enabled = true;
            Destroy(bump);
            bumping = false;
        }
        // ----------





        IEnumerator PopCoin(SpriteRenderer boxVisual)
        {
            // 중복 획득 방지
            var coin = new GameObject("BoxCoin", typeof(SpriteRenderer));
            coin.transform.SetParent(transform, false);
            var renderer = coin.GetComponent<SpriteRenderer>();

            renderer.sprite = coinSprite;
            renderer.sharedMaterial = boxVisual.sharedMaterial;
            renderer.sortingLayerID = boxVisual.sortingLayerID;
            renderer.sortingOrder = boxVisual.sortingOrder + 1;

            float elapsed = 0;
            const float duration = 0.6f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                coin.transform.localPosition = Vector3.up * (0.5f + Mathf.Sin(t * Mathf.PI) * 1.2f);
                var color = Color.white;
                color.a = Mathf.Clamp01((1 - t) * 4);
                renderer.color = color;
                yield return null;
            }

            Destroy(coin);
        }
    }
}
