using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VS.UI
{
    [RequireComponent(typeof(VideoPlayer))]
    public class MenuVideoBackground : MonoBehaviour
    {
        [SerializeField] private RawImage targetImage;
        [SerializeField] private VideoClip videoClip;

        void Awake()
        {
            var vp = GetComponent<VideoPlayer>();
            var rt = new RenderTexture(1080, 1920, 0);

            vp.clip = videoClip;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt;
            vp.audioOutputMode = VideoAudioOutputMode.None;
            vp.isLooping = true;
            vp.Play();

            targetImage.texture = rt;
        }
    }
}
