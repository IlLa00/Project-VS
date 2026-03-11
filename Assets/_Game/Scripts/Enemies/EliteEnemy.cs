using System.Collections;
using UnityEngine;
using VS.Core;
using VS.Player;

namespace VS.Enemies
{
    public class EliteEnemy : EnemyBase
    {
        [Header("원형 범위 공격")]
        [SerializeField] private float areaAttackInterval = 15f;
        [SerializeField] private float areaAttackWindupTime = 1f;
        [SerializeField] private float areaAttackRadius = 5f;
        [SerializeField] private float areaAttackDamage = 40f;

        [Header("조준 발사체 공격")]
        [SerializeField] private float projectileAttackInterval = 5f;
        [SerializeField] private float aimDuration = 0.5f;
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float projectileMaxRange = 20f;
        [SerializeField] private float projectileDetectionRange = 1000f;
        [SerializeField] private GameObject enemyProjectilePrefab;

        [Header("비주얼 머티리얼")]
        [SerializeField] private Material lineMaterial;

        private LineRenderer _circleRenderer;
        private LineRenderer _aimRenderer;
        private const int CircleSegments = 36;

        // 공격 중복 방지
        private bool _isAttacking;
        private bool _pendingProjectile;

        protected override void Awake()
        {
            base.Awake();
            _circleRenderer = CreateLineRenderer(Color.red, 0.08f);
            _circleRenderer.positionCount = CircleSegments + 1;
            _circleRenderer.gameObject.SetActive(false);

            _aimRenderer = CreateLineRenderer(new Color(1f, 0.9f, 0f), 0.05f);
            _aimRenderer.positionCount = 2;
            _aimRenderer.gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(AreaAttackRoutine());
            StartCoroutine(ProjectileAttackRoutine());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isAttacking = false;
            _pendingProjectile = false;
            if (_circleRenderer != null)
                _circleRenderer.gameObject.SetActive(false);

            if (_aimRenderer != null)
                _aimRenderer.gameObject.SetActive(false);
        }


        private IEnumerator AreaAttackRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(areaAttackInterval);
                // 투사체 공격 중이거나 대기 중이면 양보
                while (_isAttacking || _pendingProjectile)
                    yield return null;
                _isAttacking = true;
                yield return StartCoroutine(DoAreaAttack());
                _isAttacking = false;
            }
        }

        private IEnumerator DoAreaAttack()
        {
            _circleRenderer.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < areaAttackWindupTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / areaAttackWindupTime);
                float radius = Mathf.Lerp(0f, areaAttackRadius, t);
                DrawCircle(_circleRenderer, radius);
                yield return null;
            }

            DrawCircle(_circleRenderer, areaAttackRadius);

            var pc = PlayerController.Instance;
            if (pc != null)
            {
                float dist = Vector2.Distance(transform.position, pc.Transform.position);
                if (dist <= areaAttackRadius)
                    pc.GetComponent<PlayerStats>()?.TakeDamage(areaAttackDamage);
            }

            yield return new WaitForSeconds(0.2f);
            _circleRenderer.gameObject.SetActive(false);
        }

        private IEnumerator ProjectileAttackRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(projectileAttackInterval);
                _pendingProjectile = true;
                // 다른 공격이 끝날 때까지 대기 (우선순위 확보)
                while (_isAttacking)
                    yield return null;
                _pendingProjectile = false;
                _isAttacking = true;
                yield return StartCoroutine(DoProjectileAttack());
                _isAttacking = false;
            }
        }

        private IEnumerator DoProjectileAttack()
        {
            var pc = PlayerController.Instance;

            if (pc == null)
                yield break;

            // 감지 거리 밖이면 공격 취소
            if (Vector2.Distance(transform.position, pc.Transform.position) > projectileDetectionRange)
                yield break;

            Transform playerTransform = pc.Transform;

            _aimRenderer.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < aimDuration)
            {
                elapsed += Time.deltaTime;
                _aimRenderer.SetPosition(0, transform.position);
                _aimRenderer.SetPosition(1, playerTransform.position);
                yield return null;
            }
            _aimRenderer.gameObject.SetActive(false);


            if (enemyProjectilePrefab == null) 
                yield break;

            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            GameObject projGO = Instantiate(enemyProjectilePrefab, transform.position, Quaternion.identity);
            var proj = projGO.GetComponent<EnemyProjectile>();
            proj?.Init(dir, projectileSpeed, projectileDamage, projectileMaxRange);
        }

        private void DrawCircle(LineRenderer lr, float radius)
        {
            for (int i = 0; i <= CircleSegments; i++)
            {
                float angle = (float)i / CircleSegments * 2f * Mathf.PI;
                lr.SetPosition(i, new Vector3(
                    transform.position.x + Mathf.Cos(angle) * radius,
                    transform.position.y + Mathf.Sin(angle) * radius,
                    0f));
            }
        }

        private LineRenderer CreateLineRenderer(Color color, float width)
        {
            var go = new GameObject("LineIndicator");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();

            if (lineMaterial != null)
                lr.material = lineMaterial;
            else
                lr.material = new Material(Shader.Find("Sprites/Default"));
                
            lr.startColor = lr.endColor = color;
            lr.startWidth = lr.endWidth = width;
            lr.useWorldSpace = true;
            lr.sortingOrder = 2;
            return lr;
        }
    }
}
