using UnityEngine;

namespace PhoenixReborn.Core
{

    public class BootState : IState
    {
        private readonly StateMachine _flow;
        private readonly SceneLoader _sceneLoader;

        public BootState(StateMachine flow, SceneLoader sceneLoader)
        {
            _flow = flow;
            _sceneLoader = sceneLoader;
        }

        public void Enter()
        {
            Debug.Log("[GameFlow] Boot -> MainMenu");
            _flow.ChangeState(new MainMenuState(_flow, _sceneLoader));
        }

        public void Tick(float deltaTime) { }
        public void Exit() { }
    }

    public class MainMenuState : IState
    {
        private readonly StateMachine _flow;
        private readonly SceneLoader _sceneLoader;

        public MainMenuState(StateMachine flow, SceneLoader sceneLoader)
        {
            _flow = flow;
            _sceneLoader = sceneLoader;
        }

        public void Enter() => _sceneLoader.LoadContentScene("01_MainMenu");
        public void Tick(float deltaTime) { }
        public void Exit() { }

        public void StartStoryMode() => _flow.ChangeState(new StoryModeState(_flow, _sceneLoader));
        public void StartArcadeMode() => _flow.ChangeState(new ArcadeModeState(_flow, _sceneLoader));
    }

    public class StoryModeState : IState
    {
        private readonly StateMachine _flow;
        private readonly SceneLoader _sceneLoader;

        public StoryModeState(StateMachine flow, SceneLoader sceneLoader)
        {
            _flow = flow;
            _sceneLoader = sceneLoader;
        }

        public void Enter() => _sceneLoader.LoadContentScene("02_StoryMode");
        public void Tick(float deltaTime) { }
        public void Exit() { }

        public void ReturnToMenu() => _flow.ChangeState(new MainMenuState(_flow, _sceneLoader));
    }

    public class ArcadeModeState : IState
    {
        private readonly StateMachine _flow;
        private readonly SceneLoader _sceneLoader;

        public ArcadeModeState(StateMachine flow, SceneLoader sceneLoader)
        {
            _flow = flow;
            _sceneLoader = sceneLoader;
        }

        public void Enter() => _sceneLoader.LoadContentScene("03_ArcadeMode");
        public void Tick(float deltaTime) { }
        public void Exit() { }

        public void ReturnToMenu() => _flow.ChangeState(new MainMenuState(_flow, _sceneLoader));
    }
}
