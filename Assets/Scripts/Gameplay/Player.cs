using System.Collections;
using UnityEngine;
using ZaoMeng.Data;
using ZaoMeng.Events;
using ZaoMeng.Services;

namespace ZaoMeng.Gameplay
{
    public class Player : MonoBehaviour
    {
        [Header("数据层配置")]
        [SerializeField] private PlayerConfig config;

        [Header("表现层引用")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("M3 打击感")]
        [SerializeField] private HitStop hitStop;

        [Header("事件通道（发布）")]
        [SerializeField] private GameEvent attackEvent;
        [SerializeField] private PlayerHealthEvent healthEvent;
        [SerializeField] private ScreenShakeEvent shakeEvent;

        [Header("重生")]
        [SerializeField] private float respawnDelay = 2f;
        private int hp;
        private bool dead;
        private Vector2 spawnPosition;

        [Header("输入缓冲")]
        [SerializeField] private float attackBufferWindow = 0.2f;   // 缓冲窗口（秒）
        private float attackBufferTimer;                            // 缓冲剩余时间

        [Header("物理检测")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask monsterLayer;

        private readonly Collider2D[] groundHits = new Collider2D[4];
        private readonly Collider2D[] hitResults = new Collider2D[8];

        private Rigidbody2D rb;
        private Animator animator;
        private int jumpCount;
        private int facing = -1;                       // 素材默认朝左

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int GroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VSpeedHash = Animator.StringToHash("VSpeed");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            spawnPosition = transform.position;
        }

        private void Start()
        {
            hp = config.MaxHp;
            healthEvent.Raise(hp, config.MaxHp);
        }

        private void Update()
        {
            if (dead)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");

            // ===== 攻击输入与缓冲（必须放在早退 return 之前）=====
            if (Input.GetKeyDown(KeyCode.J))
            {
                attackEvent.Raise();
                if (IsAttacking())
                {
                    // 攻击中按 J：立即连招（attackN→attackN+1 链箭头随时消费，不打对）
                    animator.SetTrigger(AttackHash);
                }
                else
                {
                    // 空闲/受击/跳跃中：记下意图，等能接招时释放
                    attackBufferTimer = attackBufferWindow;
                }
            }

            // 缓冲释放：脱离攻击/受击状态的一瞬间把缓存的出刀意图放出去
            if (attackBufferTimer > 0f)
            {
                attackBufferTimer -= Time.deltaTime;
                if (!IsBusy())
                {
                    animator.SetTrigger(AttackHash);
                    attackBufferTimer = 0f;
                }
            }

            // ===== 移动 =====
            bool running = Input.GetKey(KeyCode.LeftShift);

            bool grounded = IsGrounded();
            if (grounded && rb.velocity.y <= 0.01f) jumpCount = 2;

            float speed = running ? config.RunSpeed : config.WalkSpeed;
            rb.velocity = new Vector2(h * speed, rb.velocity.y);

            animator.SetFloat(SpeedHash, Mathf.Abs(h) * (running ? 2f : 1f));
            animator.SetBool(GroundedHash, grounded);
            animator.SetFloat(VSpeedHash, rb.velocity.y);

            if (Input.GetKeyDown(KeyCode.K) && jumpCount > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, config.JumpForce);
                animator.SetTrigger(JumpHash);
                jumpCount--;
            }

            if (Mathf.Approximately(h, 0f)) return;

            bool flip = h > 0f;              // 素材默认朝左
            bodyRenderer.flipX = flip;
            weaponRenderer.flipX = flip;
            facing = flip ? 1 : -1;
        }

        /// <summary>当前就在攻击动画中（用于“立即连招”判定）。</summary>
        private bool IsAttacking()
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        }

        /// <summary>忙=攻击中或受击中，忙时暂存攻击输入。</summary>
        private bool IsBusy()
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("Attack") || info.IsTag("Hurt");
        }

        /// <summary>被怪物攻击。fromDirection：击退方向（+1 向右 / -1 向左）。</summary>
        public void TakeDamage(int damage, float fromDirection)
        {
            if (dead) return;

            hp -= damage;
            if (hp < 0) hp = 0;
            healthEvent.Raise(hp, config.MaxHp);

            animator.SetTrigger(HurtHash);
            shakeEvent?.Request(0.25f, 0.22f);
            hitStop?.Trigger(0.08f);

            Vector2 knockback = new Vector2(fromDirection * config.HurtKnockbackX, config.HurtKnockbackY);
            rb.velocity = knockback;

            if (hp <= 0) { dead = true; StartCoroutine(Respawn()); }
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);
            transform.position = spawnPosition;
            rb.velocity = Vector2.zero;
            hp = config.MaxHp;
            dead = false;
            healthEvent.Raise(hp, config.MaxHp);
        }

        /// <summary>Animation Event：攻击命中帧调用（挂在 Wukong_attack1~4 上）。</summary>
        public void OnAttackHit()
        {
            Vector2 center = (Vector2)transform.position + new Vector2(config.HitboxOffsetX * facing, 0f);
            int count = Physics2D.OverlapBoxNonAlloc(center, config.HitboxSize, 0f, hitResults, monsterLayer);

            for (int i = 0; i < count; i++)
            {
                Monster monster = hitResults[i].GetComponent<Monster>();
                if (monster == null) continue;

                bool isCrit = Random.value < config.CritChance;
                int damage = isCrit
                    ? Mathf.RoundToInt(config.AttackDamage * config.CritMultiplier)
                    : config.AttackDamage;

                monster.TakeHit(damage, facing);
                hitStop?.Trigger();
                shakeEvent?.Request(isCrit ? 0.2f : 0.12f, isCrit ? 0.18f : 0.1f);
                Debug.Log($"[战斗] {hitResults[i].name} 受到 {damage} 点伤害{(isCrit ? "（暴击！）" : "")}");
            }
        }

        private bool IsGrounded()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                groundCheck.position, groundCheckRadius, groundHits, groundLayer);
            return hitCount > 0;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }

            Gizmos.color = Color.red;
            Vector2 size = config != null ? config.HitboxSize : new Vector2(0.8f, 0.8f);
            float offsetX = config != null ? config.HitboxOffsetX : 0.8f;
            Vector2 center = (Vector2)transform.position + new Vector2(offsetX * facing, 0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}