using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Ecos
{
    /// <summary>
    /// Usaremos los enum para crear nuestros estados. Estos deben estar fuera de
    /// la declaracion de la clase.
    /// </summary>
    public enum GameStates
    {
        GameStart,
        InGame,
        GameOver
    }

    /// <summary>
    /// Core GameManager using GenericSingleton and UnityEvents, synchronized with GameSession.
    /// </summary>
    public class GameManager : GenericSingleton<GameManager>
    {
        public GameStates currentState = GameStates.GameStart;

        [Header("Unity Events")]
        public UnityEvent OnGameStartEvent = new UnityEvent();
        public UnityEvent OnInGameEvent = new UnityEvent();
        public UnityEvent OnGameOverEvent = new UnityEvent();

        public void SetState(GameStates newState)
        {
            if (currentState == newState) return;
            currentState = newState;

            switch (newState)
            {
                case GameStates.GameStart:
                    OnGameStartEvent?.Invoke();
                    break;
                case GameStates.InGame:
                    OnInGameEvent?.Invoke();
                    break;
                case GameStates.GameOver:
                    OnGameOverEvent?.Invoke();
                    break;
            }

            if (EventManager.Instance != null)
            {
                EventManager.Instance.TriggerGameStateChanged(newState);
            }
        }

        // Scene management helpers (matching MainMenu pattern)
        public void SceneChanger(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void SceneChanger(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }

        public void QuitApp()
        {
            Application.Quit();
        }
    }
}
