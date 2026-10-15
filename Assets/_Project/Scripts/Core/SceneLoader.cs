using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoenixReborn.Core
{
    /// <summary>
    /// Wraps additive scene loading/unloading so the rest of the
    /// codebase never touches SceneManager directly. Each call
    /// unloads the previous content scene before loading the next.
    /// </summary>
    public class SceneLoader
    {
        private string _currentContentScene;

        public void LoadContentScene(string sceneName, Action onLoaded = null)
        {
            _currentContentScene = sceneName;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"SceneLoader: could not load scene '{sceneName}'. Is it added to Build Settings?");
                return;
            }

            operation.completed += _ => onLoaded?.Invoke();
        }
    }
}
