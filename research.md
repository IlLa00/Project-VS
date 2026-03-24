# 프로젝트 VS - 3개 작업 병렬 구현 계획

## Context
출시 가속화를 위한 3가지 병렬 작업:
1. GC 스파이크 제거 (메모리 최적화)
2. Addressables 전환 준비 (v1.0 이후 적용 권장, 코드 가이드라인 제공)
3. AdMob 광고 삽입 준비 (SDK 미설치 상태에서도 컴파일 가능한 stub 구조)

---

## 작업 1: 메모리 최적화

### S등급 (즉시 수정)

#### 1. DamageFloaterSpawner.cs + DamageFloater.cs — ObjectPool 적용
- `DamageFloaterSpawner.cs`: `Instantiate` → `ObjectPool<DamageFloater>` 사용
- `DamageFloater.cs` Init 시그니처 변경: `Init(float, bool)` → `Init(float, bool, Action onComplete)`
- `DamageFloater.cs` 코루틴 마지막: `Destroy(gameObject)` → `_onComplete?.Invoke()`
- `DamageFloater.cs` 라인 37: `new Color(...)` → `baseColor.a = ...; label.color = baseColor;`

#### 2. EliteEnemy.cs — WaitForSeconds 캐싱
- 필드 추가: `_waitAreaInterval`, `_waitProjectileInterval`, `_waitShortDelay`
- `Awake()`에서 SerializeField 값으로 초기화
- 라인 72, 106, 114의 `new WaitForSeconds(...)` → 캐시된 필드 사용

#### 3. BossEnemy.cs — WaitForSeconds 캐싱
- 필드 추가: `_waitBombInterval`, `_waitConeInterval`, `_waitShortDelay`
- 라인 78, 129, 140, 178의 `new WaitForSeconds(...)` → 캐시된 필드 사용

#### 4. InfiniteBackground.cs — LateUpdate Vector3 할당 제거
- 필드 추가: `private Vector3 _tilePos;`
- 라인 76: `tile.position = new Vector3(px, py, 0f)` → `_tilePos.x=px; _tilePos.y=py; tile.position=_tilePos;`

#### 5. BeamWeapon.cs — Update Color 매프레임 생성 제거
- 필드 추가: `private Color _startColorCache;`
- 라인 92-93: `new Color(...)` → `_startColorCache = beamColor; _startColorCache.a = alpha; _line.startColor = _startColorCache;`
- `_endColorCache` 불변이므로 Awake에서 1회 초기화 후 재사용

### A등급

#### 6. LightningWeapon.cs 라인 77 — NonAlloc
- `static readonly Collider2D[] _overlapBuffer = new Collider2D[32]` 필드 추가
- `Physics2D.OverlapCircleAll` → `Physics2D.OverlapCircleNonAlloc` + for 루프

#### 7. BeamWeapon.cs 라인 107 — NonAlloc
- `static readonly RaycastHit2D[] _raycastBuffer = new RaycastHit2D[32]` 필드 추가
- `Physics2D.RaycastAll` → `Physics2D.RaycastNonAlloc` + for 루프

#### 8. EnemySpawner.cs 라인 173 — Camera.main 캐싱
- 필드: `private Camera _mainCamera;`
- `Start()`에서 `_mainCamera = Camera.main;`
- `GetSpawnPosition()` 내 `Camera.main` → `_mainCamera`

#### 9. LevelUpUI.cs 라인 48-51 — List 재사용
- 필드: `private readonly List<UpgradeDataBase> _applicableBuffer = new();`
- `ShowCards()` 내 `new List<>` 제거, `_applicableBuffer.Clear()` 후 재사용
- `PickRandom` 시그니처: `UpgradeDataBase[]` → `List<UpgradeDataBase>` (ToArray 제거)

#### 10. ProjectileWeapon.cs — GetNearestEnemy foreach → for
- `foreach (EnemyBase enemy in EnemyBase.ActiveEnemies)` → `var enemies = EnemyBase.ActiveEnemies; for (int i=0; i<enemies.Count; i++)`

#### 11. EnemyBase.cs — ActiveEnemies.Remove() O(n) → swap-and-pop O(1)
```csharp
int idx = ActiveEnemies.IndexOf(this);
if (idx >= 0) {
    ActiveEnemies[idx] = ActiveEnemies[^1];
    ActiveEnemies.RemoveAt(ActiveEnemies.Count - 1);
}
```

### B등급

#### 12. HUDController.cs — 타이머 string 생성 최소화
- 필드: `private int _lastTimerSeconds = -1;`
- Update의 타이머 갱신: 초가 바뀔 때만 `GetFormattedTime()` 호출

#### 13. HUDController.cs 라인 93, 99 — string interpolation → concat
- `$"Lv.{level}"` → `"Lv." + level`
- `$"처치 {count}"` → `"처치 " + count`

### ObjectPool 미적용 항목

#### 14. EnemySpawner.cs 라인 141 — BossEnemy 풀링
- `private ObjectPool<BossEnemy> _bossPool;` 추가
- `Start()`에서 bossPrefab으로 풀 초기화
- `Instantiate(bossPrefab, ...)` → `_bossPool.Get()` + 위치 설정

#### 15. EliteEnemy.cs 라인 155 — EnemyProjectile 풀링
- `private ObjectPool<EnemyProjectile> _projectilePool;` 추가
- `EnemyProjectile.Init`에 `Action<EnemyProjectile> onComplete` 콜백 파라미터 추가
- `Instantiate(...)` → `_projectilePool.Get()` + `_projectilePool.Return(p)` 콜백

---

