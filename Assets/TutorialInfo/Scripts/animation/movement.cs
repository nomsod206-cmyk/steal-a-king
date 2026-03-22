using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Movement : MonoBehaviour
{
    [Header("การเคลื่อนที่ (Movement Settings)")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    [Range(0, 1)] public float airControlMin = 0.3f; // แรงบังคับกลางอากาศ

    [Header("ช่องใส่ Animation")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationClip jumpClip;

    [Header("กล้อง (Camera Reference)")]
    public Transform mainCamera;

    private Rigidbody rb;
    private Animator animator;
    
    private string currentAnimName;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        if (mainCamera == null && Camera.main != null)
        {
            mainCamera = Camera.main.transform;
        }

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // กันสั่น

        if (idleClip != null)
        {
            animator.Play(idleClip.name);
            currentAnimName = idleClip.name;
        }
    }

    void Update()
    {
        // 1. รับค่า Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        // 2. คำนวณทิศทางตามมุมกล้อง
        Vector3 moveDirection = Vector3.zero;
        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.forward;
            Vector3 camRight = mainCamera.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            moveDirection = (camRight * moveX + camForward * moveZ).normalized;
        }

        bool isWalking = moveDirection.sqrMagnitude > 0.01f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 3. การเคลื่อนที่และการหมุนตัว
        if (isWalking)
        {
            // หมุนตัวละครไปตามทิศที่เดิน
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            
            if (isGrounded)
            {
                // เดินบนพื้นปกติ
                rb.linearVelocity = new Vector3(moveDirection.x * currentSpeed, rb.linearVelocity.y, moveDirection.z * currentSpeed);
            }
            else
            {
                // ควบคุมกลางอากาศ (Air Control)
                Vector3 airMove = moveDirection * currentSpeed * airControlMin;
                rb.AddForce(airMove, ForceMode.Acceleration);
            }
        }
        else if (isGrounded)
        {
            // หยุดเดินเมื่ออยู่บนพื้น
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // 4. ระบบกระโดด (Space + W = Jump Forward)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float forwardJumpMultiplier = 0.6f; // ปรับค่านี้เพื่อให้พุ่งไปข้างหน้าแรงขึ้น
            Vector3 jumpVector = Vector3.up * jumpForce;

            if (isWalking)
            {
                // เพิ่มแรงส่งไปข้างหน้าตามหน้าหุ่น
                jumpVector += transform.forward * (jumpForce * forwardJumpMultiplier);
            }

            // รีเซ็ต Y ก่อนกระโดดเพื่อให้แรงเท่ากันทุกครั้ง
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(jumpVector, ForceMode.Impulse);
            
            isGrounded = false;
            PlayAnimation(jumpClip);
        }

        // 5. อัปเดตแอนิเมชัน
        UpdateAnimationState(isWalking, isRunning);
    }

    private void UpdateAnimationState(bool isWalking, bool isRunning)
    {
        if (!isGrounded) return; // ถ้าลอยอยู่ ให้เล่นท่า Jump ต่อไป

        if (isRunning) PlayAnimation(runClip);
        else if (isWalking) PlayAnimation(walkClip);
        else PlayAnimation(idleClip);
    }

    private void PlayAnimation(AnimationClip clip)
    {
        if (clip == null) return;
        if (currentAnimName != clip.name)
        {
            animator.CrossFadeInFixedTime(clip.name, 0.15f);
            currentAnimName = clip.name;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ตรวจสอบว่าชนพื้น (แนะนำให้ใส่ Tag "Ground" ที่พื้นใน Unity)
        if (collision.gameObject.CompareTag("Ground") || collision.relativeVelocity.y > 0.1f)
        {
            isGrounded = true;
        }
    }
}