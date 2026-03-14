using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Movement : MonoBehaviour
{
    [Header("การเคลื่อนที่ (Movement Settings)")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;

    [Header("ช่องใส่ Animation (ลากไฟล์ Animation มาใส่ที่นี่)")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationClip jumpClip;

    private Rigidbody rb;
    private Animator animator;
    
    private string currentAnimName;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // ล็อคการหมุนไม่ให้ตัวละครล้มเมื่อใช้ฟิสิกส์ Rigidbody
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. รับค่า Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        // 2. สร้าง Vector ทิศทางอิงตามทิศหน้าหุ่น (Local Space)
        // W = ไปข้างหน้า (transform.forward)
        // S = ถอยหลัง (-transform.forward)
        // A = ไปทางซ้าย (-transform.right)
        // D = ไปทางขวา (transform.right)
        Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;

        // 3. ตรวจสอบการเดินและวิ่ง
        bool isWalking = moveDirection.sqrMagnitude > 0.01f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 4. การเคลื่อนที่ตามทิศทางของหุ่น
        if (isWalking)
        {
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);
        }
        else
        {
            // ถ้าไม่กดปุ่ม ให้ความเร็วแนวราบเป็น 0 (หุ่นจะได้ไม่สไลด์)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 5. กด Spacebar เพื่อกระโดด (ต้องอยู่บนพื้นเท่านั้น)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            PlayAnimation(jumpClip); // เล่นท่ากระโดดทันที
        }

        // 6. เปลี่ยนแอนิเมชันตามสถานะปัจจุบัน (เฉพาะตอนอยู่บนพื้นเท่านั้น)
        if (isGrounded)
        {
            if (isRunning)
            {
                PlayAnimation(runClip);     // ท่าวิ่ง
            }
            else if (isWalking)
            {
                PlayAnimation(walkClip);    // ท่าเดิน
            }
            else
            {
                PlayAnimation(idleClip);    // ท่ายืนนิ่ง
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // เมื่อชนวัตถุใดๆ (ที่คาดว่าเป็นพื้น) ให้สามารถกระโดดใหม่ได้
        // (สามารถประยุกต์เช็ค collision.gameObject.CompareTag("Ground") ได้ถ้าต้องการ)
        isGrounded = true;
    }

    // ฟังก์ชันสำหรับเล่นแอนิเมชันอย่างนุ่มนวล
    private void PlayAnimation(AnimationClip clip)
    {
        // ป้องกัน Error หากไม่ได้ใส่ไฟล์ Animation มาให้
        if (clip == null) return;
        
        // ใช้ชื่อไฟล์ของ Animation เป็นตัวกำหนดเป้าหมาย
        string targetAnim = clip.name;

        // เปลี่ยนแอนิเมชันเฉพาะเมื่อไม่ใช่แอนิเมชันเดิม เพื่อไม่ให้มันเล่นตั้งแต่จุดเริ่มต้นซ้ำๆ
        if (currentAnimName != targetAnim)
        {
            // ใช้ CrossFadeInFixedTime ช่วยให้การเปลี่ยนผ่านระหว่างแอนิเมชันสมูทขึ้น (ในระยะเวลา 0.15 วิ)
            animator.CrossFadeInFixedTime(targetAnim, 0.15f);
            currentAnimName = targetAnim;
        }
    }
}
