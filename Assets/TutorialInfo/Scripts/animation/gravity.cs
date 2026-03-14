using UnityEngine;

public class PlayerAntiGravity : MonoBehaviour 
{
    private Rigidbody rb;
    public float speed = 5f;
    public bool isAntiGravity = false;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        // กด G เพื่อเปิด/ปิดแรงโน้มถ่วง
        if (Input.GetKeyDown(KeyCode.G)) {
            isAntiGravity = !isAntiGravity;
            rb.useGravity = !isAntiGravity;
        }

        // ระบบเดินแบบพื้นฐาน (ถ้า WASD ปกติยังไม่ทำงาน)
        float moveH = Input.GetAxis("Horizontal");
        float moveV = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(moveH, 0, moveV) * speed * Time.deltaTime;
        transform.Translate(move);
    }
}