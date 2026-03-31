using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        private AsyncOperationHandle<IList<Sprite>> _handle;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _handle = Addressables.LoadAssetsAsync<Sprite>("LightningStrikeFrames", null);
            _handle.Completed += OnFramesLoaded;
        }

        private void OnFramesLoaded(AsyncOperationHandle<IList<Sprite>> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("LightningStrike 스프라이트 로드 실패");
                return;
            }
            _frames = new Sprite[handle.Result.Count];
            handle.Result.CopyTo(_frames, 0);
            _frameCount = _frames.Length;
        }

        void OnDestroy()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);
        }

        void OnEnable()
        {
            _playing = false;
        }

        public void Play(Action onComplete)
        {
            if (_frameCount == 0)
            {
                onComplete?.Invoke();
                return;
            }
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
