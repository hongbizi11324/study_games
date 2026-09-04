using UnityEngine;
using ZaoMeng.Events;        // 新增：MonsterDiedEvent 在这个命名空间
using ZaoMeng.Services;      // 新增：ObjectPool 在这个命名空间

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 小怪宿主：拥有状态机、缓存组件、向状态提供服务。
    /// M2-2C 改造为"对象池友好"：prefab 不保存场景引用，由 Spawner Init 注入。
    /// </summary>
    public class Monster : MonoBehaviour
    {
        // ===== 场景引用（prefab 里无法保存，必须 Init 注入）=====
        private Transform target;
        private ObjectPool<Monster> pool;

        [Header("表现引用")]
        [SerializeField] private SpriteRenderer bodyRenderer;

        [Header("事件")]
        [SerializeField] private MonsterDiedEvent diedEvent;

        [Header("AI 参数")]
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float patrolSpeed = 1.5f;
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float detectRange = 5f;
        [SerializeField] private float loseRange = 7f;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackDuration = 0.8f;
        [SerializeField] private float hurtDuration = 0.3f;
        [SerializeField] private float knockbackForce = 4f;
        [SerializeField] private int maxHp = 3;
        [SerializeField] private float deathDuration = 0.8f;

        public float DeathDuration => deathDuration;

        // M2-2C：优先回对象池；如果没有池（比如旧场景手动拖的），回退到隐藏
        public void Deactivate()
        {
            if (pool != null)
                pool.Release(this);
            else
                gameObject.SetActive(false);
        }

        private Rigidbody2D rb;
        private Animator animator;
        private StateMachine<MonsterStateType> fsm;
        private int hp;

        public Transform Target => target;
        public Rigidbody2D Rb => rb;
        public float PatrolSpeed => patrolSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectRange => detectRange;
        public float LoseRange => loseRange;
        public float AttackRange => attackRange;
        public float AttackDuration => attackDuration;
        public float HurtDuration => hurtDuration;
        public float KnockbackForce => knockbackForce;
        public float DistanceToTarget => Vector2.Distance(transform.position, target.position);
        public float LastHitDirection { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // 一次性注册状态机。对象池复用时这里不会重新执行，所以不做 Start。
            fsm = new StateMachine<MonsterStateType>();
            fsm.Register(new PatrolState(this), MonsterStateType.Chase, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new ChaseState(this), MonsterStateType.Patrol, MonsterStateType.Attack, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new AttackState(this), MonsterStateType.Chase, MonsterStateType.Hurt, MonsterStateType.Dead);
            fsm.Register(new HurtState(this), MonsterStateType.Patrol, MonsterStateType.Chase, MonsterStateType.Dead);
            fsm.Register(new DeadState(this));
        }

        // 对象池 Get() 时会 SetActive(true)，触发这里。每次重生都从这里开始。
        private void OnEnable()
        {
            hp = maxHp;
            LastHitDirection = 0f;
            fsm.Start(MonsterStateType.Patrol);
        }

        private void Update() => fsm.Tick();

        /// <summary>
        /// Spawner 生成后调用。把 prefab 存不了的引用注进来。
        /// </summary>
        public void Init(ObjectPool<Monster> monsterPool, Transform player)
        {
            pool = monsterPool;
            target = player;
        }

        public void OnAttackHit()
        {
            if (DistanceToTarget > AttackRange + 0.3f) return;
            Target.GetComponent<Player>()?.TakeDamage(attackDamage);
        }

        public void TakeHit(int damage, float direction)
        {
            if (fsm.Current == MonsterStateType.Dead) return;

            hp -= damage;
            LastHitDirection = direction;

            if (hp <= 0)
            {
                diedEvent?.Raise();                   // 死前广播，通知 Spawner 安排重生
                fsm.ChangeState(MonsterStateType.Dead);
            }
            else
            {
                fsm.ChangeState(MonsterStateType.Hurt);
            }
        }

        public void ChangeState(MonsterStateType next) => fsm.ChangeState(next);
        public void Play(int animHash) => animator.Play(animHash);
        public void Face(float moveDirection) => bodyRenderer.flipX = moveDirection > 0f;
        public void Stop() => rb.velocity = new Vector2(0f, rb.velocity.y);
    }
}
