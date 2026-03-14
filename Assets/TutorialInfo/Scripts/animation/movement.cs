using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class movement : MonoBehaviour
{
    [Header("ตั้งค่าความเร็ว (ปรับได้อิสระ)")]
    public float walkSpeed = 5f;        // ความเร็วเดินปกติ
    public float sprintSpeed = 10f;     // ความเร็วตอนกด Shift วิ่ง
    public float jumpForce = 5f;        // แรงพุ่งตอนกระโดด
    
    [Header("การเช็คพื้น")]
    public float groundCheckDistance = 1.1f; // ระยะที่ยิงเส้นเพื่อตรวจว่ายืนติดดินไหม

    [Header("แอนิเมชัน")]
    public Animator animator;           // ช่องสำหรับลาก Animator มาใส่

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // ล็อกไม่ให้ตัวละครล้มกลิ้งไปมาเวิ้งว้างเวลากระแทกของ
        rb.freezeRotation = true; 

        // ถ้าลืมลาก Animator มาใส่ ให้ตัวสคริปต์พยายามหาเอง
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 1. เช็คว่าตัวละครยืนติดพื้นหรือไม่ (ยิงเส้น Raycast ลงไปด้านล่าง)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // ส่งสถานะพื้นบอก Animator ให้รู้
        if (animator != null)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        // 2. กระโดด (กด Spacebar และต้องยืนอยู่บนพื้นเท่านั้น)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // ดันตัวละครขึ้นบนฟ้า
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            if (animator != null)
            {
                animator.SetTrigger("Jump"); // เล่นท่ากระโดด
            }
        }
    }

    void FixedUpdate()
    {
        // 3. รับค่าทิศทางการเดิน W A S D
        float horizontal = Input.GetAxisRaw("Horizontal"); // A (-1), D (1)
        float vertical = Input.GetAxisRaw("Vertical");     // S (-1), W (1)

        // ผสมทิศทางให้เป็นเวกเตอร์เดียว พร้อมปรับขนาดไม่ให้เดินเฉียงไวเกินปกติ (.normalized)
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        // 4. คำนวณความเร็ว (เช็คว่ากด Left Shift ค้างอยู่หรือไม่)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = walkSpeed;

        if (isSprinting) 
        {
            currentSpeed = sprintSpeed; // เปลี่ยนโหมดเป็นความเร็ววิ่ง
        }

        // ถ้าปล่อยปุ่มหมด (ไม่ได้เดิน) ให้ความเร็วเป็นศูนย์
        if (moveDirection.magnitude == 0)
        {
            currentSpeed = 0f;
        }

        // 5. สั่งตัวละครให้เคลื่อนที่ในแกนแนวนอน ส่วนแนวแกน Y (ความสูง) ปล่อยให้มันร่วงตามแรงโน้มถ่วงเดิม
        Vector3 movementVec = moveDirection * currentSpeed;
        rb.velocity = new Vector3(movementVec.x, rb.velocity.y, movementVec.z);

        // 6. อัปเดตความเร็วไปยัง Animator
        if (animator != null)
        {
            float animationSpeedValue = 0f; // สมมติว่าพารามิเตอร์ของ Animator เราใช้: 0=ยืนนิ่ง, 1=เดิน, 2=วิ่ง
            
            if (currentSpeed == walkSpeed && moveDirection.magnitude > 0) animationSpeedValue = 1f;
            else if (currentSpeed == sprintSpeed && moveDirection.magnitude > 0) animationSpeedValue = 2f;

            // เอาค่าที่ได้ไปยัดใส่ตัวแปร "Speed" ใน Animator
            animator.SetFloat("Speed", animationSpeedValue);
        }
    }
}
