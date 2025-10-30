using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))] // บังคับให้ต้องมี CharacterController
public class SimplePlayerMovement: MonoBehaviour
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
        if (controller == null) { /* ... (โค้ดเดิม) ... */ }

        standingHeight = controller.height;

        if (playerCamera != null)
        {
            // --- ⬇️ นี่คือส่วนที่แก้ไข/เพิ่มเข้ามา ⬇️ ---

            // 1. ค้นหา Component "Camera" ที่ติดอยู่กับ "playerCamera"
            cameraComponent = playerCamera.GetComponent<Camera>();
            if (cameraComponent != null)
            {
                // 2. ถ้าเจอ ให้เก็บค่า FOV ปัจจุบันไว้เป็น "FOV ปกติ"
                normalFOV = cameraComponent.fieldOfView;
            }
            else
            {
                // 3. ถ้าไม่เจอ Component Camera ให้แจ้งเตือน (สำคัญมาก)
                Debug.LogError("Player Camera Transform does not have a Camera component!");
                enabled = false;
                return;
            }
            // --- ⬆️ จบส่วนที่เพิ่ม ⬆️ ---

            standingCameraPos = playerCamera.localPosition;
            crouchCameraPos = new Vector3(standingCameraPos.x, standingCameraPos.y - (standingHeight - crouchHeight), standingCameraPos.z);
        }
        else { /* ... (โค้ด Error เดิม) ... */ }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        wallRunTimer = maxWallRunTime;
    }

    void Update()
    {
        // --- จัดการ Cooldown ---
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // --- รับ Input ---
        MyInput(); // รับค่าปุ่มกด
        CheckForWall(); // เช็คกำแพง (สำหรับ Wall Jump / Wall Run)
        HandleWallRunState(); // เช็คสถานะ Wall Run

        // --- เรียกฟังก์ชันหลัก ---
        HandleMovement(); // จัดการการเคลื่อนที่
        HandleMouseLook(); // จัดการมุมกล้อง
        HandleHeightChange(); // จัดการย่อตัว/ยืน
        HandleCameraEffects();
    }
    private void HandleCameraEffects()
    {
        // ถ้าไม่มี Component Camera ก็ไม่ต้องทำอะไร
        if (cameraComponent == null) return;

        float targetFOV; // FOV เป้าหมาย

        // 1. ตรวจสอบสถานะ: ถ้ากำลัง Dash
        if (isDashing)
        {
            targetFOV = dashFOV; // ให้ FOV เป้าหมายเป็น dashFOV
        }
        else
        {
            targetFOV = normalFOV; // ถ้าไม่ได้ Dash ให้ FOV เป้าหมายกลับเป็นปกติ
        }

        // 2. ค่อยๆ เปลี่ยนค่า FOV ปัจจุบัน ไปหาค่าเป้าหมาย (เพื่อให้มันนุ่มนวล)
        cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }
    // แยกการรับ Input ออกมา
    void MyInput()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
        moveInputDirection = transform.right * xInput + transform.forward * zInput;
        moveInputDirection.Normalize();

        wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        bool canSprint = !isCrouching && zInput > 0.1f && isGrounded;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && canSprint;

        // --- เช็ค Input Dash ---
        if (Input.GetKeyDown(KeyCode.Q) && dashCooldownTimer <= 0 && !isDashing && !isCrouching)
        {
            StartCoroutine(Dash());
        }
    }

    private void CheckForWall()
    {
        // ยิง Raycast ไปทางขวา และซ้าย เพื่อหากำแพง
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    // ฟังก์ชันเช็คสถานะ Wall Run
    private void HandleWallRunState()
    {
        // เงื่อนไข: 1. ลอยอยู่, 2. มีกำแพง, 3. กด Shift, 4. ไม่ได้สไลด์/ย่อ
        bool canWallRun = !isGrounded && (wallLeft || wallRight) && Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        if (canWallRun && !isWallRunning) // ถ้าเริ่มไต่
        {
            isWallRunning = true;
            velocity.y = 0f; // ล้างความเร็วตก (เพื่อให้เกาะกำแพง)
            Debug.Log("Start Wall Run!");
        }
        else if (!canWallRun && isWallRunning) // ถ้าหยุดไต่ (เช่น ปล่อย Shift, ไม่มีกำแพง)
        {
            isWallRunning = false;
        }

        // จัดการ Timer
        if (isWallRunning)
        {
            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0) // ถ้าเวลาหมด
            {
                isWallRunning = false; // บังคับหยุด
            }
        }
        else if (isGrounded) // ถ้าแตะพื้น
        {
            wallRunTimer = maxWallRunTime; // รีเซ็ตเวลา (เพื่อให้ไต่ใหม่ได้)
        }
    }


    void HandleMovement()
    {
        if (isDashing) return; // Dash มาก่อน

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0) velocity.y = -2f; // รีเซ็ตแรงโน้มถ่วงเมื่อแตะพื้น

        // --- Logic การเปลี่ยนสถานะ Slide/Crouch ---
        // (ส่วนนี้เหมือนเดิม)
        if (wantsToCrouch && isSprinting && !isSliding && !isCrouching) StartSlide(moveInputDirection);
        else if (!wantsToCrouch && isSliding) StopSlide();

        if (wantsToCrouch && !isSliding) isCrouching = true;
        else if (!wantsToCrouch && !isSliding) isCrouching = false;


        // --- คำนวณความเร็วแนวนอน (State Machine) ---
        Vector3 targetHorizontalVelocity;

        if (isWallRunning) // 1. ไต่กำแพง
        {
            // ... (โค้ด Wall Running เหมือนเดิม) ...
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
            if (Vector3.Dot(transform.forward, wallForward) < 0) wallForward = -wallForward;
            targetHorizontalVelocity = wallForward * wallRunSpeed;
            velocity.y = -1f;
        }
        else if (isSliding) // 2. สไลด์
        {
            // ... (โค้ด Slide เหมือนเดิม) ...
            currentSlideSpeed -= slideFriction * Time.deltaTime;
            if (currentSlideSpeed <= crouchSpeed) { StopSlide(); targetHorizontalVelocity = slideDirection * crouchSpeed; }
            else { targetHorizontalVelocity = slideDirection * currentSlideSpeed; }
        }
        else // 3. เคลื่อนที่ปกติ
        {
            // --- ⬇️ นี่คือส่วนที่แก้ไข ⬇️ ---

            // 1. เช็คว่า "อยาก" วิ่งไหม (กด Shift + W + ไม่ย่อ)
            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && zInput > 0.1f && !isCrouching;

            // 2. กำหนดความเร็ว
            float currentSpeed;
            if (isCrouching)
            {
                currentSpeed = crouchSpeed;
            }
            else if (wantsToSprint)
            { // ถ้า "อยาก" วิ่ง
                currentSpeed = sprintSpeed; // ใช้ sprintSpeed (ไม่ว่าจะลอยอยู่หรือไม่ก็ตาม)
            }
            else
            {
                currentSpeed = moveSpeed;
            }

            // 3. ใช้ความเร็วที่ถูกต้อง
            targetHorizontalVelocity = moveInputDirection * currentSpeed;

            // --- ⬆️ จบส่วนที่แก้ไข ⬆️ ---
        }


        // --- Slope Check (เหมือนเดิม) ---
        if (!isWallRunning)
        {
            // ... (โค้ด Slope Check ทั้งหมดเหมือนเดิม) ...
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


        // --- การกระโดด (เหมือนเดิม) ---
        if (Input.GetButtonDown("Jump"))
        {
            // ... (โค้ด Jump ทั้งหมดเหมือนเดิม) ...
            if (isWallRunning) { WallJump(); isWallRunning = false; wallRunTimer = 0f; }
            else if (isGrounded)
            {
                if (isSliding) { velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); StopSlide(); }
                else if (!isCrouching) { velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); }
            }
            else if (wallLeft || wallRight) { WallJump(); }
        }

        // --- ใช้แรงโน้มถ่วง (เหมือนเดิม) ---
        if (!isWallRunning)
        {
            if (!isGrounded || velocity.y > -slopeForce + 0.1f)
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        // --- รวมความเร็วสุดท้าย และ Move (เหมือนเดิม) ---
        Vector3 finalVelocity = targetHorizontalVelocity;
        finalVelocity.y = velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void WallJump()
    {
        Debug.Log("Wall Jump!");
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // กำหนด Velocity ใหม่เลย (X, Y, Z)
        velocity = forceToApply;
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // --- ⬇️ เพิ่ม Camera Tilt ⬇️ ---
        // 1. คำนวณเป้าหมายการเอียง
        float targetTilt = 0f;
        if (isWallRunning)
        {
            targetTilt = wallLeft ? -wallRunCameraTilt : wallRunCameraTilt;
        }

        // 2. ค่อยๆ เอียงกล้องไปหาเป้าหมาย (ใช้ LerpAngle สำหรับมุม)
        float currentTilt = Mathf.LerpAngle(playerCamera.localRotation.eulerAngles.z, targetTilt, Time.deltaTime * cameraTiltSpeed);

        // 3. ตั้งค่ามุมกล้อง (รวมมุมก้มเงย และมุมเอียง)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        // --- ⬆️ จบส่วน Camera Tilt ⬆️ ---

        // หมุนตัวละครซ้าย/ขวา
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeightChange()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float currentHeight = controller.height;

        // เช็คเพดานก่อนยืน (ใช้ SphereCast)
        if (!isCrouching && currentHeight < standingHeight - 0.1f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * (currentHeight / 2 + 0.05f);
            if (Physics.SphereCast(rayOrigin, controller.radius, Vector3.up, out RaycastHit headHit, standingHeight - currentHeight + 0.1f, ~0, QueryTriggerInteraction.Ignore)) // ~0 = ทุก Layer, Ignore Trigger
            {
                targetHeight = crouchHeight;
                isCrouching = true;
                if (isSliding) StopSlide();
            }
        }

        // ค่อยๆ เปลี่ยนความสูงและ Center
        controller.height = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * cameraHeightChangeSpeed * 2);
        controller.center = new Vector3(0, controller.height / 2, 0);

        // --- ปรับตำแหน่งกล้อง ---
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
        // isCrouching จะถูกตัดสินใน HandleMovement
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        float startTime = Time.time;
        Vector3 originalVerticalVelocity = new Vector3(0, velocity.y, 0); // เก็บแค่ Y

        float xDash = Input.GetAxisRaw("Horizontal");
        float zDash = Input.GetAxisRaw("Vertical");
        Vector3 dashInputDirection = transform.right * xDash + transform.forward * zDash;
        Vector3 dashDirection = dashInputDirection.magnitude > 0.1f ? dashInputDirection.normalized : transform.forward;

        while (Time.time < startTime + dashDuration)
        {
            // พุ่งไปตามทิศทาง + รักษาความเร็วตกเดิมไว้เล็กน้อย (กันทะลุพื้น)
            controller.Move((dashDirection * dashSpeed + originalVerticalVelocity * 0.2f) * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }
}