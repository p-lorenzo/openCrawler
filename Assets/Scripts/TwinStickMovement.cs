using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class TwinStickMovement : MonoBehaviour
{
    [Header("Movement variables")]
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float controllerDeadzone = 0.1f;
    [SerializeField] private float rotateControllerSmoothing = 1000f;
    [SerializeField] private float rotateKBMSmoothing = 10f;
    [SerializeField] private Animator animator;
    [SerializeField] private float controllerVelocity;
    
    private bool isGamepad;
    private bool isGrounded;

    [Header("Ground Checking")]
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundCheck;

    [Header("Dashing variables")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpwardForce;
    [SerializeField] private float dashCooldown;
    private float dashCdTimer = 0;
    private bool isDashing;
    private bool isRunning;
    private Slider dashingCdSlider;

    [Header("Shooting variables")]
    [SerializeField] private bool isShooting = false;

    private Vector3 impact = Vector3.zero;
    
    private CharacterController controller;

    private Vector2 movement;
    private Vector2 aim;

    private Vector3 playerVelocity;

    private PlayerInput playerInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        dashingCdSlider = transform.parent.GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isDashing", isDashing);
        if (Mathf.Abs(controller.velocity.x) >= 0.1f || Mathf.Abs(controller.velocity.z) >= 0.1f)
        {
            controllerVelocity = controller.velocity.magnitude;
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        dashingCdSlider.value = (dashCooldown - dashCdTimer) / 3f;
        if (dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
        }
        if (impact.magnitude > 0.2f)
        {
            controller.Move(impact * Time.deltaTime);
        } else
        {
            isDashing = false;
        }
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);
        CheckIsGrounded();
        HandleMovement();
        HandleRotation();
        if (isShooting)
        {
            animator.SetBool("isRunning", false);
        }
    }

    void HandleMovement()
    {
        if (!isShooting)
        {
            Vector3 move = new Vector3(movement.x, 0, movement.y);
            controller.Move(move * Time.deltaTime * playerSpeed);
        }

        if (!isGrounded) {
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        } else {
            playerVelocity.y = 0f;
        }
    }

    void HandleRotation()
    {
        if (isGamepad) {
            
            if (Mathf.Abs(aim.x) > controllerDeadzone || Mathf.Abs(aim.y) > controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * aim.x + Vector3.forward * aim.y;
                if (playerDirection.sqrMagnitude > 0.1f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    Debug.Log(newRotation + "|" + transform.rotation);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, rotateControllerSmoothing * Time.deltaTime);
                }
            }
        } else {
            if (isShooting)
            {
                Camera camera = transform.parent.GetComponentInChildren<Camera>();
                Ray ray = camera.ScreenPointToRay(aim);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float rayDistance;

                if (groundPlane.Raycast(ray, out rayDistance))
                {
                    Vector3 point = ray.GetPoint(rayDistance);
                    LookAt(point);
                }
            } else if (Mathf.Abs(movement.x) > controllerDeadzone || Mathf.Abs(movement.y) > controllerDeadzone)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(movement.x, 0, movement.y)), Time.deltaTime * rotateKBMSmoothing);
            }

        }
    }

    private void LookAt(Vector3 lookPoint){
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void OnDeviceChange(PlayerInput pi) 
    {
        isGamepad = pi.currentControlScheme.Equals("KBM") ? false : true;       
    }

    private void CheckIsGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.3f, ground);
    }

    public void OnMove(CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    public void OnAim(CallbackContext context)
    {
        aim = context.ReadValue<Vector2>();
    }

    public void OnDash(CallbackContext context)
    {
        if (isGrounded && dashCdTimer <= 0f)
        {
            StartCoroutine(DashCoroutine());
        }
    }
    
    public void OnShoot(CallbackContext context)
    {
        if (context.performed) isShooting = true;
        else if (context.canceled) isShooting = false;
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        dashCdTimer = dashCooldown;
        AddImpact(transform.forward, dashForce);
        AddImpact(transform.up, dashUpwardForce);
        yield return null;
    }

    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0)
        {
            dir.y -= dir.y;
        }
        impact += dir.normalized * force;
    }    

}
