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

        public void Init(float damage, bool isKill)
        {
            label.text = Mathf.RoundToInt(damage).ToString();
            label.color = isKill ? Color.red : Color.white;
            // 여러 숫자 겹침 방지용 랜덤 수평 오프셋
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

                // fadeDelay 이후부터 알파 감소 (디졸브)
                if (elapsed > fadeDelay)
                {
                    float t = (elapsed - fadeDelay) / (lifetime - fadeDelay);
                    label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
