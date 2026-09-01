using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("渲染器引用")]
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer weaponRenderer;

    [Header("待机动画")]
    public Sprite[] bodyIdleFrames;
    public Sprite[] weaponIdleFrames;

    [Header("行走动画")]
    public Sprite[] bodyRunFrames;
    public Sprite[] weaponRunFrames;

    [Header("动画速度")]
    public float frameInterval = 0.12f; // 每帧间隔秒数，越小越快

    private float timer;
    private int currentFrame;
    private bool isMoving = false;

    void Update()
    {
        // 检测是否在移动（按方向键就算移动）
        isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        // 选择当前播放哪组动画
        Sprite[] bodyFrames = isMoving ? bodyRunFrames : bodyIdleFrames;
        Sprite[] weaponFrames = isMoving ? weaponRunFrames : weaponIdleFrames;

        if (bodyFrames == null || bodyFrames.Length == 0) return;

        // 帧计时
        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % bodyFrames.Length;

            // 同时设置角色和武器，保证同步
            bodyRenderer.sprite = bodyFrames[currentFrame];
            if (weaponFrames != null && weaponFrames.Length > currentFrame)
            {
                weaponRenderer.sprite = weaponFrames[currentFrame];
            }
        }
    }
}
