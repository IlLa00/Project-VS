using UnityEngine;
using VS.Player;

namespace VS.Core
{
    /// <summary>
    /// 카메라가 플레이어를 부드럽게 따라감. Cinemachine 불필요.
    /// Main Camera에 attach.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        private Transform _target;

        void LateUpdate()
        {
            // PlayerController가 씬에 없으면 아무것도 안 함
            if (_target == null)
            {
                if (PlayerController.Instance != null)
                    _target = PlayerController.Instance.Transform;
                return;
            }

            Vector3 goal = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            transform.position = goal;
        }
    }
}
