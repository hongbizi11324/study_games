using UnityEngine;

namespace ZaoMeng.Gameplay
{
    public class Player : MonoBehaviour
    {
        [Header("表现层引用")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("移动参数（M0 临时方案，M2 迁移到 ScriptableObject）")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;

        private Animator animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            bool running = Input.GetKey(KeyCode.LeftShift);

            animator.SetFloat(SpeedHash, Mathf.Abs(h) * (running ? 2f : 1f));

            if (Input.GetKeyDown(KeyCode.J)) animator.SetTrigger(AttackHash);
            if (Input.GetKeyDown(KeyCode.K)) animator.SetTrigger(JumpHash);

            if (Mathf.Approximately(h, 0f)) return;

            bool flip = h > 0f;              // 素材默认朝左
            bodyRenderer.flipX = flip;
            weaponRenderer.flipX = flip;

            float speed = running ? runSpeed : walkSpeed;
            transform.position += new Vector3(h * speed * Time.deltaTime, 0f, 0f);
        }
    }
}