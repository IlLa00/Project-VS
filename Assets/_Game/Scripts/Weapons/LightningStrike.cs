using System;
using UnityEngine;

namespace VS.Weapons
{
    public class LightningStrike : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.4f;

        private SpriteRenderer _sr;
        private Action _onComplete;
        private float _timer;
        private bool _playing;

        private Sprite[] _frames;
        private int _frameCount;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _frames = Resources.LoadAll<Sprite>("Weapons/LightningStrike");
            _frameCount = _frames?.Length ?? 0;
        }

        void OnEnable()
        {
            _playing = false;
        }

        public void Play(Action onComplete)
        {
            _onComplete = onComplete;
            _timer = lifetime;
            _playing = true;

            if (_sr != null)
                _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 1f);
        }

        void Update()
        {
            if (!_playing)
                return;

            _timer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(_timer / lifetime);

            if (_sr != null && _frameCount > 0)
            {
                int frameIndex = Mathf.Min(Mathf.FloorToInt(t * _frameCount), _frameCount - 1);
                _sr.sprite = _frames[frameIndex];
            }

            if (_timer <= 0f)
            {
                _playing = false;
                _onComplete?.Invoke();
            }
        }
    }
}
