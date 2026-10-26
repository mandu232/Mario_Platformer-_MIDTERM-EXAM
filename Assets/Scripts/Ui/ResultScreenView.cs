using UnityEngine;

namespace Midterm
{
    public sealed class ResultScreenView : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Text title;
        [SerializeField] UnityEngine.UI.Text details;
        [SerializeField] GameObject retryHint;
        public UnityEngine.UI.Text Title => title;

        public void Show(string heading, string description, bool retry)
        {
            title.text = heading;
            details.text = description;
            retryHint.SetActive(retry);
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
