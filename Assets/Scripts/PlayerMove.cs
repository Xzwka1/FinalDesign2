using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))] // บังคับให้ต้องมี CharacterController
public class SimplePlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    public float airMultiplier = 0.4f;
    public float groundAcceleration = 10f; // ความเร็วในการ "เร่ง"
    // (เราจะไม่ใช้ groundDeceleration แล้ว เพราะจะใช้ Hard Stop)

    [Header("Dashing")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Sliding / Crouching")]
    public float slideSpeed = 15f;
    public float slideFriction = 10f;
    public float crouchSpeed = 3f;
    public float crouchHeight = 0.8f;
    private float standingHeight;
    private bool isSliding = false;
    private float currentSlideSpeed;
    private Vector3 slideDirection;
    private bool isCrouching = false;

    [Header("Slope Handling")]
    public float slopeForce = 6f;
    public float slopeRayLength = 1.5f;
    private Vector3 slopeMoveDirection;

    [Header("Wall Mechanics")]
    public LayerMask whatIsWall;
    public float wallCheckDistance = 0.7f;
    public float wallJumpUpForce = 7f;
    public float wallJumpSideForce = 5f;
    public float wallRunSpeed = 8f;
    public float wallRunCameraTilt = 10f;
    public float cameraTiltSpeed = 6f;
    public float maxWallRunTime = 2f;
    private float wallRunTimer;
    private bool isWallRunning = false;
    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    [Header("Camera & Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float cameraHeightChangeSpeed = 8f;
    private float xRotation = 0f;
    private Vector3 standingCameraPos;
    private Vector3 crouchCameraPos;

    [Header("Camera Effects (FOV)")]
    public float dashFOV = 90f;
    public float fovChangeSpeed = 10f;
    private Camera cameraComponent;
    private float normalFOV;

    // --- Private Variables ---
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // --- Input Variables ---
    private float xInput;
    private float zInput;
    private Vector3 moveInputDirection;
    private bool wantsToCrouch;
    private bool isSprinting;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) { Debug.LogError("CharacterController component not found!"); enabled = false; return; }

        standingHeight = controller.height;
        controller.center = new Vector3(0, standingHeight / 2, 0);

        if (playerCamera != null)
        {
            cameraComponent = playerCamera.GetComponent<Camera>();
            if (cameraComponent != null)
            {
                normalFOV = cameraComponent.fieldOfView;
            }
            else
            {
                Debug.LogError("Player Camera Transform does not have a Camera component!");
                enabled = false;
                return;
            }

            standingCameraPos = playerCamera.localPosition;
            float cameraOffsetY = standingCameraPos.y - (standingHeight / 2);
            float newCenterY = crouchHeight / 2;
            crouchCameraPos = new Vector3(standingCameraPos.x, newCenterY + cameraOffsetY, standingCameraPos.z);
        }
        else { Debug.LogError("Player Camera Transform is not assigned!"); enabled = false; return; }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        wallRunTimer = maxWallRunTime;
    }


    void Update()
    {
        if (PauseMenu.GameIsPaused) { return; }
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        MyInput();
        CheckForWall();
        HandleWallRunState();
        HandleMovement();
        HandleMouseLook();
        HandleHeightChange();
        HandleCameraEffects();
    }

    private void HandleCameraEffects()
    {
        if (cameraComponent == null) return;
        float targetFOV = isDashing ? dashFOV : normalFOV;
        cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    void MyInput()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        moveInputDirection = transform.right * xInput + transform.forward * zInput;
        moveInputDirection.Normalize();
        wantsToCrouch = Input.GetKey(KeyCode.LeftControl);
        bool canSprint = !isCrouching && zInput > 0.1f && isGrounded;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && canSprint;
        if (Input.GetKeyDown(KeyCode.Q) && dashCooldownTimer <= 0 && !isDashing && !isCrouching)
        {
            StartCoroutine(Dash());
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private void HandleWallRunState()
    {
        bool canWallRun = !isGrounded && (wallLeft || wallRight) && Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        if (canWallRun && !isWallRunning)
        {
            isWallRunning = true;
            velocity.y = 0f;
            wallRunTimer = maxWallRunTime;
            Debug.Log("Start Wall Run!");
        }
        else if (!canWallRun && isWallRunning)
        {
            isWallRunning = false;
        }
        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0) isWallRunning = false;
        }
    }

    void HandleMovement()
    {
        if (isDashing) return;

        isGrounded = controller.isGrounded;
        Vector3 rayOrigin = transform.position + Vector3.up * (controller.radius * 0.5f);

        if (wantsToCrouch && isSprinting && !isSliding && !isCrouching) StartSlide(moveInputDirection);
        else if (!wantsToCrouch && isSliding) StopSlide();
        if (wantsToCrouch && !isSliding) isCrouching = true;
        else if (!wantsToCrouch && !isSliding) isCrouching = false;


        if (isWallRunning)
        {
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
            if (Vector3.Dot(transform.forward, wallForward) < 0) wallForward = -wallForward;
            velocity.x = wallForward.x * wallRunSpeed;
            velocity.z = wallForward.z * wallRunSpeed;
        }
        else if (isSliding)
        {
            currentSlideSpeed -= slideFriction * Time.deltaTime;
            if (currentSlideSpeed <= crouchSpeed) { StopSlide(); }
            float speed = isSliding ? currentSlideSpeed : crouchSpeed;
            velocity.x = Mathf.Lerp(velocity.x, slideDirection.x * speed, Time.deltaTime * groundAcceleration);
            velocity.z = Mathf.Lerp(velocity.z, slideDirection.z * speed, Time.deltaTime * groundAcceleration);
        }
        else if (isGrounded) // Normal Ground Movement
        {
            // --- ⬇️ (แก้ไข - แก้บั๊กตัวไหลแบบ "หยุดทันที") ⬇️ ---

            // 1. คำนวณความเร็วเป้าหมาย
            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && zInput > 0.1f && !isCrouching;
            float currentSpeed = isCrouching ? crouchSpeed : (wantsToSprint ? sprintSpeed : moveSpeed);
            Vector3 targetVelocity = moveInputDirection * currentSpeed;

            // 2. เช็คว่ากดปุ่มหรือไม่
            if (moveInputDirection.magnitude < 0.1f) // ถ้าปล่อยปุ่ม WASD
            {
                // "หยุดทันที" (Hard Stop)
                velocity.x = 0f;
                velocity.z = 0f;
            }
            else // ถ้ากดปุ่ม
            {
                // "เร่ง" (Lerp) ตามปกติ
                velocity.x = Mathf.Lerp(velocity.x, targetVelocity.x, Time.deltaTime * groundAcceleration);
                velocity.z = Mathf.Lerp(velocity.z, targetVelocity.z, Time.deltaTime * groundAcceleration);
            }
            // --- ⬆️ (จบส่วนแก้ไข) ⬆️ ---
        }
        else // Normal Air Movement (Apex Style)
        {
            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && zInput > 0.1f && !isCrouching;
            float airSpeed = wantsToSprint ? sprintSpeed : moveSpeed;
            velocity += moveInputDirection * airSpeed * airMultiplier * Time.deltaTime;
        }

        // --- 3. คำนวณความเร็วแนวดิ่ง (Vertical Velocity) ---
        if (!isWallRunning && isGrounded)
        {
            RaycastHit slopeHit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out slopeHit, controller.height * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                if (angle < controller.slopeLimit && angle != 0)
                {
                    if (controller.velocity.y < 0.1f && Vector3.Dot(velocity, slopeHit.normal) < 0 && !isCrouching)
                    {
                        velocity.y = -slopeForce;
                    }
                }
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isWallRunning) { WallJump(); isWallRunning = false; wallRunTimer = 0f; }
            else if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (isSliding) StopSlide();
                wallRunTimer = maxWallRunTime;
            }
            else if (wallLeft || wallRight) { WallJump(); }
        }

        if (!isWallRunning)
        {
            if (!isGrounded || velocity.y > -slopeForce + 0.1f)
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        if (isGrounded && velocity.y < 0 && !Input.GetButtonDown("Jump") && !isSliding && !isWallRunning)
        {
            RaycastHit groundHit;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out groundHit, controller.height * 0.5f + 0.3f) || Vector3.Angle(Vector3.up, groundHit.normal) == 0)
                velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void WallJump()
    {
        Debug.Log("Wall Jump!");
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        velocity.x = forceToApply.x;
        velocity.y = forceToApply.y;
        velocity.z = forceToApply.z;
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        float targetTilt = 0f;
        if (isWallRunning)
        {
            targetTilt = wallLeft ? -wallRunCameraTilt : wallRunCameraTilt;
        }
        float currentTilt = Mathf.LerpAngle(playerCamera.localRotation.eulerAngles.z, targetTilt, Time.deltaTime * cameraTiltSpeed);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeightChange()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float currentHeight = controller.height;
        if (!isCrouching && currentHeight < standingHeight - 0.1f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * (currentHeight / 2 + 0.05f);
            if (Physics.SphereCast(rayOrigin, controller.radius, Vector3.up, out RaycastHit headHit, standingHeight - currentHeight + 0.1f, ~0, QueryTriggerInteraction.Ignore))
            {
                targetHeight = crouchHeight;
                isCrouching = true;
                if (isSliding) StopSlide();
            }
        }
        controller.height = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * cameraHeightChangeSpeed * 2);
        controller.center = new Vector3(0, controller.height / 2, 0);
        if (playerCamera != null)
        {
            Vector3 targetCameraLocalPos = isCrouching ? crouchCameraPos : standingCameraPos;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCameraLocalPos, Time.deltaTime * cameraHeightChangeSpeed);
        }
    }

    private void StartSlide(Vector3 direction)
    {
        if (!isGrounded) return;
        isSliding = true;
        isCrouching = true;
        currentSlideSpeed = slideSpeed;
        float currentHorizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        if (currentHorizontalSpeed > slideSpeed)
        {
            currentSlideSpeed = currentHorizontalSpeed;
        }
        slideDirection = (direction.magnitude > 0.1f) ? direction : transform.forward;
    }

    private void StopSlide()
    {
        isSliding = false;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        float startTime = Time.time;
        Vector3 originalVerticalVelocity = new Vector3(0, velocity.y, 0);
        float xDash = Input.GetAxisRaw("Horizontal");
        float zDash = Input.GetAxisRaw("Vertical");
        Vector3 dashInputDirection = transform.right * xDash + transform.forward * zDash;
        Vector3 dashDirection = dashInputDirection.magnitude > 0.1f ? dashInputDirection.normalized : transform.forward;
        Vector3 dashStartVelocity = new Vector3(velocity.x, 0, velocity.z);

        while (Time.time < startTime + dashDuration)
        {
            Vector3 dashVelocity = dashDirection * dashSpeed;
            Vector3 combinedVelocity = Vector3.Lerp(dashStartVelocity, dashVelocity, (Time.time - startTime) / dashDuration);
            controller.Move((combinedVelocity + originalVerticalVelocity * 0.2f) * Time.deltaTime);
            velocity = new Vector3(combinedVelocity.x, velocity.y, combinedVelocity.z);
            yield return null;
        }
        isDashing = false;
    }

    // --- ฟังก์ชัน Respawn (มีอันเดียว) ---
    public void Respawn(Vector3 spawnPoint, CharacterController charController)
    {
        Debug.Log("PlayerMove is respawning...");

        if (charController != null)
        {
            charController.enabled = false;
        }
        transform.position = spawnPoint;
        if (charController != null)
        {
            charController.enabled = true;
        }

        velocity = Vector3.zero;
        isDashing = false;
        isSliding = false;
        isCrouching = false;
        isWallRunning = false;
        wallRunTimer = maxWallRunTime;
        dashCooldownTimer = 0f;

        if (playerCamera != null)
        {
            xRotation = 0f;
            playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
            playerCamera.localPosition = standingCameraPos;
        }
        if (cameraComponent != null)
        {
            cameraComponent.fieldOfView = normalFOV;
        }
    }
}