using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Midterm
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerController2D player;
        [SerializeField] GameHud hud;
        [SerializeField] Transform spawnPoint;
        [SerializeField] float fallY = -6f;
        [SerializeField, Min(0)] float clearDelay = 1.5f;
        [SerializeField, Min(0)] float deathRestartDelay = 1.25f;
        [SerializeField, Min(1)] int startingLives = 3;
        [SerializeField, Min(1)] float stageDuration = 180f;

        // Only deliberate stage transitions carry the current run to another scene.
        static string nextRunScene;
        static int nextLives, nextCoins, nextScore;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRunTransfer() => nextRunScene = null;

        public PlayerController2D Player => player;
        public UnityEngine.UI.Text StageMessage => hud.ResultTitle;
        public int Coins { get; private set; }
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsStageCleared { get; private set; }
        public bool IsRestarting { get; private set; }
        public bool IsGameOver { get; private set; }
        public event System.Action<int> CoinsChanged;
        Vector3 initialSpawnPosition;
        CheckpointFlag activeCheckpoint;
        Vector3 RespawnPosition => spawnPoint != null ? spawnPoint.position : initialSpawnPosition;

        void Start()
        {
            if (player == null || hud == null)
            {
                Debug.LogError("GameManager requires Player and Hud references.", this);
                enabled = false;
                return;
            }
            initialSpawnPosition = player.transform.position;
            Lives = startingLives;
            if (nextRunScene == gameObject.scene.path || nextRunScene == gameObject.scene.name)
            {
                Lives = nextLives;
                Coins = nextCoins;
                Score = nextScore;
            }
            nextRunScene = null;
            TimeRemaining = stageDuration;
            hud.Bind(player);
            hud.BindCoins(this);
            hud.HideResult();
            hud.RefreshSession(Lives, Score, Mathf.CeilToInt(TimeRemaining));
        }

        void OnEnable()
        {
            if (player != null) player.HealthChanged += OnPlayerHealthChanged;
        }

        void Update()
        {
            if (IsGameOver || IsStageCleared)
            {
                if ((IsGameOver || !IsRestarting) && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                    RetryStage();
                return;
            }
            // Entrance cinematics and death animations do not consume play time.
            if (!IsRestarting && !player.IsControlLocked) AdvanceTimer(Time.deltaTime);
        }

        void AdvanceTimer(float elapsed)
        {
            if (IsStageCleared || IsGameOver || IsRestarting) return;
            TimeRemaining = Mathf.Max(0, TimeRemaining - Mathf.Max(0, elapsed));
            hud.RefreshSession(Lives, Score, Mathf.CeilToInt(TimeRemaining));
            if (TimeRemaining <= 0) EndGame("TIME UP");
        }

        public void CollectCoin()
        {
            if (IsGameOver || IsStageCleared) return;
            GameAudio.Play(GameSound.Coin);
            Coins++;
            AddScore(100);
            CoinsChanged?.Invoke(Coins);
        }

        public void AddScore(int amount)
        {
            if (amount <= 0 || IsGameOver || IsStageCleared) return;
            Score += amount;
            hud.RefreshSession(Lives, Score, Mathf.CeilToInt(TimeRemaining));
        }

        public void ActivateCheckpoint(CheckpointFlag checkpoint)
        {
            if (checkpoint == null || checkpoint == activeCheckpoint || IsGameOver || IsStageCleared) return;
            if (activeCheckpoint != null) activeCheckpoint.SetActivated(false);
            activeCheckpoint = checkpoint;
            spawnPoint = checkpoint.SpawnPoint;
            checkpoint.SetActivated(true);
        }

        void OnPlayerHealthChanged(int health, int maximum)
        {
            if (health > 0 || IsRestarting || IsStageCleared || IsGameOver) return;
            Lives = Mathf.Max(0, Lives - 1);
            hud.RefreshSession(Lives, Score, Mathf.CeilToInt(TimeRemaining));
            if (Lives == 0)
            {
                EndGame("NO LIVES LEFT");
                return;
            }
            IsRestarting = true;
            ShowResult("YOU DIED", "RESPAWNING...\nLIVES " + Lives, false);
            StartCoroutine(RespawnAfterDeath());
        }

        System.Collections.IEnumerator RespawnAfterDeath()
        {
            yield return new WaitForSeconds(deathRestartDelay);
            if (IsGameOver || IsStageCleared) yield break;
            player.Revive(RespawnPosition);
            TimeRemaining = stageDuration;
            IsRestarting = false;
            hud.HideResult();
            hud.RefreshSession(Lives, Score, Mathf.CeilToInt(TimeRemaining));
        }

        void EndGame(string reason)
        {
            if (IsGameOver || IsStageCleared) return;
            IsGameOver = true;
            IsRestarting = false;
            StopPlayer();
            GameAudio.Play(GameSound.Death);
            ShowResult("GAME OVER", reason + "\nSCORE " + Score + "   COINS " + Coins, true);
        }

        void StopPlayer()
        {
            player.enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        void ShowResult(string title, string details, bool retry)
        {
            hud.ShowResult(title, details, retry);
        }

        public void CompleteStage(string nextScene)
        {
            if (IsStageCleared || IsGameOver || player == null || player.CurrentHealth <= 0) return;
            if (!string.IsNullOrEmpty(nextScene) && !Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Next stage is missing from Build Settings: " + nextScene, this);
                return;
            }
            IsStageCleared = true;
            bool final = string.IsNullOrEmpty(nextScene);
            GameAudio.Play(final ? GameSound.FinalClear : GameSound.Clear);
            StopPlayer();
            ShowResult(final ? "ALL STAGES CLEAR!" : "STAGE CLEAR!",
                "SCORE " + Score + "   COINS " + Coins + (final ? "" : "\nNEXT STAGE..."), final);
            if (!final)
            {
                IsRestarting = true;
                StartCoroutine(LoadNextStage(nextScene));
            }
        }

        System.Collections.IEnumerator LoadNextStage(string nextScene)
        {
            yield return new WaitForSeconds(clearDelay);
            nextRunScene = nextScene;
            nextLives = Lives;
            nextCoins = Coins;
            nextScore = Score;
            SceneManager.LoadSceneAsync(nextScene);
        }

        public void RetryStage()
        {
            if ((!IsGameOver && !IsStageCleared) || IsRestarting) return;
            IsRestarting = true;
            nextRunScene = null;
            SceneManager.LoadSceneAsync(gameObject.scene.path);
        }

        void FixedUpdate()
        {
            if (IsStageCleared || IsRestarting || IsGameOver || player == null) return;
            if (player.transform.position.y < fallY)
            {
                player.TakeDamage(player.CurrentHealth);
            }
        }

        void OnDisable()
        {
            if (player != null) player.HealthChanged -= OnPlayerHealthChanged;
        }

        public void DamagePlayer(int amount) => player.TakeDamage(amount);
        public void HealPlayer(int amount) => player.Heal(amount);
    }
}
