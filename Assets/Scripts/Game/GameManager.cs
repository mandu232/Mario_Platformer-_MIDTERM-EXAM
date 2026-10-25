using UnityEngine;

namespace Midterm
{
    public sealed class GameManager : MonoBehaviour
    {





        [SerializeField] PlayerController2D player;
        [SerializeField] GameHud hud;
        [SerializeField] Transform spawnPoint;
        [SerializeField] float fallY = -6f;

        [SerializeField] UnityEngine.UI.Text stageMessage;

        [SerializeField, Min(0)] float clearDelay = 1.5f;
        [SerializeField, Min(0)] float deathRestartDelay = 1.25f;





        public bool IsStageCleared { get; private set; }

        public bool IsRestarting { get; private set; }




        public void CompleteStage(string nextScene)
        {
            if (IsStageCleared || player == null || player.CurrentHealth <= 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(nextScene) && !UnityEngine.Application.CanStreamedLevelBeLoaded(nextScene))
            {
                Debug.LogError("Next stage is missing from Build Settings: " + nextScene, this);
                return;
            }

            IsStageCleared = true;
            if (string.IsNullOrEmpty(nextScene)) GameAudio.Play(GameSound.FinalClear);
            else GameAudio.Play(GameSound.Clear);
            player.enabled = false;

            var body = player.GetComponent<Rigidbody2D>();

            body.linearVelocity = Vector2.zero;
            body.simulated = false;

            if (stageMessage != null)
            {
                stageMessage.text = string.IsNullOrEmpty(nextScene) ? "ALL STAGES CLEAR!" : "STAGE CLEAR!";
                stageMessage.gameObject.SetActive(true);
            }

            if (!string.IsNullOrEmpty(nextScene)) StartCoroutine(LoadNextStage(nextScene));
        }





        System.Collections.IEnumerator LoadNextStage(string nextScene)
        {
            yield return new WaitForSeconds(clearDelay);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextScene);
        }





        Vector3 initialSpawnPosition;

        CheckpointFlag activeCheckpoint;





        public void ActivateCheckpoint(CheckpointFlag checkpoint)
        {
            if (checkpoint == null || activeCheckpoint == checkpoint)
            {
                return;
            }

            if (activeCheckpoint != null) activeCheckpoint.SetActivated(false);

            activeCheckpoint = checkpoint;
            spawnPoint = checkpoint.SpawnPoint;

            checkpoint.SetActivated(true);
        }





        public PlayerController2D Player => player;

        public int Coins { get; private set; }

        public event System.Action<int> CoinsChanged;





        public void CollectCoin()
        {
            GameAudio.Play(GameSound.Coin);
            Coins++;
            CoinsChanged?.Invoke(Coins);
        }





        void Start()
        {
            if (player == null || hud == null)
            {
                Debug.LogError("GameManager requires Player and Hud references.", this);
                enabled = false;
                return;
            }

            initialSpawnPosition = player.transform.position;

            hud.Bind(player);
            hud.BindCoins(this);

            player.HealthChanged += OnPlayerHealthChanged;
        }





        void OnPlayerHealthChanged(int health, int maximum)
        {
            if (health > 0 || IsRestarting || IsStageCleared)
            {
                return;
            }

            IsRestarting = true;

            if (stageMessage != null)
            {
                stageMessage.text = "YOU DIED";
                stageMessage.gameObject.SetActive(true);
            }

            StartCoroutine(RestartStage());
        }





        System.Collections.IEnumerator RestartStage()
        {
            yield return new WaitForSeconds(deathRestartDelay);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameObject.scene.path);
        }





        void OnDestroy()
        {
            if (player != null) player.HealthChanged -= OnPlayerHealthChanged;
        }





        void FixedUpdate()
        {
            if (IsStageCleared || IsRestarting)
            {
                return;
            }

            if (player.transform.position.y < fallY)
            {
                player.TakeDamage(1);
                if (!player.IsDead)
                {
                    Vector3 respawnPosition = initialSpawnPosition;
                    if (spawnPoint != null)
                    {
                        respawnPosition = spawnPoint.position;
                    }
                    player.Respawn(respawnPosition);
                }
            }
        }


        public void DamagePlayer(int amount) => player.TakeDamage(amount);

        public void HealPlayer(int amount) => player.Heal(amount);


    }
}
