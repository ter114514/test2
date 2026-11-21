using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 下蹲系統（完整）。蹲下、持續4秒正座（鏡頭下帶）、放開回放+鏡頭回彈。
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
    [Tooltip("正座時鏡頭往下的量")]
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
            // 蹲下先到幀2
            if (currentProgress < crouchProgress - 0.001f)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, crouchProgress, crouchSpeed * Time.deltaTime);
                crouchHoldTimer = 0f;
            }
            else
            {
                // 蹲到幀2，計時進正座
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
            // 放開：回放
            crouchHoldTimer = 0f;
            currentProgress = Mathf.MoveTowards(currentProgress, 0f, crouchSpeed * Time.deltaTime);

            if (currentProgress > 0.001f)
            {
                ApplyCrouch();
                wasCrouching = true;
            }
            else if (wasCrouching)
            {
                // 回站立
                if (crouchAnimator != null)
                {
                    crouchAnimator.speed = 1f;
                    crouchAnimator.Play(idleStateName, 0, 0f);
                }
                if (playerAnimator != null) playerAnimator.enabled = true;
                wasCrouching = false;
            }
        }

        // 鏡頭下帶：只有「按住下」且「正座進度」才下帶，放開立刻回彈
        UpdateCamera(holdingDown);
    }

    void UpdateCamera(bool holdingDown)
    {
        if (cameraFollow == null) return;

        // 關鍵：加 holdingDown 條件，放開（false）立刻回 0
        bool inSeiza = holdingDown && currentProgress >= seizaProgress - 0.05f;
        cameraFollow.SetExtraYOffset(inSeiza ? -cameraDownAmount : 0f);
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