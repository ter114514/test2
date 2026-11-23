using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 下蹲系統（完整 + 診斷 log）。蹲下、持續4秒正座（鏡頭下帶）、放開回放+鏡頭回彈。
/// </summary>
public class CrouchSystem : MonoBehaviour
{
    [Header("蹲下設定")]
    [SerializeField] float crouchProgress = 0.25f;
    [SerializeField] float seizaProgress = 1f;
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float seizaSpeed = 0.5f;

    [Header("正座")]
    [SerializeField] float seizaHoldTime = 4f;

    [Header("鏡頭下帶")]
    [SerializeField] float cameraDownAmount = 3f;

    [Header("引用")]
    [SerializeField] Animator crouchAnimator;
    [SerializeField] string crouchStateName = "Crouch";
    [SerializeField] string idleStateName = "Player_Idle";
    [SerializeField] PlayerAnimator playerAnimator;
    [SerializeField] CameraFollow cameraFollow;

    PlayerInputHandler input;
    float currentProgress;
    float moveInput;
    bool wasCrouching;
    float crouchHoldTimer;

    void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        if (crouchAnimator == null) crouchAnimator = GetComponentInChildren<Animator>();
        if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        if (cameraFollow == null) cameraFollow = CameraFollow.Instance;

        // 診斷：確認抓到的 cameraFollow
        Debug.Log($"【Crouch】cameraFollow = {(cameraFollow != null ? cameraFollow.name : "null")}");
    }

    void OnEnable()
    {
        if (input != null) input.OnMove += SetMoveInput;
    }

    void OnDisable()
    {
        if (input != null) input.OnMove -= SetMoveInput;
    }

    void SetMoveInput(float value) => moveInput = value;

    void Update()
    {
        bool holdingDown = IsHoldingDown();
        if (Mathf.Abs(moveInput) > 0.1f) holdingDown = false;

        if (holdingDown)
        {
            if (currentProgress < crouchProgress - 0.001f)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, crouchProgress, crouchSpeed * Time.deltaTime);
                crouchHoldTimer = 0f;
            }
            else
            {
                crouchHoldTimer += Time.deltaTime;
                if (crouchHoldTimer >= seizaHoldTime)
                    currentProgress = Mathf.MoveTowards(currentProgress, seizaProgress, seizaSpeed * Time.deltaTime);
                else
                    currentProgress = crouchProgress;
            }

            ApplyCrouch();
            wasCrouching = true;
        }
        else
        {
            crouchHoldTimer = 0f;
            currentProgress = Mathf.MoveTowards(currentProgress, 0f, crouchSpeed * Time.deltaTime);

            if (currentProgress > 0.001f)
            {
                ApplyCrouch();
                wasCrouching = true;
            }
            else if (wasCrouching)
            {
                if (crouchAnimator != null)
                {
                    crouchAnimator.speed = 1f;
                    crouchAnimator.Play(idleStateName, 0, 0f);
                }
                if (playerAnimator != null) playerAnimator.enabled = true;
                wasCrouching = false;
            }
        }

        UpdateCamera(holdingDown);
    }

    void UpdateCamera(bool holdingDown)
    {
        if (cameraFollow == null) return;

        bool inSeiza = holdingDown && currentProgress >= seizaProgress - 0.05f;
        float off = inSeiza ? -cameraDownAmount : 0f;

        // 診斷：只在偏移該是 0（放開/非正座）時印，看有沒有設回 0
        if (!inSeiza)
            Debug.Log($"【Crouch】設鏡頭偏移 0（holdingDown={holdingDown}, progress={currentProgress:F2}）");

        cameraFollow.SetExtraYOffset(off);
    }

    void ApplyCrouch()
    {
        if (playerAnimator != null) playerAnimator.enabled = false;
        if (crouchAnimator != null)
        {
            crouchAnimator.Play(crouchStateName, 0, Mathf.Clamp01(currentProgress));
            crouchAnimator.speed = 0;
        }
    }

    bool IsHoldingDown()
    {
        var kb = Keyboard.current;
        return kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed);
    }
}