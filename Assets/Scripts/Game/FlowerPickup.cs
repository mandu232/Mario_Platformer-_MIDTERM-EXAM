using System.Collections;
using UnityEngine;

namespace Midterm
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class FlowerPickup : MonoBehaviour
    {




        
        public enum FlowerKind
        {
            Heart,
            Fire
        }
        [SerializeField] FlowerKind kind;
        bool collected;
        bool emerging;





        public void Emerge(Vector3 destination, float duration)
        {
            emerging = true;
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(Rise(destination, duration));
        }




        // Ai Code ----
        IEnumerator Rise(Vector3 destination, float duration)
        {
            Vector3 origin = transform.position;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(origin, destination, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            transform.position = destination;
            emerging = false;
            GetComponent<Collider2D>().enabled = true;
        }
        // ----------




        void OnTriggerEnter2D(Collider2D other) => Collect(other);



        void OnTriggerStay2D(Collider2D other) => Collect(other);




        void Collect(Collider2D other)
        {
            if (collected || emerging)
            {
                return;
            }
            var player = other.GetComponentInParent<PlayerController2D>();
            if (player == null || !player.isActiveAndEnabled || player.CurrentHealth <= 0)
            {
                return;
            }
            if (kind == FlowerKind.Heart) player.Heal(1);
            else
            {
                var attack = player.GetComponent<PlayerFireAttack>();
                if (attack == null)
                {
                    return;
                }
                attack.GrantFireFlower();
            }
            collected = true;
            GameAudio.Play(GameSound.Upgrade);
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(gameObject);
        }
    }
}
