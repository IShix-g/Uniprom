#if ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Uniprom
{
    public sealed class UnipromCanvasCreator : MonoBehaviour
    {
        public RectTransform Transform
        {
            get
            {
                CreateCanvasIfNeeded();
                return _canvasTransform;
            }
        }
        
        public Canvas Canvas
        {
            get
            {
                CreateCanvasIfNeeded();
                return _canvas;
            }
        }

        Canvas _canvas;
        RectTransform _canvasTransform;

        void Awake()
        {
            CreateCanvasIfNeeded();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        
        void OnDestroy() => SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        public void CreateCanvasIfNeeded()
        {
            if (_canvas == default)
            {
                _canvas = CreateCanvasInternal();
                _canvasTransform = (RectTransform) _canvas.transform;
                _canvasTransform.SetParent(transform);
            }
            CreateEventSystemIfNeeded();
        }

        void OnActiveSceneChanged(Scene current, Scene next) => CreateEventSystemIfNeeded();

        static Canvas CreateCanvasInternal()
        {
            var go = new GameObject("Uniprom Canvas", new []{ typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster) });
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 99;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referencePixelsPerUnit = 100;
            var rayCaster = go.GetComponent<GraphicRaycaster>();
            rayCaster.ignoreReversedGraphics = true;
            rayCaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            rayCaster.blockingMask = ~0;
            return canvas;
        }

        static void CreateEventSystemIfNeeded()
        {
            if (EventSystem.current == default)
            {
                _ = new GameObject("EventSystem", new[] {typeof(EventSystem), typeof(StandaloneInputModule)});
            }
        }
    }
}
#endif