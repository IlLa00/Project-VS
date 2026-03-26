using UnityEngine;
using VS.Enemies;

namespace VS.Core
{
    public enum SoundType
    {
        // 플레이어
        PlayerHurt,
        PlayerDeath,
        PlayerHeal,

        // 적
        EnemyHit,
        EnemyDie,
        EliteSpawn,
        BossSpawn,
        BossAttack,
        EliteAttack,

        // 무기
        Shoot,
        BeamFire,
        LightningStrike,

        // XP / 레벨업
        XpCollect,
        LevelUp,
        UpgradeSelect,

        // UI / 게임 상태
        ButtonClick,
        GameOver,
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("BGM")]
        [SerializeField] private AudioClip bgmMainMenu;
        [SerializeField] private AudioClip bgmGame;

        [Header("SFX - 플레이어")]
        [SerializeField] private AudioClip sfxPlayerHurt;
        [SerializeField] private AudioClip sfxPlayerDeath;
        [SerializeField] private AudioClip sfxPlayerHeal;

        [Header("SFX - 적")]
        [SerializeField] private AudioClip sfxEnemyHit;
        [SerializeField] private AudioClip sfxEnemyDie;
        [SerializeField] private AudioClip sfxEliteSpawn;
        [SerializeField] private AudioClip sfxBossSpawn;
        [SerializeField] private AudioClip sfxBossAttack;
        [SerializeField] private AudioClip sfxEliteAttack;

        [Header("SFX - 무기")]
        [SerializeField] private AudioClip sfxShoot;
        [SerializeField] private AudioClip sfxBeamFire;
        [SerializeField] private AudioClip sfxLightningStrike;

        [Header("SFX - XP / 레벨업")]
        [SerializeField] private AudioClip sfxXpCollect;
        [SerializeField] private AudioClip sfxLevelUp;
        [SerializeField] private AudioClip sfxUpgradeSelect;

        [Header("SFX - UI / 게임 상태")]
        [SerializeField] private AudioClip sfxButtonClick;
        [SerializeField] private AudioClip sfxGameOver;

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
            DontDestroyOnLoad(gameObject);

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

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    PlayBGM(bgmGame);
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

        public void PlayMainMenuBGM()
        {
            PlayBGM(bgmMainMenu);
        }

        private void HandleBossSpawned(EnemyBase _)
        {
            Play(SoundType.BossSpawn);
        }

        public void Play(SoundType type)
        {
            AudioClip clip = type switch
            {
                SoundType.PlayerHurt      => sfxPlayerHurt,
                SoundType.PlayerDeath     => sfxPlayerDeath,
                SoundType.PlayerHeal      => sfxPlayerHeal,
                SoundType.EnemyHit        => sfxEnemyHit,
                SoundType.EnemyDie        => sfxEnemyDie,
                SoundType.EliteSpawn      => sfxEliteSpawn,
                SoundType.BossSpawn       => sfxBossSpawn,
                SoundType.BossAttack      => sfxBossAttack,
                SoundType.EliteAttack     => sfxEliteAttack,
                SoundType.Shoot           => sfxShoot,
                SoundType.BeamFire        => sfxBeamFire,
                SoundType.LightningStrike => sfxLightningStrike,
                SoundType.XpCollect       => sfxXpCollect,
                SoundType.LevelUp         => sfxLevelUp,
                SoundType.UpgradeSelect   => sfxUpgradeSelect,
                SoundType.ButtonClick     => sfxButtonClick,
                SoundType.GameOver        => sfxGameOver,
                _                         => null
            };

            if (clip == null) 
                return;

            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        private void PlayBGM(AudioClip clip)
        {
            if (clip == null || (_bgmSource.clip == clip && _bgmSource.isPlaying)) 
                return;
                
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }

        private void StopBGM()
        {
            _bgmSource.Stop();
        }
    }
}
