using UnityEngine;

namespace ZaoMeng.Gameplay
{
    /// <summary>巡逻：来回走，玩家进圈 → 追。</summary>
    public class PatrolState : IState<MonsterStateType>
    {
        private readonly Monster monster;
        private float direction = -1f;      // 先朝素材默认朝向走
        private float flipTimer;

        public MonsterStateType Id => MonsterStateType.Patrol;
        public PatrolState(Monster monster) => this.monster = monster;

        public void Enter(MonsterStateType previous) => monster.Play(MonsterAnim.Walk);
        public void Exit(MonsterStateType next) { }

        public void Tick()
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= 2f)            // 每 2 秒掉头（M3 换边缘检测，别走出平台）
            {
                flipTimer = 0f;
                direction = -direction;
            }

            monster.Rb.velocity = new Vector2(direction * monster.PatrolSpeed, monster.Rb.velocity.y);
            monster.Face(direction);

            if (monster.DistanceToTarget <= monster.DetectRange)
            {
                monster.ChangeState(MonsterStateType.Chase);
            }
        }
    }

    /// <summary>追击：朝玩家跑，进攻击距离 → 出手，跟丢了 → 回巡逻。</summary>
    public class ChaseState : IState<MonsterStateType>
    {
        private readonly Monster monster;
        public MonsterStateType Id => MonsterStateType.Chase;
        public ChaseState(Monster monster) => this.monster = monster;

        public void Enter(MonsterStateType previous) => monster.Play(MonsterAnim.Walk);
        public void Exit(MonsterStateType next) { }

        public void Tick()
        {
            float dist = monster.DistanceToTarget;

            if (dist > monster.LoseRange)                       // 迟滞区间：防在边界来回抖
            {
                monster.ChangeState(MonsterStateType.Patrol);
                return;
            }
            if (dist <= monster.AttackRange)
            {
                monster.ChangeState(MonsterStateType.Attack);
                return;
            }

            float dir = Mathf.Sign(monster.Target.position.x - monster.transform.position.x);
            monster.Rb.velocity = new Vector2(dir * monster.ChaseSpeed, monster.Rb.velocity.y);
            monster.Face(dir);
        }
    }

    /// <summary>攻击：站定出一刀，打完回追击。</summary>
    public class AttackState : IState<MonsterStateType>
    {
        private readonly Monster monster;
        private float timer;

        public MonsterStateType Id => MonsterStateType.Attack;
        public AttackState(Monster monster) => this.monster = monster;

        public void Enter(MonsterStateType previous)
        {
            timer = 0f;
            monster.Play(MonsterAnim.Attack);
            monster.Stop();                 // 出招站定——攻击是有_commitment_的
        }

        public void Exit(MonsterStateType next) { }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= monster.AttackDuration)
            {
                monster.ChangeState(MonsterStateType.Chase);
            }
        }
    }

    /// <summary>受击：硬直 + 击退，恢复后回追击。</summary>
    public class HurtState : IState<MonsterStateType>
    {
        private readonly Monster monster;
        private float timer;

        public MonsterStateType Id => MonsterStateType.Hurt;
        public HurtState(Monster monster) => this.monster = monster;

        public void Enter(MonsterStateType previous)
        {
            timer = 0f;
            monster.Play(MonsterAnim.Hurt);
            // 击退 = 攻击来向的水平力 + 一点垂直弹起
            // LastHitDirection: 1 表示玩家朝右打，怪物应向右飞；-1 则向左
            monster.Rb.velocity = new Vector2(
            monster.LastHitDirection * monster.KnockbackForce,
            monster.KnockbackUp);                                                // 垂直小弹起（固定值）
        }


        public void Exit(MonsterStateType next) { }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= monster.HurtDuration)
            {
                monster.ChangeState(MonsterStateType.Chase);
            }
        }
    }

    /// <summary>死亡：终态。M2-2 接对象池回收。</summary>
    /// <summary>死亡：播完死亡动画后离场。M2-2B 换成回对象池。</summary>
    public class DeadState : IState<MonsterStateType>
    {
        private readonly Monster monster;
        private float timer;

        public MonsterStateType Id => MonsterStateType.Dead;
        public DeadState(Monster monster) => this.monster = monster;

        public void Enter(MonsterStateType previous)
        {
            timer = 0f;
            monster.Play(MonsterAnim.Dead);
            monster.Stop();
        }

        public void Exit(MonsterStateType next) { }

        public void Tick()
        {
            timer += Time.deltaTime;
            if (timer >= monster.DeathDuration)
            {
                monster.Deactivate();
            }
        }
    }
}