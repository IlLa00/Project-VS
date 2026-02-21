using UnityEngine;
using UnityEngine.EventSystems;
using VS.Player;

namespace VS.UI
{
    /// <summary>
    /// 다이나믹 조이스틱: 화면을 누른 위치에 조이스틱이 나타남.
    /// 전체(또는 하단) 화면을 덮는 투명 Panel에 attach.
    /// _joystickRoot = BackGround+Knob을 담는 부모 (Canvas 직속 자식, 기본 비활성)
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _joystickRoot; // 누른 위치로 이동할 루트
        [SerializeField] private RectTransform _knob;
        [SerializeField] private float _radius = 80f;

        public Vector2 Direction { get; private set; }

        private RectTransform _canvasRect;

        void Awake()
        {
            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            _joystickRoot.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 누른 위치로 조이스틱 루트 이동 후 활성화
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 canvasPos);

            _joystickRoot.localPosition = canvasPos;
            _joystickRoot.gameObject.SetActive(true);
            _knob.localPosition = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _joystickRoot, eventData.position, eventData.pressEventCamera, out Vector2 localPos);

            localPos = Vector2.ClampMagnitude(localPos, _radius);
            _knob.localPosition = localPos;
            Direction = localPos / _radius;

            PlayerController.Instance?.SetMoveDirection(Direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            _knob.localPosition = Vector2.zero;
            _joystickRoot.gameObject.SetActive(false);
            PlayerController.Instance?.SetMoveDirection(Vector2.zero);
        }
    }
}
