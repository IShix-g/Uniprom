#if ENABLE_CMSUNIVORTEX
using UnityEngine;

namespace Uniprom
{
    public sealed class UnipromTransformAnimation
    {
        public readonly Vector3 StartValue;
        public readonly Vector3 EndValue;
        public bool IsPlaying => Animation.IsPlaying;
        public UnipromAnimation Animation { get; private set; }
        
        Transform _transform;
        
        public UnipromTransformAnimation(Transform transform, Vector3 startValue, Vector3 endValue, UnipromAnimation animation)
        {
            _transform = transform;
            StartValue = startValue;
            EndValue = endValue;
            Animation = animation;
            Animation.OnUpdate(OnUpdate);
        }
        
        void OnUpdate(float value) => _transform.position = Vector3.Lerp(StartValue, EndValue, value);

        public void Destroy()
        {
            Animation.Destroy();
            _transform = default;
        }
    }
}
#endif