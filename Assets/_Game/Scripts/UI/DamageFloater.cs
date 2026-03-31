using DG.Tweening;
using TMPro;
using UnityEngine;

namespace VS.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DamageFloater : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float floatDistance = 1.2f;
        [SerializeField] private float lifetime = 0.8f;

        public void Init(float damage, bool isKill, System.Action onComplete)
        {
            label.text = Mathf.RoundToInt(damage).ToString();
            label.color = isKill ? Color.red : Color.white;
            transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);

            var cg = GetComponent<CanvasGroup>();
            cg.alpha = 1f;

            DOTween.Sequence()
                .Join(transform.DOLocalMoveY(transform.localPosition.y + floatDistance, lifetime).SetEase(Ease.OutQuad))
                .Join(cg.DOFade(0f, lifetime).SetEase(Ease.OutQuad))
                .SetUpdate(false)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}
