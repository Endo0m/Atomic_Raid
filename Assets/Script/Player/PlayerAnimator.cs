

using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationClips
    {
        public AnimationClip idle;
        public AnimationClip moveLeft;
        public AnimationClip moveRight;
        public AnimationClip slide;
    }

    [SerializeField] private AnimationClips flyingAnimations;
    [SerializeField] private AnimationClips runningAnimations;
    [SerializeField] private AnimationClip jumpAnimation;
    [SerializeField] private AnimationClip doubleJumpAnimation;

    private Animator _animator;
    private PlayerMove _playerMove;
    private Rigidbody _rigidbody;

    private readonly int _moveXHash = Animator.StringToHash("MoveX");
    private readonly int _isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int _isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int _isSlidingHash = Animator.StringToHash("IsSliding");
    private readonly int _isRunningHash = Animator.StringToHash("IsRunning");

    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isDoubleJumping = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Ищем Animator в дочерних объектах
        _animator = GetComponentInChildren<Animator>();
        _playerMove = GetComponent<PlayerMove>();
        _rigidbody = GetComponent<Rigidbody>();

        if (_animator == null)
            Debug.LogError("Animator component not found in children!");
        if (_playerMove == null)
            Debug.LogError("PlayerMove component not found!");
        if (_rigidbody == null)
            Debug.LogError("Rigidbody component not found!");
    }

    private void Update()
    {
        if (_animator != null && _rigidbody != null)
        {
            UpdateAnimationParameters();
        }
    }

    private void UpdateAnimationParameters()
    {
        float moveX = _rigidbody.velocity.x;
        bool isMoving = Mathf.Abs(moveX) > 0.1f;

        _animator.SetFloat(_moveXHash, moveX);
        _animator.SetBool(_isMovingHash, isMoving);
        _animator.SetBool(_isRunningHash, _isRunning);

        if (_isRunning)
        {
            bool isGrounded = _playerMove.IsGrounded();
            _animator.SetBool(_isJumpingHash, _isJumping && !isGrounded);
        }

        UpdateAnimationState(moveX, isMoving);
    }

    private void UpdateAnimationState(float moveX, bool isMoving)
    {
        if (_isRunning)
        {
            if (_isJumping && !_playerMove.IsGrounded())
            {
                PlayAnimation(_isDoubleJumping ? doubleJumpAnimation : jumpAnimation);
            }
            else if (_animator.GetBool(_isSlidingHash))
            {
                PlayAnimation(runningAnimations.slide);
            }
            else if (isMoving)
            {
                PlayAnimation(moveX > 0 ? runningAnimations.moveRight : runningAnimations.moveLeft);
            }
            else
            {
                PlayAnimation(runningAnimations.idle);
            }
        }
        else // Режим полета
        {
            isMoving = false;   // Анимация flyingAnimations.moveRight : flyingAnimations.moveLeft отсутствует в аниматоре
            if (isMoving)
            {
                PlayAnimation(moveX > 0 ? flyingAnimations.moveRight : flyingAnimations.moveLeft);
            }
            else
            {
                PlayAnimation(flyingAnimations.idle);
            }
        }
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (clip != null && !IsPlayingAnimation(clip.name))
        {
            _animator.Play(clip.name);
        }
    }

    private bool IsPlayingAnimation(string clipName)
    {
        return _animator.GetCurrentAnimatorStateInfo(0).IsName(clipName);
    }

    public void SetRunningMode(bool isRunning)
    {
        _isRunning = isRunning;
        _animator.SetBool(_isRunningHash, isRunning);
    }

    public void TriggerJump()
    {
        _isJumping = true;
        _isDoubleJumping = false;
        _animator.SetTrigger(_isJumpingHash);
    }

    public void TriggerDoubleJump()
    {
        _isDoubleJumping = true;
        _isJumping = true;
        _animator.SetTrigger(_isJumpingHash);
    }

    public void TriggerLanding()
    {
        _isJumping = false;
        _isDoubleJumping = false;
        _animator.SetBool(_isJumpingHash, false);
    }

    public void TriggerSlide()
    {
        if (_isRunning)
        {
            _animator.SetBool(_isSlidingHash, true);
            StartCoroutine(ResetSlideAnimation());
        }
    }

    private IEnumerator ResetSlideAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Используйте значение из настроек, если оно доступно
        _animator.SetBool(_isSlidingHash, false);
    }

    public void TriggerFlyingIdle()
    {
        if (!_isRunning)
        {
            PlayAnimation(flyingAnimations.idle);
        }
    }
}