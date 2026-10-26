using UnityEngine;

namespace Midterm
{
    public sealed class SessionHudView : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Text livesText;
        [SerializeField] UnityEngine.UI.Text scoreText;
        [SerializeField] UnityEngine.UI.Text timerText;
        
        [SerializeField] Color normalTimeColor = Color.white;
        [SerializeField] Color warningTimeColor = new Color(1, .35f, .25f);
        [SerializeField, Min(0)] int warningSeconds = 30;

        int shownLives = -1, shownScore = -1, shownSeconds = -1;

        public void Refresh(int lives, int score, int seconds)
        {
            if (shownLives != lives) { livesText.text = "LIVES " + lives; shownLives = lives; }
            if (shownScore != score) { scoreText.text = "SCORE " + score; shownScore = score; }
            if (shownSeconds == seconds) return;
            timerText.text = "TIME " + seconds.ToString("000");
            timerText.color = seconds <= warningSeconds ? warningTimeColor : normalTimeColor;
            shownSeconds = seconds;
        }
    }
}
