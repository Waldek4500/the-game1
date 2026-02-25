using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Private Fields
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

    // Is player on ground
    private bool _isGrounded;

    [Header("Others")]
    // Ground layer for grounded detection
    [SerializeField] private LayerMask _groundLayer;

    // Layer of items with player can interact
    [SerializeField] private LayerMask _interactionLayer;

    // RigidBody2D component
    private Rigidbody2D _rb;

    // BoxCollider2D component
    [SerializeField] private BoxCollider2D _boxCollider;

    // Animator component
    private Animator _animator;

    // Vector2 from input system
    private Vector2 _movement;
    #endregion


    void Start()
    {
        // Get start facing
        _isFacingRight = transform.localScale.x > 0;

        // Get components
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_movement.x > 0 && !_isFacingRight)
            Flip();
        else if (_movement.x < 0 && _isFacingRight)
            Flip();


        _animator.SetFloat("MoveX", Mathf.Abs(_rb.linearVelocity.x));

        if(_animator.GetBool("Grounded") != IsGrounded())
            _animator.SetBool("Grounded", IsGrounded());

        Debug.Log(Mouse.current.position.ReadValue());
    }

    private void FixedUpdate()
    {
        if(_isSprinting)
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
        if (context.performed && IsGrounded())
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, _jumpForce);
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

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Check for item with player can interact
            Collider2D area = Physics2D.OverlapBox(_boxCollider.bounds.center, _boxCollider.bounds.size*5, 0, _interactionLayer);

            // Retrun if not found
            if (!area)
                return;

            // Get GameObject to interact
            GameObject item = area.gameObject;

            // Execute action
            if(item.TryGetComponent<IInteractable>(out IInteractable interaction))
            {
                interaction.Interact();
            }
        }
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
        return Physics2D.BoxCast(_boxCollider.bounds.center, _boxCollider.bounds.size, 0, -Vector2.up, 0.3f, _groundLayer);
    }

    #endregion

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireCube(_boxCollider.bounds.center, _boxCollider.bounds.size * 5);
    //}
}
