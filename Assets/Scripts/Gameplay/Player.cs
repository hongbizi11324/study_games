using System.Collections;
using UnityEngine;
using ZaoMeng.Data;
using ZaoMeng.Events;

namespace ZaoMeng.Gameplay
{
    public class Player : MonoBehaviour
    {
        [Header("数据层配置")]
        [SerializeField] private PlayerConfig config;

        [Header("表现层引用")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("事件通道（发布）")]
        [SerializeField] private GameEvent attackEvent;
        [SerializeField] private PlayerHealthEvent healthEvent;

        [Header("重生")]
        [SerializeField] private float respawnDelay = 2f;
        private int hp;
        private bool dead;
        private Vector2 spawnPosition;

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
            spawnPosition = transform.position;//出生点
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
            bool running = Input.GetKey(KeyCode.LeftShift);

            bool grounded = IsGrounded();
            if (grounded && rb.velocity.y <= 0.01f) jumpCount = 2;

            float speed = running ? config.RunSpeed : config.WalkSpeed;
            rb.velocity = new Vector2(h * speed, rb.velocity.y);

            animator.SetFloat(SpeedHash, Mathf.Abs(h) * (running ? 2f : 1f));
            animator.SetBool(GroundedHash, grounded);
            animator.SetFloat(VSpeedHash, rb.velocity.y);

            if (Input.GetKeyDown(KeyCode.J))
            {
                animator.SetTrigger(AttackHash);
                attackEvent.Raise();
            }

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

        // —— ⑤ 新增两个方法 ——
        /// <summary>被怪物攻击。M3 加受击硬直与击退。</summary>
        public void TakeDamage(int damage)
        {
            if (dead) return;

            hp -= damage;
            if (hp < 0) hp = 0;
            healthEvent.Raise(hp, config.MaxHp);
            animator.SetTrigger(HurtHash);
           

            if (hp <= 0)
            {
                dead = true;
                StartCoroutine(Respawn());
            }
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);   // 挂起 2 秒，不阻塞主线程
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