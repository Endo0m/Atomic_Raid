using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private MovementSettings _settings;
    [SerializeField] private GameObject _fireJetpackParticle1;
    [SerializeField] private GameObject _fireJetpackParticle2;

    [Header("Зависимости")]
    private Rigidbody _rigidbody;
    private PlayerAnimator _playerAnimator;
    private EnvironmentStaticGenerator _environmentGenerator;

    [Header("Состояние движения")]
    private Vector2 _direction;
    private float _verticalVelocity;
    private bool _isRevertMove = false;
    private bool _isFrozen = false;
    private bool _isRunning = false;
    private bool _isGrounded = false;
    private bool _isSliding = false;

    [Header("Прыжки")]
    private bool _canDoubleJump = false;
    private bool _isDoubleJumping = false;
    private float _doubleJumpStartTime;

    [Header("Управление")]
    private KeyCode _jumpKey;
    private KeyCode _slideKey;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();

        if (_playerAnimator == null)
            Debug.LogError("PlayerAnimator компонент не найден!");
        if (_environmentGenerator == null)
            Debug.LogError("EnvironmentStaticGenerator не найден в сцене!");
    }

    private void Update()
    {
        if (_isFrozen)
            return;

        HandleInput();

        // Добавьте эту проверку
        if (_isRunning != _rigidbody.useGravity)
        {
            Debug.LogWarning($"PlayerMove: Inconsistent state detected. _isRunning: {_isRunning}, useGravity: {_rigidbody.useGravity}");
            _rigidbody.useGravity = _isRunning;
        }
    }

    private void HandleInput()
    {
        _direction = InputHandler.Instance.GetMovementInput();

        if (_isRunning)
        {
            HandleRunningInput();
        }
    }

    private void HandleRunningInput()
    {
        if (Input.GetKeyDown(_jumpKey))
            Jump();
        else if (Input.GetKey(_jumpKey) && _isDoubleJumping)
            ContinueDoubleJump();
        else if (Input.GetKeyUp(_jumpKey))
            EndDoubleJump();

        if (Input.GetKeyDown(_slideKey) && _isGrounded && !_isSliding)
            StartCoroutine(Slide());
    }

    private void FixedUpdate()
    {
        if (_isFrozen)
        {
            _rigidbody.velocity = Vector3.zero;
            return;
        }

        if (_isRunning)
            MovePlayerRunning();
        else
            MovePlayerFlying();

        ConstrainPosition();
        CheckGrounded();
    }

    private void MovePlayerFlying()
    {
        Vector2 finalDirection = _isRevertMove ? -_direction : _direction;
        Vector3 movement = new Vector3(finalDirection.x, finalDirection.y, 0) * _settings.flyingMoveSpeed * Time.fixedDeltaTime;
        _rigidbody.velocity = movement;
    }

    private void MovePlayerRunning()
    {
        Vector2 finalDirection = _isRevertMove ? -_direction : _direction;

        if (!_isGrounded && !_isDoubleJumping)
            _verticalVelocity += _settings.gravity * Time.fixedDeltaTime;
        else if (_isGrounded)
            _verticalVelocity = 0f;

        Vector3 movement = new Vector3(finalDirection.x * _settings.runningMoveSpeed, _verticalVelocity, 0);
        _rigidbody.velocity = new Vector3(movement.x, _rigidbody.velocity.y, 0);
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            PerformFirstJump();
        }
        else if (_canDoubleJump)
        {
            StartDoubleJump();
        }
    }

    private void PerformFirstJump()
    {
        _verticalVelocity = _settings.jumpForce;
        _canDoubleJump = true;
        EnableJetpackParticles(false);
        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _verticalVelocity, 0);
        _playerAnimator?.TriggerJump();
        Debug.Log("Выполнен первый прыжок");
    }

    private void StartDoubleJump()
    {
        _isDoubleJumping = true;
        _canDoubleJump = false;
        _doubleJumpStartTime = Time.time;
        _verticalVelocity = _settings.doubleJumpForce;
        EnableJetpackParticles(true);
        _playerAnimator?.TriggerDoubleJump();
        Debug.Log("Начат двойной прыжок");
    }

    private void ContinueDoubleJump()
    {
        float elapsedTime = Time.time - _doubleJumpStartTime;
        if (elapsedTime < _settings.doubleJumpDuration)
        {
            _verticalVelocity = _settings.doubleJumpUpwardSpeed;
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _verticalVelocity, 0);
        }
        else
        {
            EndDoubleJump();
        }
    }

    private void EndDoubleJump()
    {
        if (_isDoubleJumping)
        {
            _isDoubleJumping = false;
            EnableJetpackParticles(false);
            _verticalVelocity = 0;
            Debug.Log("Завершен двойной прыжок");
        }
    }

    private IEnumerator Slide()
    {
        _isSliding = true;
        _playerAnimator?.TriggerSlide();

        float originalSpeed = _environmentGenerator.moveSpeed;
        _environmentGenerator.moveSpeed *= _settings.slideWorldSpeedMultiplier;

        yield return new WaitForSeconds(_settings.slideDuration);

        _isSliding = false;
        _environmentGenerator.SetEnvironmentSpeed(_isRunning);
    }

    private void CheckGrounded()
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);

        if (_isGrounded && !wasGrounded)
        {
            OnLanding();
        }
        else if (!_isGrounded && wasGrounded)
        {
            OnLeavingGround();
        }
    }

    private void OnLanding()
    {
        EnableJetpackParticles(false);
        _isDoubleJumping = false;
        Debug.Log("На земле");
    }

    private void OnLeavingGround()
    {
        Debug.Log("Покинул землю");
    }

    private void ConstrainPosition()
    {
        Vector3 position = _rigidbody.position;
        Vector2 minBounds = _isRunning ? _settings.runningMinBounds : _settings.flyingMinBounds;
        Vector2 maxBounds = _isRunning ? _settings.runningMaxBounds : _settings.flyingMaxBounds;

        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);

        _rigidbody.position = position;
    }

    private void EnableJetpackParticles(bool enable)
    {
        if (_fireJetpackParticle1 != null)
            _fireJetpackParticle1.SetActive(enable);
        if (_fireJetpackParticle2 != null)
            _fireJetpackParticle2.SetActive(enable);
        Debug.Log($"Частицы джетпака {(enable ? "включены" : "выключены")}");
    }


    public void SetRunningMode(bool isRunning)
    {
        Debug.Log($"PlayerMove: SetRunningMode called. isRunning: {isRunning}");
        _isRunning = isRunning;
        _rigidbody.useGravity = isRunning;

        if (isRunning)
        {
            SetupRunningMode();
        }
        else
        {
            SetupFlyingMode();
        }

        if (_environmentGenerator != null)
        {
            _environmentGenerator.SetEnvironmentSpeed(isRunning);
            Debug.Log($"PlayerMove: EnvironmentGenerator speed set. isRunning: {isRunning}");
        }
        else
        {
            Debug.LogError("PlayerMove: EnvironmentGenerator is null!");
        }

        if (_playerAnimator != null)
        {
            _playerAnimator.SetRunningMode(isRunning);
            Debug.Log($"PlayerMove: PlayerAnimator set to running mode. isRunning: {isRunning}");
            if (!isRunning)
                _playerAnimator.TriggerFlyingIdle();
        }
        else
        {
            Debug.LogError("PlayerMove: PlayerAnimator is null!");
        }

        Debug.Log($"PlayerMove: Running mode set to: {isRunning}");
    }


    private void SetupRunningMode()
    {
        _jumpKey = InputHandler.Instance.GetKeyBinding("MoveUp");
        _slideKey = InputHandler.Instance.GetKeyBinding("MoveDown");
        EnableJetpackParticles(false);
    }

    private void SetupFlyingMode()
    {
        _jumpKey = KeyCode.Space;
        _isSliding = false;
        _canDoubleJump = false;
        _verticalVelocity = 0f;
        _rigidbody.velocity = Vector3.zero;
        EnableJetpackParticles(true);
    }

    public void SetRevertMove(bool isRevert)
    {
        _isRevertMove = isRevert;
    }

    public bool IsGrounded()
    {
        return _isGrounded;
    }

    public void SetFrozen(bool isFrozen)
    {
        _isFrozen = isFrozen;
        if (_isFrozen)
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }
}