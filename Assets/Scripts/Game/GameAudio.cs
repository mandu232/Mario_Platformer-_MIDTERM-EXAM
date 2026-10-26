using UnityEngine;
using System.Collections;

namespace Midterm
{
    public enum GameSound
    {
        Jump, Stomp, Coin, Upgrade, Fireball, Hit, Box, JumpPad,
        BossAppear, BossImpact, BossDeath, Clear, FinalClear, Death, EnemyDeath, BlockBreak, FireballBounce,
        Checkpoint, SwitchPress
    }

    public sealed class GameAudio : MonoBehaviour
    {
        [SerializeField] AudioSource effects;
        [SerializeField] AudioSource jingles;
        [SerializeField] AudioClip jump;
        [SerializeField] AudioClip stomp;
        [SerializeField] AudioClip coin;
        [SerializeField] AudioClip upgrade;
        [SerializeField] AudioClip fireball;
        [SerializeField] AudioClip hit;
        [SerializeField] AudioClip box;
        [SerializeField] AudioClip jumpPad;
        [SerializeField] AudioClip bossAppear;
        [SerializeField] AudioClip bossImpact;
        [SerializeField] AudioClip bossDeath;
        [SerializeField] AudioClip clear;
        [SerializeField] AudioClip finalClear;
        [SerializeField] AudioClip death;
        [SerializeField] AudioClip enemyDeath;
        [SerializeField] AudioClip blockBreak;
        [SerializeField] AudioClip fireballBounce;
        [SerializeField] AudioClip checkpoint;
        [SerializeField] AudioClip switchPress;
        static GameAudio instance;

        // AiCode -------
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // AiCode -------

        public static void Play(GameSound sound)
        {
            if (instance == null) return;
            instance.PlaySound(sound);
        }

        void PlaySound(GameSound sound)
        {
            AudioClip clip = null;
            bool isJingle = false;
            switch (sound)
            {
                case GameSound.Jump: clip = jump; break;
                case GameSound.Stomp: clip = stomp; break;
                case GameSound.Coin: clip = coin; break;
                case GameSound.Upgrade: clip = upgrade; break;
                case GameSound.Fireball: clip = fireball; break;
                case GameSound.Hit: clip = hit; break;
                case GameSound.Box: clip = box; break;
                case GameSound.JumpPad: clip = jumpPad; break;
                case GameSound.BossAppear: clip = bossAppear; break;
                case GameSound.BossImpact: clip = bossImpact; break;
                case GameSound.BossDeath: clip = bossDeath; break;
                case GameSound.Clear: clip = clear; isJingle = true; break;
                case GameSound.FinalClear: clip = finalClear; isJingle = true; break;
                case GameSound.Death: clip = death; isJingle = true; break;
                case GameSound.EnemyDeath: clip = enemyDeath; break;
                case GameSound.FireballBounce: clip = fireballBounce; break;
                case GameSound.Checkpoint: clip = checkpoint; break;
                case GameSound.SwitchPress: clip = switchPress; break;
                case GameSound.BlockBreak:
                    if (blockBreak != null) StartCoroutine(PlayBlockBreak());
                    return;
            }
            if (clip == null) return;
            if (isJingle)
            {
                jingles.Stop();
                jingles.clip = clip;
                jingles.Play();
            }
            else effects.PlayOneShot(clip);
        }

        IEnumerator PlayBlockBreak()
        {
            for (int i = 0; i < 3; i++)
            {
                effects.PlayOneShot(blockBreak, 1f - i * 0.2f);
                if (i < 2) yield return new WaitForSeconds(0.045f);
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
