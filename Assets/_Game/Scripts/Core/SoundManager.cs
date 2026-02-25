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

        [Header("볼륨")]
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.volume = bgmVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.volume = sfxVolume;
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
            _sfxSource.PlayOneShot(clip, sfxVolume);
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
