using System;
using UnityEngine;

namespace VS.Weapons
{
    public class LightningStrike : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.3f;

        private SpriteRenderer _sr;
        private Action _onComplete;
        private float _timer;
        private bool _playing;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
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
            
            if (_sr != null)
            {
                float alpha = Mathf.Clamp01(_timer / lifetime);
                var c = _sr.color;
                _sr.color = new Color(c.r, c.g, c.b, alpha);
            }

            if (_timer <= 0f)
            {
                _playing = false;
                _onComplete?.Invoke();
            }
        }
    }
}
