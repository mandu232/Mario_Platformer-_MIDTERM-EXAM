using UnityEngine;

namespace Midterm
{
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0, 2, -10);
        [SerializeField] float minimumY = 0f;
        float shakeRemaining;
        float shakeDuration;
        float shakeStrength;
        float shakeElapsed;
        Vector3 lastShake;
        Transform focusTarget;
        [SerializeField, Min(0.01f)] float focusReturnDuration = 0.65f;
        Vector3 returnStart;
        float returnElapsed;
        bool returning;

        public void FocusOn(Transform focus)
        {
            focusTarget = focus;
            returning = false;
        }

        public void ClearFocus(Transform focus)
        {
            if (focusTarget != focus)
            {
                return;
            }
            focusTarget = null;
            returnStart = transform.position - lastShake;
            returnElapsed = 0;
            returning = true;
        }

        public void Shake(float duration, float strength)
        {
            if (!isActiveAndEnabled || duration <= 0 || strength <= 0)
            {
                return;
            }
            if (shakeRemaining <= 0)
            {
                shakeStrength = 0;
            }
            shakeStrength = Mathf.Max(shakeStrength, strength);
            shakeDuration = Mathf.Max(shakeRemaining, duration);
            shakeRemaining = shakeDuration;
            shakeElapsed = 0;
        }

        public void Configure(Transform hero)
        {
            target = hero;
        }

        // AiCode -------
        void LateUpdate()
        {
            Vector3 position = transform.position - lastShake;
            Transform followed = target;
            if (focusTarget != null)
            {
                followed = focusTarget;
            }
            if (followed != null)
            {
                position = followed.position + offset;
                position.y = Mathf.Max(position.y, minimumY);
            }
            lastShake = Vector3.zero;
            if (returning && focusTarget == null)
            {
                returnElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(returnElapsed / focusReturnDuration);
                position = Vector3.Lerp(returnStart, position, Mathf.SmoothStep(0, 1, t));
                if (t >= 1) returning = false;
            }
            if (shakeRemaining > 0)
            {
                shakeElapsed += Time.deltaTime;
                shakeRemaining = Mathf.Max(0, shakeRemaining - Time.deltaTime);
                float strength = shakeStrength * shakeRemaining / shakeDuration;
                lastShake = new Vector3(Mathf.Sin(shakeElapsed * 97) * strength,
                    Mathf.Cos(shakeElapsed * 113) * strength * 0.7f, 0);
            }
            else shakeStrength = 0;
            transform.position = position + lastShake;
        }
        // AiCode -------

        void OnDisable()
        {
            transform.position -= lastShake;
            lastShake = Vector3.zero;
            shakeRemaining = shakeStrength = 0;
            focusTarget = null;
            returning = false;
        }
    }
}
