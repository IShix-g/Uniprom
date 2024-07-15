#if ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    public static class UnipromAnimationExtension
    {
        public static UnipromAnimation ScaleFromTo(this Transform transform, float startValue, float endValue, float duration, bool autoPlay = true)
            => transform.ScaleTo(
                new Vector3(startValue, startValue, startValue),
                new Vector3(endValue, endValue, endValue),
                duration,
                autoPlay);

        public static UnipromAnimation ScaleTo(this Transform transform, Vector3 startValue, Vector3 endValue, float duration, bool autoPlay = true)
        {
            var animation = UnipromManager.Instance.GetAnimation(0, 1, duration, autoPlay);
            animation.OnUpdate(t => transform.localScale = Vector3.Lerp(startValue, endValue, t));
            return animation;
        }
        
        public static UnipromAnimation FromTo(this Transform transform, Vector3 startValue, Vector3 endValue, float duration, bool autoPlay = true)
        {
            var animation = UnipromManager.Instance.GetAnimation(0, 1, duration, autoPlay);
            animation.OnUpdate(t => transform.position = Vector3.Lerp(startValue, endValue, t));
            return animation;
        }
        
        public static UnipromAnimation FadeTo(this CanvasGroup group, float endValue, float duration, bool autoPlay = true)
        {
            var animation = UnipromManager.Instance.GetAnimation(group.alpha, endValue, duration, autoPlay);
            animation.OnUpdate(t => group.alpha = t);
            return animation;
        }
        
        public static UnipromAnimation FadeTo(this Image image, float endValue, float duration, bool autoPlay = true)
        {
            var start = image.color;
            var end = image.color;
            end.a = endValue;
            var animation = UnipromManager.Instance.GetAnimation(0, 1, duration, autoPlay);
            animation.OnUpdate(t => image.color = Color.Lerp(start, end, t));
            return animation;
        }
    }
}
#endif