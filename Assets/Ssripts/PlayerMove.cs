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
    private bool isCrouching = false; // รวมสถานะตอนสไลด์ด้วย

    [Header("Slope Handling")]
    public float slopeForce = 6f;
    public float slopeRayLength = 1.5f;
    private Vector3 slopeMoveDirection;

    [Header("Wall Mechanics")]
    public LayerMask whatIsWall;
    public float wallCheckDistance = 0.7f;
    // Wall Jump
    public float wallJumpUpForce = 7f;
    public float wallJumpSideForce = 5f;
    // Wall Running
    public float wallRunSpeed = 8f;
    public float wallRunCameraTilt = 10f;
    public float cameraTiltSpeed = 6f;
    public float maxWallRunTime = 2f;
    private float wallRunTimer;
    private bool isWallRunning = false;
    // Wall Check
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
    public float dashFOV = 90f; // FOV ที่จะเปลี่ยนไปตอน Dash
    public float fovChangeSpeed = 10f; // ความเร็วในการเปลี่ยน FOV
    private Camera cameraComponent; // ตัว Component "Camera"
    private float normalFOV; // FOV ปกติ (จะถูกเก็บค่าอัตโนมัติ)


    // --- Private Variables ---
    private CharacterController controller;
    private Vector3 velocity; // เก็บความเร็วแนวดิ่ง (ตก/กระโดด/แรงกด)
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

        // --- ❗️ (เพิ่ม) ตั้งค่า Center Y เริ่มต้น ป้องกันจมดิน ---
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

            // --- ❗️ (แก้ไข) คำนวณตำแหน่งกล้องตอนย่อใหม่ (แก้บั๊กกล้องจม) ---
            float cameraOffsetY = standingCameraPos.y - (standingHeight / 2); // ระยะห่างกล้องจาก Center
            float newCenterY = crouchHeight / 2; // Center ใหม่ตอนย่อ
            crouchCameraPos = new Vector3(standingCameraPos.x, newCenterY + cameraOffsetY, standingCameraPos.z);
        }
        else { Debug.LogError("Player Camera Transform is not assigned!"); enabled = false; return; }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        wallRunTimer = maxWallRunTime;
    }


    void Update()
    {
        // --- ❗️ (แก้ไข) เช็ค Pause ทีเดียวบนสุด ---
        if (PauseMenu.GameIsPaused)
        {
            return; // ถ้าเกมหยุด ให้หยุดทำงาน Update นี้ทันที
        }

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
        else if (isGrounded)
        {
            wallRunTimer = maxWallRunTime;
        }
    }

    void HandleMovement()
    {
        if (isDashing) return;
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;
        if (wantsToCrouch && isSprinting && !isSliding && !isCrouching) StartSlide(moveInputDirection);
        else if (!wantsToCrouch && isSliding) StopSlide();
        if (wantsToCrouch && !isSliding) isCrouching = true;
        else if (!wantsToCrouch && !isSliding) isCrouching = false;

        Vector3 targetHorizontalVelocity;
        if (isWallRunning)
        {
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
            if (Vector3.Dot(transform.forward, wallForward) < 0) wallForward = -wallForward;
            targetHorizontalVelocity = wallForward * wallRunSpeed;
            velocity.y = -1f;
        }
        else if (isSliding)
        {
            currentSlideSpeed -= slideFriction * Time.deltaTime;
            if (currentSlideSpeed <= crouchSpeed) { StopSlide(); targetHorizontalVelocity = slideDirection * crouchSpeed; }
            else { targetHorizontalVelocity = slideDirection * currentSlideSpeed; }
        }
        else
        {
            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && zInput > 0.1f && !isCrouching;
            float currentSpeed;
            if (isCrouching) currentSpeed = crouchSpeed;
            else if (wantsToSprint) currentSpeed = sprintSpeed;
            else currentSpeed = moveSpeed;
            targetHorizontalVelocity = moveInputDirection * currentSpeed;
        }

        if (!isWallRunning)
        {
            RaycastHit slopeHit;
            Vector3 rayOrigin = transform.position + Vector3.up * (controller.radius * 0.5f);
            if (Physics.Raycast(rayOrigin, Vector3.down, out slopeHit, controller.height * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                if (angle < controller.slopeLimit && angle != 0)
                {
                    targetHorizontalVelocity = Vector3.ProjectOnPlane(targetHorizontalVelocity, slopeHit.normal);
                    if (controller.velocity.y < 0.1f && Vector3.Dot(velocity, slopeHit.normal) < 0)
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
                if (isSliding) { velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); StopSlide(); }
                else if (!isCrouching) { velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); }
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
        Vector3 finalVelocity = targetHorizontalVelocity;
        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void WallJump()
    {
        Debug.Log("Wall Jump!");
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        velocity = forceToApply;
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
        slideDirection = (direction.magnitude > 0.1f) ? direction : transform.forward;
    }

    private void StopSlide()
    {
        isSliding = false;
    }

    // --- ⬇️ นี่คือฟังก์ชัน Dash() ที่ถูกต้อง (มี yield return) ⬇️ ---
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

        while (Time.time < startTime + dashDuration)
        {
            controller.Move((dashDirection * dashSpeed + originalVerticalVelocity * 0.2f) * Time.deltaTime);
            yield return null; // ❗️ บรรทัดนี้คือหัวใจสำคัญ
        }

        isDashing = false;
    }

    // --- ⬇️ นี่คือฟังก์ชัน Respawn() ที่ถูกต้อง (มีอันเดียว) ⬇️ ---
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