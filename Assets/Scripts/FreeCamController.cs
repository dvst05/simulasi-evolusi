using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCamController : MonoBehaviour
{
    [Header("Pengaturan Free Cam")]
    public float moveSpeed = 15f;
    public float lookSpeed = 0.2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    // Variabel untuk mode First-Person (Possess)
    private Creature possessedCreature = null;
    private float fpLookX = 0f;
    private float fpLookY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // Tekan ESC untuk memunculkan kursor, Klik Kiri di layar untuk mengunci lagi
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Cek apakah sedang di dalam tubuh makhluk atau mode Free Cam
        if (possessedCreature != null)
        {
            HandleFirstPersonMode();
        }
        else
        {
            HandleFreeCamMode();
        }
    }

    void HandleFreeCamMode()
    {
        // 1. Rotasi kamera pakai mouse
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        rotationX += mouseDelta.x * lookSpeed;
        rotationY -= mouseDelta.y * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);
        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // 2. Gerak WASD + Q/E
        float moveForward = 0f;
        float moveSide = 0f;
        if (Keyboard.current.wKey.isPressed) moveForward += 1f;
        if (Keyboard.current.sKey.isPressed) moveForward -= 1f;
        if (Keyboard.current.dKey.isPressed) moveSide += 1f;
        if (Keyboard.current.aKey.isPressed) moveSide -= 1f;

        Vector3 moveDirection = new Vector3(moveSide, 0f, moveForward).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Keyboard.current.eKey.isPressed)
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        if (Keyboard.current.qKey.isPressed)
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);

        // 3. Klik Kiri untuk menembakkan sinar (Raycast) dan masuk ke makhluk
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Creature target = hit.collider.GetComponentInParent<Creature>();
                if (target != null)
                {
                    EnterCreature(target);
                }
            }
        }
    }

    void EnterCreature(Creature target)
    {
        possessedCreature = target;
        fpLookX = 0f;
        fpLookY = 0f;

        // Jadikan kamera menempel (child) di tubuh makhluk, tepat di posisi mata
        transform.SetParent(target.transform);
        transform.localPosition = new Vector3(0f, 1.7f, 1.9f);
        transform.localRotation = Quaternion.identity;
    }

    void HandleFirstPersonMode()
    {
        // Kalau makhluk yang kita masuki mati kelaparan, otomatis keluar ke Free Cam
        if (possessedCreature == null)
        {
            ExitCreature();
            return;
        }

        // Tetap bisa tengok kanan-kiri atas-bawah dari sudut pandang makhluk
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        fpLookX += mouseDelta.x * lookSpeed;
        fpLookY -= mouseDelta.y * lookSpeed;
        fpLookX = Mathf.Clamp(fpLookX, -85f, 85f);
        fpLookY = Mathf.Clamp(fpLookY, -60f, 60f);

        transform.localRotation = Quaternion.Euler(fpLookY, fpLookX, 0f);

        // Tekan Spasi untuk keluar kembali ke mode Free Cam
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExitCreature();
        }
    }

    void ExitCreature()
    {
        possessedCreature = null;
        transform.SetParent(null); // Lepas dari tubuh makhluk
        transform.position += Vector3.up * 3f; // Naik sedikit biar nggak nyangkut di tanah

        // Sinkronkan sudut rotasi agar transisi kamera mulus
        rotationX = transform.eulerAngles.y;
        rotationY = 0f;
    }
}