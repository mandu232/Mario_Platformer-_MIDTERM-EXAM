using UnityEngine;
using UnityEngine.UI;

namespace Midterm
{
    public sealed class BossHealthHud : MonoBehaviour
    {
        [SerializeField] Image[] hearts;
        [SerializeField] Sprite fullHeart;
        [SerializeField] Sprite halfHeart;
        [SerializeField] Sprite emptyHeart;
        [SerializeField, Min(0.01f)] float revealInterval = 0.16f;
        [SerializeField, Min(0.01f)] float punchDuration = 0.34f;
        FlyingBoss boss;
        float elapsed;
        bool presented;
        Vector2[] restPositions;

        void Awake()
        {
            restPositions = new Vector2[hearts.Length];
            for (int i = 0; i < hearts.Length; i++)
            {
                restPositions[i] = hearts[i].rectTransform.anchoredPosition;
                hearts[i].enabled = false;
            }
        }

        // AiCode -------
        public void ShowEntrance(FlyingBoss source)
        {
            if (boss != null) boss.HealthChanged -= Refresh;
            boss = source;
            boss.HealthChanged += Refresh;
            elapsed = 0;
            presented = true;
            for (int i = 0; i < hearts.Length; i++) hearts[i].enabled = false;
            Refresh(boss.RemainingHits, boss.MaxFireballHits);
        }
        // AiCode -------

        void OnEnable()
        {
            if (boss != null)
            {
                boss.HealthChanged += Refresh;
                Refresh(boss.RemainingHits, boss.MaxFireballHits);
            }
        }

        void OnDisable()
        {
            if (boss != null) boss.HealthChanged -= Refresh;
        }

        void Refresh(int health, int maximum)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(i * 2 < maximum);
                int remaining = health - i * 2;
                if (remaining >= 2)
                {
                    hearts[i].sprite = fullHeart;
                }
                else if (remaining == 1)
                {
                    hearts[i].sprite = halfHeart;
                }
                else
                {
                    hearts[i].sprite = emptyHeart;
                }
            }
            if (health <= 0) Hide();
        }

        void Hide()
        {
            presented = false;
            for (int i = 0; i < hearts.Length; i++) hearts[i].enabled = false;
        }

        void Update()
        {
            if (!presented)
            {
                return;
            }
            if (boss == null || boss.IsDying)
            {
                Hide();
                return;
            }
            elapsed += Time.deltaTime;
            for (int i = 0; i < hearts.Length; i++)
            {
                AnimateHeart(i);
            }
        }

        // AiCode -------
        void AnimateHeart(int index)
        {
            float age = elapsed - index * revealInterval;
            var image = hearts[index];
            image.enabled = age >= 0;
            if (age < 0)
            {
                return;
            }

            float progress = Mathf.Clamp01(age / punchDuration);
            float decay = Mathf.Pow(1 - progress, 2);
            float scale = 1 + Mathf.Cos(progress * Mathf.PI * 4) * decay * 0.8f;
            float angle = Mathf.Sin(progress * Mathf.PI * 6) * decay * 18;
            float height = Mathf.Sin(progress * Mathf.PI * 3) * decay * 12;

            var rect = image.rectTransform;
            rect.localScale = Vector3.one * scale;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            rect.anchoredPosition = restPositions[index] + Vector2.up * height;
        }
        // AiCode -------
    }
}
