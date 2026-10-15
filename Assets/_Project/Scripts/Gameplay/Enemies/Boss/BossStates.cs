using UnityEngine;

namespace PhoenixReborn.Gameplay.Enemies.Boss
{
    public interface IBossState
    {
        void Enter(BossController boss);
        void Update(BossController boss, float deltaTime);
        void Exit(BossController boss);
    }

    public class BossIntroState : IBossState
    {
        private float _introTimer;
        private const float INTRO_DURATION = 2.5f;

        public void Enter(BossController boss)
        {
            _introTimer = 0f;
            BossController.NotifyBossIncoming();
            Systems.AudioManager.Instance?.PlayBossWarning();
            WaveManager.Instance?.EnableFiringForBoss();
            Arcade.ArcadeModeManager.Instance?.EnableFiringForBoss();
            Debug.Log("[Boss] State -> Intro (Descending into battle position)");
        }

        public void Update(BossController boss, float deltaTime)
        {
            _introTimer += deltaTime;

            boss.transform.position = Vector3.Lerp(boss.SpawnStartPosition, boss.BattleStartPosition, _introTimer / INTRO_DURATION);

            if (_introTimer >= INTRO_DURATION)
            {
                boss.ChangeState(new BossPhase1State());
            }
        }

        public void Exit(BossController boss) { }
    }

    public class BossPhase1State : IBossState
    {
        private float _shootTimer;

        public void Enter(BossController boss)
        {
            _shootTimer = boss.Phase1FireRate;
            Debug.Log("[Boss] State -> Phase 1 (Standard Movement & Firing)");
        }

        public void Update(BossController boss, float deltaTime)
        {
            boss.MoveHorizontally(deltaTime, boss.Phase1Speed);

            _shootTimer -= deltaTime;
            if (_shootTimer <= 0f)
            {
                boss.FireBossWeapons();
                _shootTimer = boss.Phase1FireRate;
            }

            if (boss.GetDestroyedBarrierRatio() >= 0.5f)
            {
                boss.ChangeState(new BossPhase2State());
            }
        }

        public void Exit(BossController boss) { }
    }

    public class BossPhase2State : IBossState
    {
        private float _shootTimer;

        public void Enter(BossController boss)
        {
            _shootTimer = boss.Phase2FireRate;
            Debug.Log("[Boss] State -> Phase 2 (Enraged: Faster Firing & Movement)");
        }

        public void Update(BossController boss, float deltaTime)
        {
            boss.MoveHorizontally(deltaTime, boss.Phase2Speed);

            _shootTimer -= deltaTime;
            if (_shootTimer <= 0f)
            {
                boss.FireBossWeapons();
                _shootTimer = boss.Phase2FireRate;
            }
        }

        public void Exit(BossController boss) { }
    }

    public class BossDefeatedState : IBossState
    {
        public void Enter(BossController boss)
        {
            Debug.Log("[Boss] State -> Defeated! Triggering explosion sequence.");
            WaveManager.Instance?.DisableFiring();
            boss.TriggerDeathSequence();
        }

        public void Update(BossController boss, float deltaTime) { }
        public void Exit(BossController boss) { }
    }
}
