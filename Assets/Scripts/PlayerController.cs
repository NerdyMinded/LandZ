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

    [Header("Effects & Audio")]
    public GameObject muzzleFlashPrefab; // Visual flash (Particle System)
    public AudioSource gunAudioSource;   // The AudioSource component on the gun
    public AudioClip shootSound;         // The gunshot sound file (.wav / .mp3)
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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
        {
            CreateHitImpact(hit.point, hit.normal);

            // Search up from the collider so hits on child colliders (limbs, armor) still find the damageable root
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
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
    // 1. Play the visual particle flash if assigned
    if (muzzleFlashPrefab != null && muzzlePoint != null)
    {
        GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
        Destroy(flash, 0.1f); // Quick cleanup
    }

    // 2. Play the gunshot sound
    if (gunAudioSource != null && shootSound != null)
    {
        gunAudioSource.PlayOneShot(shootSound);
    }
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
            Destroy(rend.material, 0.2f); // .material creates an instance that isn't cleaned up with the object
        }

        Destroy(impact, 0.2f);
    }
}