## 작업 2: Addressables 가이드라인 (v1.0 이후 권장)

> 현재 Resources.Load가 3곳뿐이고 모두 Awake/Start 시점 1회 호출. 런타임 성능 영향 없음.
> 출시 이후 패치에서 적용 권장.

### 패키지 설치
`Packages/manifest.json` dependencies에 추가:
```json
"com.unity.addressables": "1.21.21"
```

### 변경 대상 파일
| 파일 | 라인 | 현재 | 변경 |
|------|------|------|------|
| `LevelUpUI.cs` | 17 | `Resources.LoadAll<UpgradeDataBase>("Upgrades")` | `Addressables.LoadAssetsAsync<UpgradeDataBase>` |
| `OrbitalWeapon.cs` | 39 | `Resources.Load<Sprite>("Weapons/Orbital")` | `[SerializeField]` Inspector 직결 (간단) |
| `LightningStrike.cs` | 21 | `Resources.LoadAll<Sprite>("Weapons/LightningStrike")` | `[SerializeField] Sprite[] frames` Inspector 직결 (간단) |

OrbitalWeapon, LightningStrike는 Addressables 대신 `[SerializeField]`로 Inspector 직결로 변경하는 것이 더 단순하고 빠름.

---

## 작업 3: AdMob 광고 삽입 준비

### 신규 파일
**`Assets/_Game/Scripts/Core/AdManager.cs`** — DontDestroyOnLoad 싱글톤

구조:
- `#if UNITY_ADS_ENABLE` 조건부 컴파일 (SDK 미설치 시 스텁 동작)
- `Start()`에서 `MobileAds.Initialize()` → `LoadInterstitial()`, `LoadRewarded()` 호출
- 배너: `ShowBanner()`, `HideBanner()`, `DestroyBanner()`
- 전면: `LoadInterstitial()`, `ShowInterstitial(Action onClosed)`
- 보상형: `LoadRewarded()`, `ShowRewarded(Action<bool> onReward)`
- 테스트 광고 ID 상수로 선언 (출시 전 실제 ID 교체 주석 포함)

### 기존 파일 수정

**`HUDController.cs`** — `Start()` 끝에:
```csharp
AdManager.Instance?.ShowBanner();
```

**`GameOverUI.cs`** — GameOver 상태 처리를 코루틴으로 전환:
```
OnStateChanged(GameOver)
 → SaveRecords()  ← 즉시 실행
 → StartCoroutine(ShowInterstitialThenPanel())
    → adManager.ShowInterstitial(() => closed = true)
    → while (!closed && elapsed < 3f) { elapsed += Time.unscaledDeltaTime; yield return null; }
    → panel.SetActive(true)
```
`Time.timeScale = 0` 환경이므로 `Time.unscaledDeltaTime` 사용 필수.

**`MainMenuUI.cs`** — `Start()` 끝에:
```csharp
AdManager.Instance?.ShowBanner();
```

### SDK 설치 절차 (광고 실제 표시 필요 시)
1. [googleads-mobile-unity releases](https://github.com/googleads/googleads-mobile-unity/releases) 에서 `.unitypackage` 다운로드
2. Assets > Import Package > Custom Package
3. Project Settings > Player > Scripting Define Symbols에 `UNITY_ADS_ENABLE` 추가

---

## 수정 대상 파일 목록

| 파일 | 작업 | 비고 |
|------|------|------|
| `UI/DamageFloaterSpawner.cs` | 1 (S) | 구조 변경 |
| `UI/DamageFloater.cs` | 1 (S) | Init 시그니처 변경 |
| `Enemies/EliteEnemy.cs` | 1 (S) + 15 | WaitForSeconds 캐싱 + 투사체 풀링 |
| `Enemies/BossEnemy.cs` | 1 (S) | WaitForSeconds 캐싱 |
| `Core/InfiniteBackground.cs` | 1 (S) | Vector3 필드 캐싱 |
| `Weapons/BeamWeapon.cs` | 1 (S) + 7 | Color 캐싱 + RaycastNonAlloc |
| `Weapons/LightningWeapon.cs` | 6 | OverlapCircleNonAlloc |
| `Enemies/EnemySpawner.cs` | 8 + 14 | Camera 캐싱 + BossEnemy 풀링 |
| `UI/LevelUpUI.cs` | 9 | List 버퍼 재사용 |
| `Weapons/ProjectileWeapon.cs` | 10 | for 루프 전환 |
| `Enemies/EnemyBase.cs` | 11 | swap-and-pop |
| `UI/HUDController.cs` | 12 + 13 + 광고 | 타이머 캐싱 + 배너 |
| `UI/GameOverUI.cs` | 광고 | 코루틴 구조로 변경 |
| `UI/MainMenuUI.cs` | 광고 | 배너 추가 |
| `Enemies/EnemyProjectile.cs` | 15 | Init 콜백 파라미터 추가 |
| **`Core/AdManager.cs`** | 광고 | **신규 생성** |

---

## 검증 방법

### 메모리 최적화
- Unity Profiler > CPU Usage > GC.Alloc 항목 확인
- BeamWeapon Update 행: GC Alloc 0B 확인
- InfiniteBackground LateUpdate 행: GC Alloc 0B 확인
- 데미지 숫자 활성화 후 Memory Profiler로 할당 감소 확인

### 광고
- `UNITY_ADS_ENABLE` 없이 빌드: 컴파일 오류 없음 확인
- Android 빌드 후 실기기에서 테스트 배너 표시 확인
- GameOver 시 전면광고 후 UI 정상 표시 확인 (3초 타임아웃 포함)
