using UnityEngine;

namespace PhoenixReborn.Core
{
    /// <summary>
    /// Lives only in 00_Bootstrap. Owns the systems that must survive
    /// the whole session and kicks off the game flow.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        public static Bootstrapper Instance { get; private set; }

        public StateMachine GameFlow { get; private set; }
        public SceneLoader SceneLoader { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneLoader = new SceneLoader();
            GameFlow = new StateMachine();
        }

        private void Start()
        {
            GameFlow.ChangeState(new BootState(GameFlow, SceneLoader));
        }

        private void Update()
        {
            GameFlow.Tick(Time.deltaTime);
        }
    }
}
