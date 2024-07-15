
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Test
{
    [RequireComponent(typeof(Button))]
    public sealed class ReloadSceneButton : MonoBehaviour
    {
        [SerializeField] Button _button;

        void Start() => _button.onClick.AddListener(ClickButton);

        void ClickButton() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        void Reset() => _button = GetComponent<Button>();
    }
}