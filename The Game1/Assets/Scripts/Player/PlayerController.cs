using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Private Fields
    [Header("Components")]
    // BoxCollider2D component
    [SerializeField] private BoxCollider2D _boxCollider;

    // RigidBody2D component
    private Rigidbody2D _rb;

    // Animator component
    private Animator _animator;

    // Vector2 from input system
    private Vector2 _movement;

    [Header("Player Movement")]
    // Player speed
    [SerializeField] private float _movementSpeed;

    // Player jump force
    [SerializeField] private float _jumpForce;

    // Player sprint speed
    [SerializeField] private float _sprintSpeed;

    // Is player sprinting
    private bool _isSprinting;

    // Is player facing right
    private bool _isFacingRight;

    [Header("Jumping")]
    // Max count of jumps player can do
    [SerializeField] private int _maxJumps;

    // Remaining count of jumps
    private int _remainingJumps;

    // Is player on ground
    private bool _isGrounded;

    // Point for checking IsGrounded
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Vector2 _groundCheckSize;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Wall Slide")]
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private Vector2 _wallCheckSize;
    [SerializeField] private LayerMask _wallLayer;


    [Header("Gravity")]
    [SerializeField] private float _baseGravity;
    [SerializeField] private float _maxFallSpeed;
    [SerializeField] private float _fallSpeedMultiplier;

    [Header("Interaction")]
    // Layer of items with player can interact
    [SerializeField] private LayerMask _interactionLayer;

    #endregion


    void Start()
    {
        // Get start facing
        _isFacingRight = transform.localScale.x > 0;

        // Get components
        _rb = GetComponent<Rigidbody2D>();
        //_animator = GetComponent<Animator>();

        _remainingJumps = _maxJumps;
    }

    private void Update()
    {
        // Fliping player
        if (_movement.x > 0 && !_isFacingRight || _movement.x < 0 && _isFacingRight)
            Flip();

        ProcessGravity();

        //_animator.SetFloat("MoveX", Mathf.Abs(_rb.linearVelocity.x));

        //if(_animator.GetBool("Grounded") != IsGrounded())
        //    _animator.SetBool("Grounded", IsGrounded());

        //Debug.Log(Mouse.current.position.ReadValue());
    }

    private void FixedUpdate()
    {
        if (_isSprinting)
            _rb.linearVelocity = new Vector2(_movement.x * _movementSpeed * _sprintSpeed, _rb.linearVelocityY);
        else
            _rb.linearVelocity = new Vector2(_movement.x * _movementSpeed, _rb.linearVelocityY);
    }

    #region Player Movement
    public void Move(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {

        if (!IsGrounded() && _remainingJumps == _maxJumps)
        {
            Debug.Log($"Jumps {_remainingJumps}");
            return;
        }
            

        if (context.performed && _remainingJumps > 0)
        {
            _remainingJumps--;
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, _jumpForce);
        }
    }

    private void ProcessGravity()
    {
        if(_rb.linearVelocityY < 0)
        {
            _rb.gravityScale = _baseGravity * _fallSpeedMultiplier;
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, Mathf.Max(_rb.linearVelocityY, -_maxFallSpeed));
        }
        else
        {
            _rb.gravityScale = _baseGravity;
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            _isSprinting = true;
            return;
        }

        _isSprinting = false;
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if(context.performed && IsGrounded())
        {
            _animator.SetTrigger("Attack");
        }
    }

    public void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(_groundCheck.position, _groundCheckSize, 0, -Vector2.up, 0, _groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Resets jumps when player is on ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && IsGrounded())
            _remainingJumps = _maxJumps;
    }

    #endregion

    #region Player Interaction
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Check for item with player can interact
            Collider2D area = Physics2D.OverlapBox(_boxCollider.bounds.center, _boxCollider.bounds.size * 5, 0, _interactionLayer);

            // Retrun if not found
            if (!area)
                return;

            // Get GameObject to interact
            GameObject item = area.gameObject;

            // Execute action
            if (item.TryGetComponent<IInteractable>(out IInteractable interaction))
            {
                interaction.Interact();
            }
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_boxCollider.bounds.center, _boxCollider.bounds.size * 5);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_groundCheck.position, _groundCheckSize);
    }
}
