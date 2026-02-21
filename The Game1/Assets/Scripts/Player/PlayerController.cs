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

    // RigidBody2D component
    private Rigidbody2D _rb;
    // Vector2 from input system
    private Vector2 _movement;
    #endregion


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(_movement.x * _movementSpeed, _rb.linearVelocityY);
    }


    #region Player Movement
    public void Move(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, _jumpForce);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(transform.position, )
    }

    #endregion
}
