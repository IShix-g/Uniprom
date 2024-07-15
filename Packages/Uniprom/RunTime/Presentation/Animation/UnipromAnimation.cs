#if ENABLE_CMSUNIVORTEX
using System;
using UnityEngine;

namespace Uniprom
{
    public sealed class UnipromAnimation
    {
        public readonly float StartValue;
        public readonly float EndValue;
        public float Value { get; private set; }
        public float Duration { get; private set; }
        public UnipromEaseType Ease { get; private set; }
        public bool IsCompleted => _elapsedTime > Duration;
        public bool IsPlaying { get; private set; }
        public bool IsDestroyed { get; private set; }
        
        Action<float> _updateAction;
        Action<UnipromAnimation> _completedAction;
        float _elapsedTime;
    
        public UnipromAnimation(
            float startValue,
            float endValue,
            float duration,
            bool autoPlay = true,
            UnipromEaseType ease = UnipromEaseType.Liner)
        {
            StartValue = startValue;
            EndValue = endValue;
            Duration = duration;
            Ease = ease;
            Value = StartValue;
            
            if (autoPlay)
            {
                Play();
            }
        }
        
        public void Update(float deltaTime)
        { 
            if (IsCompleted)
            {
                if (IsPlaying)
                {
                    _completedAction?.Invoke(this);
                    _completedAction = default;
                    _updateAction = default;
                }
                IsPlaying = false;
                return;
            }
            _elapsedTime += deltaTime;
            
            var t = _elapsedTime / Duration;
            switch (Ease)
            {
                case UnipromEaseType.Liner:
                    Value = Mathf.Lerp(StartValue, EndValue, t);
                    break;
                case UnipromEaseType.EaseIn:
                    Value = StartValue + (EndValue - StartValue) * (t * t);
                    break;
                case UnipromEaseType.EaseOut:
                    Value = StartValue + (EndValue - StartValue) * (-t * (t-2));
                    break;
            }
            _updateAction?.Invoke(Value);
        }

        public void Play()
        {
            if (!IsCompleted
                && !IsDestroyed)
            {
                IsPlaying = true;
            }
        }
        
        public void Stop()
        {
            if (!IsPlaying
                || IsDestroyed)
            {
                return;
            }
            IsPlaying = false;
            _completedAction?.Invoke(this);
            _updateAction = default;
            _completedAction = default;
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }
            if (IsPlaying)
            {
                Stop();
            }
            IsDestroyed = true;
        }

        public UnipromAnimation SetEase(UnipromEaseType type)
        {
            Ease = type;
            return this;
        }

        public UnipromAnimation OnUpdate(Action<float> updateAction)
        {
            _updateAction += updateAction;
            return this;
        }

        public UnipromAnimation OnCompleted(Action<UnipromAnimation> completedAction)
        {
            _completedAction += completedAction;
            return this;
        }
    }
}
#endif