using System.Collections;
using TMPro;
using UnityEngine;

namespace VS.UI
{
    public class DamageFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float floatSpeed = 1.5f;
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private float fadeDelay = 0.5f;

        private System.Action _onComplete;

        public void Init(float damage, bool isKill, System.Action onComplete)
        {
            _onComplete = onComplete;
            label.text = Mathf.RoundToInt(damage).ToString();
            label.color = isKill ? Color.red : Color.white;
            transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            Color baseColor = label.color;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                transform.position += Vector3.up * floatSpeed * Time.deltaTime;

                if (elapsed > fadeDelay)
                {
                    float t = (elapsed - fadeDelay) / (lifetime - fadeDelay);
                    baseColor.a = 1f - t;
                    label.color = baseColor;
                }

                yield return null;
            }

            _onComplete?.Invoke();
        }
    }
}
