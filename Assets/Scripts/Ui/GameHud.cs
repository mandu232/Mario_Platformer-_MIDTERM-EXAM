using UnityEngine;
using UnityEngine.UI;

namespace Midterm
{
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] Image[] hearts;
        [SerializeField] Sprite fullHeart;
        [SerializeField] Sprite halfHeart;
        [SerializeField] Sprite emptyHeart;
        [SerializeField] Sprite[] numberSprites;
        [SerializeField] Image[] coinDigits;
        
        GameManager manager;

        // AiCode -------
        public void BindCoins(GameManager target)
        {
            if (manager != null) manager.CoinsChanged -= RefreshCoins;
            manager = target;
            if (isActiveAndEnabled && manager != null)
            {
                manager.CoinsChanged += RefreshCoins;
                RefreshCoins(manager.Coins);
            }
        }
        // AiCode -------

        void RefreshCoins(int count)
        {
            string value = count.ToString(System.Globalization.CultureInfo.InvariantCulture);
            for (int i = 0; i < coinDigits.Length; i++)
            {
                coinDigits[i].gameObject.SetActive(i < value.Length);
                if (i < value.Length) coinDigits[i].sprite = numberSprites[value[i] - '0'];
            }
        }

        PlayerController2D player;

        public void Bind(PlayerController2D target)
        {
            Unsubscribe();
            player = target;
            if (isActiveAndEnabled) Subscribe();
        }

        void OnEnable()
        {
            Subscribe();
            if (manager != null)
            {
                manager.CoinsChanged += RefreshCoins;
                RefreshCoins(manager.Coins);
            }
        }
        void OnDisable()
        {
            Unsubscribe();
            if (manager != null) manager.CoinsChanged -= RefreshCoins;
        }

        void Subscribe()
        {
            if (player == null)
            {
                return;
            }
            player.HealthChanged += Refresh;
            Refresh(player.CurrentHealth, player.MaxHealth);
        }

        void Unsubscribe()
        {
            if (player != null) player.HealthChanged -= Refresh;
        }

        void Refresh(int health, int maxHealth)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].gameObject.SetActive(i * 2 < maxHealth);
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
        }
    }
}
