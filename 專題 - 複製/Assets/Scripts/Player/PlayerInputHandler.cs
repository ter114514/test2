using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家輸入偵測器。使用 RebindManager 的共用資產，以事件對外發送輸入。
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public event Action<float> OnMove;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnAttackPressed;
    public event Action OnDashPressed;
    public event Action OnBlockPressed;
    public event Action OnBlockReleased;
    public event Action OnUsePotionPressed;
    public event Action OnCastSkillPressed;

    InputActionMap playerMap;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction dashAction;
    InputAction blockAction;
    InputAction usePotionAction;
    InputAction castSkillAction;

    bool ready;
    bool isSubscribed;

    void Start()
    {
        InitializeInput();
    }

    void InitializeInput()
    {
        if (RebindManager.Instance == null)
        {
            Debug.LogError("【PlayerInputHandler】找不到 RebindManager！請確保場景中有 RebindManager 物件。");
            enabled = false;
            return;
        }

        var asset = RebindManager.Instance.InputActions;
        if (asset == null)
        {
            Debug.LogError("【PlayerInputHandler】InputActions 沒設定。請在 RebindManager 上綁定 PlayerControls 資產。");
            enabled = false;
            return;
        }

        playerMap = asset.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("【PlayerInputHandler】找不到 'Player' Action Map。");
            enabled = false;
            return;
        }

        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        attackAction = playerMap.FindAction("Attack");
        dashAction = playerMap.FindAction("Dash");
        blockAction = playerMap.FindAction("Block");
        usePotionAction = playerMap.FindAction("UsePotion");
        castSkillAction = playerMap.FindAction("CastSkill");

        ready = true;

        if (isActiveAndEnabled)
        {
            EnableInput();
        }
    }

    void OnEnable()
    {
        if (ready)
        {
            EnableInput();
        }
    }

    void OnDisable()
    {
        if (ready)
        {
            DisableInput();
        }
    }

    void EnableInput()
    {
        if (isSubscribed || playerMap == null) return;

        playerMap.Enable();

        if (moveAction != null)
        {
            moveAction.performed += HandleMove;
            moveAction.canceled += HandleMove;
        }
        if (jumpAction != null)
        {
            jumpAction.performed += HandleJumpPressed;
            jumpAction.canceled += HandleJumpReleased;
        }
        if (attackAction != null) attackAction.performed += HandleAttack;
        if (dashAction != null) dashAction.performed += HandleDash;
        if (blockAction != null)
        {
            blockAction.performed += HandleBlockPressed;
            blockAction.canceled += HandleBlockReleased;
        }
        if (usePotionAction != null) usePotionAction.performed += HandleUsePotion;
        if (castSkillAction != null) castSkillAction.performed += HandleCastSkill;

        isSubscribed = true;
    }

    void DisableInput()
    {
        if (!isSubscribed || playerMap == null) return;

        if (moveAction != null)
        {
            moveAction.performed -= HandleMove;
            moveAction.canceled -= HandleMove;
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= HandleJumpPressed;
            jumpAction.canceled -= HandleJumpReleased;
        }
        if (attackAction != null) attackAction.performed -= HandleAttack;
        if (dashAction != null) dashAction.performed -= HandleDash;
        if (blockAction != null)
        {
            blockAction.performed -= HandleBlockPressed;
            blockAction.canceled -= HandleBlockReleased;
        }
        if (usePotionAction != null) usePotionAction.performed -= HandleUsePotion;
        if (castSkillAction != null) castSkillAction.performed -= HandleCastSkill;

        playerMap.Disable();
        isSubscribed = false;
    }

    void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<float>());
    void HandleJumpPressed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
    void HandleJumpReleased(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();
    void HandleAttack(InputAction.CallbackContext ctx) => OnAttackPressed?.Invoke();
    void HandleDash(InputAction.CallbackContext ctx) => OnDashPressed?.Invoke();
    void HandleBlockPressed(InputAction.CallbackContext ctx) => OnBlockPressed?.Invoke();
    void HandleBlockReleased(InputAction.CallbackContext ctx) => OnBlockReleased?.Invoke();
    void HandleUsePotion(InputAction.CallbackContext ctx) => OnUsePotionPressed?.Invoke();
    void HandleCastSkill(InputAction.CallbackContext ctx) => OnCastSkillPressed?.Invoke();
}