using UnityEngine;
using VS.Enemies;

namespace VS.Core
{
    public enum SoundType
    {
        PlayerHurt,
        EnemyHit,
        EnemyDie,
        XpCollect,
        LevelUp,
        GameOver,
        Shoot,
        BossSpawn,
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("BGM")]
        [SerializeField] private AudioClip bgmGame;

        [Header("SFX")]
        [SerializeField] private AudioClip sfxPlayerHurt;
        [SerializeField] private AudioClip sfxEnemyHit;
        [SerializeField] private AudioClip sfxEnemyDie;
        [SerializeField] private AudioClip sfxXpCollect;
        [SerializeField] private AudioClip sfxLevelUp;
        [SerializeField] private AudioClip sfxGameOver;
        [SerializeField] private AudioClip sfxShoot;
        [SerializeField] private AudioClip sfxBossSpawn;

        [Header("볼륨 기본값")]
        [SerializeField] [Range(0f, 1f)] private float defaultBgmVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float defaultSfxVolume = 1f;

        private const string KeyBgmVolume = "BGMVolume";
        private const string KeySfxVolume = "SFXVolume";

        private float _bgmVolume;
        private float _sfxVolume;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _bgmVolume = PlayerPrefs.GetFloat(KeyBgmVolume, defaultBgmVolume);
            _sfxVolume = PlayerPrefs.GetFloat(KeySfxVolume, defaultSfxVolume);

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.volume = _bgmVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.volume = _sfxVolume;
        }

        public void SetBgmVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _bgmVolume;
            PlayerPrefs.SetFloat(KeyBgmVolume, _bgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            _sfxSource.volume = _sfxVolume;
            PlayerPrefs.SetFloat(KeySfxVolume, _sfxVolume);
            PlayerPrefs.Save();
        }

        void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
            EnemySpawner.OnBossSpawned += HandleBossSpawned;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
            EnemySpawner.OnBossSpawned -= HandleBossSpawned;
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    PlayBGM();
                    break;
                case GameState.LevelUp:
                    Play(SoundType.LevelUp);
                    break;
                case GameState.GameOver:
                    StopBGM();
                    Play(SoundType.GameOver);
                    break;
            }
        }

        private void HandleBossSpawned(EnemyBase _)
        {
            Play(SoundType.BossSpawn);
        }

        public void Play(SoundType type)
        {
            AudioClip clip = type switch
            {
                SoundType.PlayerHurt => sfxPlayerHurt,
                SoundType.EnemyHit   => sfxEnemyHit,
                SoundType.EnemyDie   => sfxEnemyDie,
                SoundType.XpCollect  => sfxXpCollect,
                SoundType.LevelUp    => sfxLevelUp,
                SoundType.GameOver   => sfxGameOver,
                SoundType.Shoot      => sfxShoot,
                SoundType.BossSpawn  => sfxBossSpawn,
                _                    => null
            };

            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        private void PlayBGM()
        {
            if (bgmGame == null || (_bgmSource.clip == bgmGame && _bgmSource.isPlaying)) return;
            _bgmSource.clip = bgmGame;
            _bgmSource.Play();
        }

        private void StopBGM()
        {
            _bgmSource.Stop();
        }
    }
}
