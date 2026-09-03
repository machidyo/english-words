using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using EnglishWords.Runtime.UI;

namespace EnglishWords.Runtime.GameFlow
{
    public sealed class TitleController : MonoBehaviour
    {
        private const string MAIN_SCENE_NAME = "Main";

        [SerializeField] private GameFlowAsset _gameFlow;
        [SerializeField] private CanvasGroupFader _titleFader;

        private bool _isMainLoaded;

        private void OnEnable()
        {
            _gameFlow.OnRetryRequested += Retry;
            _gameFlow.OnReturnToTitleRequested += ReturnToTitle;
        }

        private void OnDisable()
        {
            _gameFlow.OnRetryRequested -= Retry;
            _gameFlow.OnReturnToTitleRequested -= ReturnToTitle;
        }

        private void Update()
        {
            if (_isMainLoaded)
                return;
            if (!Keyboard.current.enterKey.wasPressedThisFrame)
                return;

            _titleFader.Hide();
            LoadMain();
        }

        // Mainを丸ごと破棄して読み直すことで、進行中の状態とメモリをリセットする
        private void Retry() => SceneManager.UnloadSceneAsync(MAIN_SCENE_NAME).completed += _ => LoadMain();

        private void ReturnToTitle() => SceneManager.UnloadSceneAsync(MAIN_SCENE_NAME).completed += _ =>
        {
            _isMainLoaded = false;
            _titleFader.Show();
        };

        private void LoadMain()
        {
            _isMainLoaded = true;
            SceneManager.LoadSceneAsync(MAIN_SCENE_NAME, LoadSceneMode.Additive);
        }
    }
}
