using UnityEngine;
using ZaoMeng.Data;        // ← 新增：MonsterConfig 在这个命名空间
using ZaoMeng.Events;
using ZaoMeng.Services;

namespace ZaoMeng.Gameplay
{
    public class Monster : MonoBehaviour
    {
        // ===== 场景引用（prefab 里无法保存，必须 Init 注入）=====
        private Transform target;
        private ObjectPool<Monster> pool;

        [Header("数据层配置")]
        [SerializeField] private MonsterConfig config;   // ← 新增：所有数值从这里读

        [Header("表现引用")]
        [SerializeField] private SpriteRenderer bodyRenderer;

        [Header("事件")]
        [SerializeField] private MonsterDiedEvent diedEvent;

        // ⚠️ 删掉的旧字段：attackDamage / patrolSpeed / chaseSpeed / detectRange /
        //   loseRange / attackRange / attackDuration / hurtDuration / knockbackForce /
        //   maxHp / deathDuration —— 全部搬进 config，下面的属性改读 config

        private Rigidbody2D rb;
        private Animator animator;
        private StateMachine<MonsterStateType> fsm;
        private int hp;

        // ===== 数值属性：全部改为读 config =====
        public Transform Target => target;
        public float KnockbackUp => config.KnockbackUp;
        public Rigidbody2D Rb => rb;
        public float PatrolSpeed => config.PatrolSpeed;
        public float ChaseSpeed => config.ChaseSpeed;
        public float DetectRange => config.DetectRange;
        public float LoseRange => config.LoseRange;
        public float AttackRange => config.AttackRange;
        public float AttackDuration => config.AttackDuration;
        public float HurtDuration => config.HurtDuration;
        public float KnockbackForce => config.KnockbackForce;
        public float DeathDuration => config.DeathDuration;
        public float DistanceToTarget => Vector2.Distance(transform.position, target.position);
        public float LastHitDirection { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            fsm = new StateMachine<MonsterStateType>();
            fsm.Register(new PatrolState(this), MonsterStateType.Chase, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new ChaseState(this), MonsterStateType.Patrol, MonsterStateType.Attack, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new AttackState(this), MonsterStateType.Chase, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new HurtState(this), MonsterStateType.Patrol, MonsterStateType.Chase, MonsterStateType.Dead);
            fsm.Register(new DeadState(this));
        }

        // 对象池 Get() 时 SetActive(true) 触发，每次重生从这开始
        private void OnEnable()
        {
            hp = config.MaxHp;                 // ← 原来 hp = maxHp，现在读 config
            LastHitDirection = 0f;
            fsm.Start(MonsterStateType.Patrol);
        }

        private void Update() => fsm.Tick();

        public void Init(ObjectPool<Monster> monsterPool, Transform player)
        {
            pool = monsterPool;
            target = player;
        }

        public void OnAttackHit()
        {
            if (DistanceToTarget > AttackRange + 0.3f) return;
            float dir = Mathf.Sign(Target.position.x - transform.position.x);
            Target.GetComponent<Player>()?.TakeDamage(config.AttackDamage, dir);  // ← 读 config
        }

        public void TakeHit(int damage, float direction)
        {
            if (fsm.Current == MonsterStateType.Dead) return;

            hp -= damage;
            LastHitDirection = direction;

            if (hp <= 0)
            {
                diedEvent?.Raise();
                fsm.ChangeState(MonsterStateType.Dead);
            }
            else
            {
                fsm.ChangeState(MonsterStateType.Hurt);
            }
        }

        public void Deactivate()
        {
            if (pool != null) pool.Release(this);
            else gameObject.SetActive(false);
        }

        public void ChangeState(MonsterStateType next) => fsm.ChangeState(next);
        public void Play(int animHash) => animator.Play(animHash);
        public void Face(float moveDirection) => bodyRenderer.flipX = moveDirection > 0f;
        public void Stop() => rb.velocity = new Vector2(0f, rb.velocity.y);
    }
}