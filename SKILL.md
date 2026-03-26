---
name: unity-review
description: Unity C# 코드를 리뷰한다. GetComponent 남용, Update 퍼포먼스 이슈, ScriptableObject 직렬화 문제, 멀티플레이어 동기화 패턴, 네임스페이스 구조를 체크한다. 코드 리뷰 요청이나 "이 코드 괜찮아?"라고 물을 때 자동으로 사용한다.
---

# Unity C# 코드 리뷰어

## 리뷰 순서
반드시 아래 순서대로 분석한다:

1. 퍼포먼스 안티패턴
2. Unity 아키텍처 문제
3. 직렬화 / 데이터 구조
4. 멀티플레이어 / 동기화 (해당하는 경우)
5. 개선 제안

## 1. 퍼포먼스 안티패턴 체크

다음 패턴을 발견하면 반드시 지적한다:

- `Update()`나 `FixedUpdate()` 안에서 `GetComponent<>()` 호출
- `Find()`, `FindObjectOfType()` 반복 호출
- 매 프레임 문자열 연산 또는 `new` 키워드로 객체 생성
- Coroutine 안에서 `WaitForSeconds` 매번 new 생성 (캐싱 권장)
- 불필요한 `Camera.main` 반복 접근

## 2. Unity 아키텍처 체크

- MonoBehaviour 의존성이 과도하게 강결합되어 있는지 확인
- 싱글톤 패턴이 남용되고 있는지 확인
- 이벤트/델리게이트 구독 후 OnDestroy에서 해제하는지 확인
- `[SerializeField]`와 `public` 필드 사용이 적절한지 확인

## 3. ScriptableObject / 직렬화 체크

- `ISerializationCallbackReceiver` 구현이 올바른지 확인
- Dictionary를 직렬화할 때 커스텀 직렬화 사용 여부 확인
- SO 데이터 파이프라인에서 null 참조 가능성 확인
- `[CreateAssetMenu]` 속성 누락 여부 확인

## 5. 출력 형식

리뷰 결과는 반드시 아래 형식으로 출력한다:

### 심각도 분류
- 🔴 **Critical** - 즉시 수정 필요 (버그, 크래시 가능성)
- 🟡 **Warning** - 권장 수정 (퍼포먼스, 유지보수성)
- 🟢 **Suggestion** - 개선 아이디어 (선택 사항)

### 출력 예시
```
🔴 Critical: Update()에서 GetComponent<Rigidbody>() 호출 발견
   → Awake() 또는 Start()에서 캐싱 후 변수로 사용할 것

🟡 Warning: OnDestroy()에서 이벤트 구독 해제 누락
   → onPlayerDied -= HandlePlayerDied 추가 필요

🟢 Suggestion: 매직 넘버 3.5f를 const 또는 SerializeField로 분리 고려
```

## 마무리
리뷰 후 항상 요약 한 줄을 마지막에 출력한다:
"총 N개 이슈 발견: Critical X개 / Warning Y개 / Suggestion Z개"