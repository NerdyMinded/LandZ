using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float mouseSensitivity = 20f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    [Header("Weapon & FX")]
    public Transform gunTransform;
    public Transform muzzlePoint;
    public float attackRange = 50f;
    public float baseDamage = 25f;
    public DamageType currentDamageType = DamageType.Physical;

    private CharacterController controller;
    private float cameraPitch = 0f;
    private Vector3 originalGunPosition;
    private Vector3 velocity; // Handles gravity acceleration

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gunTransform != null)
        {
            originalGunPosition = gunTransform.localPosition;
        }
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleShooting();
        SmoothGunRecoil();
    }

    void HandleLook()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * (mouseSensitivity * 0.01f);

        cameraPitch -= mouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -89f, 89f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseDelta.x);
    }

    void HandleMovement()
    {
        if (Keyboard.current == null) return;

        // Ground check & gravity reset
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Slight downward force to keep grounded on slopes
        }

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        // Apply gravity acceleration over time
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleShooting()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (cameraTransform == null) return;

        if (gunTransform != null)
        {
            gunTransform.localPosition -= Vector3.forward * 0.15f;
        }

        if (muzzlePoint != null)
        {
            CreateMuzzleFlash();
        }

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, attackRange))
        {
            CreateHitImpact(hit.point, hit.normal);

            IDamageable target = hit.transform.GetComponent<IDamageable>();
            if (target != null)
            {
                DamagePayload payload = new DamagePayload
                {
                    amount = baseDamage,
                    type = currentDamageType,
                    hitPoint = hit.point,
                    hitNormal = hit.normal
                };

                target.TakeDamage(payload);
            }
        }
    }

    void SmoothGunRecoil()
    {
        if (gunTransform != null)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalGunPosition, Time.deltaTime * 12f);
        }
    }

    void CreateMuzzleFlash()
    {
        GameObject flashLight = new GameObject("MuzzleFlashLight");
        flashLight.transform.position = muzzlePoint.position;
        Light lightComp = flashLight.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.color = Color.yellow;
        lightComp.range = 5f;
        lightComp.intensity = 8f;

        Destroy(flashLight, 0.05f);
    }

    void CreateHitImpact(Vector3 point, Vector3 normal)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.name = "HitImpact";
        impact.transform.position = point + normal * 0.01f;
        impact.transform.localScale = Vector3.one * 0.15f;

        Destroy(impact.GetComponent<Collider>());

        Renderer rend = impact.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.yellow;
        }

        Destroy(impact, 0.2f);
    }
}