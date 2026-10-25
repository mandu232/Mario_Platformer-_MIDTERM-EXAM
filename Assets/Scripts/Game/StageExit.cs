using UnityEngine;
using UnityEngine.InputSystem;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class StageExit : MonoBehaviour
    {
        [SerializeField] string nextScene = "Stage2";
        GameManager manager;
        BoxCollider2D doorTrigger;
        Collider2D playerCollider;

        public void ConfigureNextStage()
        {
            int nextIndex = gameObject.scene.buildIndex + 1;
            nextScene = nextIndex > 0 && nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings
                ? UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(nextIndex)
                : string.Empty;
        }

        void Start()
        {
            manager = FindFirstObjectByType<GameManager>();
            doorTrigger = GetComponent<BoxCollider2D>();
            if (manager != null && manager.Player != null)
                playerCollider = manager.Player.GetComponent<Collider2D>();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.upArrowKey.wasPressedThisFrame)
            {
                return;
            }
            TryEnter();
        }

        public bool TryEnter()
        {
            if (manager != null && playerCollider != null && doorTrigger.IsTouching(playerCollider))
            {
                manager.CompleteStage(nextScene);
                return manager.IsStageCleared;
            }
            return false;
        }
    }
}
