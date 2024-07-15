#if ENABLE_ADDRESSABLES && ENABLE_CMSUNIVORTEX
using UnityEngine;
using UnityEngine.UI;

namespace Uniprom
{
    public sealed class UnipromWallViewContent : MonoBehaviour
    {
        [SerializeField] Image _image;
        [SerializeField] Button _button;
        [SerializeField] Text _title;
        [SerializeField] Text _description;

        public IUnipromIconModel Model { get; private set; }

        UnipromWallView _view;
        
        public void Initialize(UnipromWallView view, IUnipromIconModel model)
        {
            _view = view;
            Model = model;
            _image.sprite = model.Sprite;
            _title.text = model.Model.GetAppName();
            _description.text = model.Model.GetAppDescription();
        }

        void Start() => _button.onClick.AddListener(ClickButton);

        void ClickButton()
        {
            if (_view != default)
            {
                _view.ClickContent(this);
            }
        }
    }
}
#endif