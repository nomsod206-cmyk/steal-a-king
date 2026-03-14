using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("ความเร็วในการหันมุมกล้อง")]
    public float mouseSensitivity = 100f;

    [Header("เป้าหมายที่ต้องการหมุน (ตัวละครหลัก)")]
    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool isCursorLocked = true;

    void Start()
    {
        // ล็อคเมาส์ไว้กลางจอ และซ่อนลูกศรเมาส์ตอนเริ่มเกม
        LockCursor(true);
    }

    void Update()
    {
        // 1. ตรวจสอบการกดปุ่ม ESC เพื่อสลับสถานะเมาส์
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            LockCursor(isCursorLocked);
        }

        // 2. ถ้าเมาส์ล็อคอยู่ ให้สามารถหันมุมกล้องได้
        if (isCursorLocked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // หมุนขึ้น/ลง (แกน X ของกล้อง) และจำกัดมุม
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // หมุนซ้าย/ขวา (แกน Y ของกล้อง)
            yRotation += mouseX;

            // หมุนกล้องอิสระทั้งขึ้นลง(X) และซ้ายขวา(Y)
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

            // ไม่หมุน playerBody อีกต่อไป (ล็อกแกน Y ของตัวละครหุ่นไว้)
        }
    }

    // ฟังก์ชันสำหรับจัดการสถานะเมาส์
    private void LockCursor(bool locked)
    {
        if (locked)
        {
            // ซ่อนและล็อคเมาส์ไว้กลางจอ
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // แสดงลูกศรและปลดล็อคเมาส์
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
