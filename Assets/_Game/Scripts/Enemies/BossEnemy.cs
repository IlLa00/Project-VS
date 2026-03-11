using System.Collections;
using UnityEngine;
using VS.Player;

namespace VS.Enemies
{
    public class BossEnemy : EnemyBase
    {
        [Header("범위 폭탄 공격")]
        [SerializeField] private float bombInterval = 10f;
        [SerializeField] private float bombWindupTime = 0.5f;
        [SerializeField] private float bombRadius = 2.5f;
        [SerializeField] private float bombDamage = 30f;
        [SerializeField] private int bombCount = 3;
        [SerializeField] private float bombSpawnRange = 5f;

        [Header("콘 스턴 공격")]
        [SerializeField] private float coneInterval = 5f;
        [SerializeField] private float coneWindupTime = 1f;
        [SerializeField] private float coneRange = 6f;
        [SerializeField] private float coneHalfAngle = 40f;
        [SerializeField] private float stunDuration = 1f;

        [Header("비주얼 머티리얼")]
        [SerializeField] private Material lineMaterial;

        private LineRenderer[] _bombRenderers;
        private LineRenderer _coneRenderer;

        private bool _isBombAttacking;
        private bool _isConeAttacking;

        private const int CircleSegments = 36;
        private const int ConeSegments = 16;

        protected override void Awake()
        {
            base.Awake();

            _bombRenderers = new LineRenderer[bombCount];
            for (int i = 0; i < bombCount; i++)
            {
                _bombRenderers[i] = CreateLineRenderer(Color.red, 0.08f, CircleSegments + 1);
                _bombRenderers[i].gameObject.SetActive(false);
            }

            _coneRenderer = CreateLineRenderer(new Color(1f, 0.4f, 0f), 0.08f, ConeSegments + 3);
            _coneRenderer.gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(BombAttackRoutine());
            StartCoroutine(ConeAttackRoutine());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isBombAttacking = false;
            _isConeAttacking = false;

            if (_bombRenderers != null)
                foreach (var lr in _bombRenderers)
                    if (lr != null) lr.gameObject.SetActive(false);

            if (_coneRenderer != null)
                _coneRenderer.gameObject.SetActive(false);
        }

        // ── 폭탄 공격 ────────────────────────────────────────

        private IEnumerator BombAttackRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(bombInterval);
                while (_isConeAttacking) yield return null;
                _isBombAttacking = true;
                yield return StartCoroutine(DoBombAttack());
                _isBombAttacking = false;
            }
        }

        private IEnumerator DoBombAttack()
        {
            // 캐스트 시점 좌표 확정 (보스가 이동해도 원은 고정)
            Vector2[] centers = new Vector2[bombCount];
            for (int i = 0; i < bombCount; i++)
            {
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float dist  = Random.Range(0f, bombSpawnRange);
                centers[i]  = (Vector2)transform.position
                              + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                _bombRenderers[i].gameObject.SetActive(true);
            }

            // 원 성장 애니메이션
            float elapsed = 0f;
            while (elapsed < bombWindupTime)
            {
                elapsed += Time.deltaTime;
                float t      = Mathf.Clamp01(elapsed / bombWindupTime);
                float radius = Mathf.Lerp(0f, bombRadius, t);
                for (int i = 0; i < bombCount; i++)
                    DrawCircle(_bombRenderers[i], centers[i], radius);
                yield return null;
            }

            // 최종 크기로 고정 후 피해 판정
            for (int i = 0; i < bombCount; i++)
                DrawCircle(_bombRenderers[i], centers[i], bombRadius);

            var pc = PlayerController.Instance;
            if (pc != null)
            {
                Vector2 playerPos = (Vector2)pc.Transform.position;
                foreach (var c in centers)
                {
                    if (Vector2.Distance(playerPos, c) <= bombRadius)
                    {
                        pc.GetComponent<PlayerStats>()?.TakeDamage(bombDamage);
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < bombCount; i++)
                _bombRenderers[i].gameObject.SetActive(false);
        }

        // ── 콘 스턴 공격 ─────────────────────────────────────

        private IEnumerator ConeAttackRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(coneInterval);
                while (_isBombAttacking) yield return null;
                _isConeAttacking = true;
                yield return StartCoroutine(DoConeAttack());
                _isConeAttacking = false;
            }
        }

        private IEnumerator DoConeAttack()
        {
            var pc = PlayerController.Instance;
            if (pc == null) yield break;

            // 캐스트 시점 방향 고정
            Vector2 castDir = ((Vector2)pc.Transform.position - (Vector2)transform.position).normalized;

            _coneRenderer.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < coneWindupTime)
            {
                elapsed += Time.deltaTime;
                DrawCone(_coneRenderer, (Vector2)transform.position, castDir, coneHalfAngle, coneRange);
                yield return null;
            }

            // 피해 판정 (스턴만, 데미지 없음)
            if (pc != null)
            {
                Vector2 toPlayer = (Vector2)pc.Transform.position - (Vector2)transform.position;
                if (toPlayer.magnitude <= coneRange)
                {
                    float angle = Vector2.Angle(castDir, toPlayer.normalized);
                    if (angle <= coneHalfAngle)
                        pc.ApplyStun(stunDuration);
                }
            }

            yield return new WaitForSeconds(0.25f);
            _coneRenderer.gameObject.SetActive(false);
        }

        // ── 드로우 헬퍼 ───────────────────────────────────────

        private void DrawCircle(LineRenderer lr, Vector2 center, float radius)
        {
            lr.positionCount = CircleSegments + 1;
            for (int i = 0; i <= CircleSegments; i++)
            {
                float a = (float)i / CircleSegments * 2f * Mathf.PI;
                lr.SetPosition(i, new Vector3(
                    center.x + Mathf.Cos(a) * radius,
                    center.y + Mathf.Sin(a) * radius,
                    0f));
            }
        }

        private void DrawCone(LineRenderer lr, Vector2 origin, Vector2 dir, float halfAngleDeg, float range)
        {
            lr.positionCount = ConeSegments + 3;

            float baseAngle  = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - halfAngleDeg;
            float endAngle   = baseAngle + halfAngleDeg;

            // index 0: 원점
            lr.SetPosition(0, new Vector3(origin.x, origin.y, 0f));

            // index 1 ~ ConeSegments+1: 호(arc)
            for (int i = 0; i <= ConeSegments; i++)
            {
                float t = (float)i / ConeSegments;
                float a = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                lr.SetPosition(i + 1, new Vector3(
                    origin.x + Mathf.Cos(a) * range,
                    origin.y + Mathf.Sin(a) * range,
                    0f));
            }

            // 마지막: 원점으로 닫기
            lr.SetPosition(ConeSegments + 2, new Vector3(origin.x, origin.y, 0f));
        }

        private LineRenderer CreateLineRenderer(Color color, float width, int positionCount)
        {
            var go = new GameObject("LineIndicator");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = lineMaterial != null
                ? lineMaterial
                : new Material(Shader.Find("Sprites/Default"));
            lr.startColor  = lr.endColor  = color;
            lr.startWidth  = lr.endWidth  = width;
            lr.useWorldSpace = true;
            lr.sortingOrder  = 2;
            lr.positionCount = positionCount;
            return lr;
        }
    }
}
