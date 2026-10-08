using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Vector3 respawnPoint;
    public bool isDead { get; private set; }

    [Header("UI References")]
    public Image healthBarFill;  // Assign if using Image (requires UISprite assigned)
    public Slider healthSlider;  // Assign if using Slider

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float mouseSensitivity = 20f;
    public float gravity = -9.81f;
    public Transform cameraTransform;

    [Header("ADS (Aim Down Sights)")]
    public float normalFOV = 60f;
    public float adsFOV = 40f;
    public float adsSpeed = 10f;
    public Vector3 adsGunOffset = new Vector3(-0.3f, 0.05f, -0.1f);

    [Header("Weapon & FX")]
    public Transform gunTransform;
    public Transform muzzlePoint;
    public float attackRange = 50f;
    public float baseDamage = 25f;
    public DamageType currentDamageType = DamageType.Physical;

    private CharacterController controller;
    private Camera playerCam;
    private float cameraPitch = 0f;
    private Vector3 originalGunPosition;
    private Vector3 velocity;

    [Header("Effects & Audio")]
    public GameObject muzzleFlashPrefab;
    public AudioSource gunAudioSource;
    public AudioClip shootSound;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        respawnPoint = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
        {
            playerCam = cameraTransform.GetComponent<Camera>();
        }
        if (playerCam == null)
        {
            playerCam = Camera.main;
        }

        if (gunTransform != null)
        {
            originalGunPosition = gunTransform.localPosition;
        }

        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Respawn();
            }
            return;
        }

        HandleLook();
        HandleMovement();
        HandleADS();
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

        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;

        bool isSprinting = Keyboard.current.leftShiftKey.isPressed;
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 moveInput = (transform.right * x + transform.forward * z).normalized * currentSpeed;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(gravity));
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveInput + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleADS()
    {
        if (Mouse.current == null) return;

        bool isAiming = Mouse.current.rightButton.isPressed;

        if (playerCam != null)
        {
            float targetFOV = isAiming ? adsFOV : normalFOV;
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);
        }

        if (gunTransform != null)
        {
            Vector3 targetGunPos = isAiming ? (originalGunPosition + adsGunOffset) : originalGunPosition;
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, targetGunPos, Time.deltaTime * adsSpeed);
        }
    }

    void HandleShooting()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (gunTransform != null)
            {
                gunTransform.localPosition -= Vector3.forward * 0.15f;
            }

            CreateMuzzleFlash();

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
    }

    void SmoothGunRecoil()
    {
        if (gunTransform != null && Mouse.current != null && !Mouse.current.rightButton.isPressed)
        {
            gunTransform.localPosition = Vector3.Lerp(gunTransform.localPosition, originalGunPosition, Time.deltaTime * 10f);
        }
    }

    void CreateMuzzleFlash()
    {
        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            Destroy(flash, 0.1f);
        }

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
        impact.transform.localScale = Vector3.one * 0.15f; // Fixed: Changed 'scale' to 'localScale'

        Destroy(impact.GetComponent<Collider>());

        Renderer rend = impact.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.yellow;
            Destroy(rend.material, 0.2f); // .material creates an instance that isn't cleaned up with the object
        }

        Destroy(impact, 0.2f);
    }

    // --- Health UI Update ---
    public void UpdateHealthUI()
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
        }

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }
    }

    public void TakeDamage(DamagePayload payload)
    {
        if (isDead) return;

        currentHealth -= payload.amount;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        UpdateHealthUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("=========================================");
        Debug.Log("  YOU DIED! Press 'R' to Respawn.  ");
        Debug.Log("=========================================");
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (controller != null) controller.enabled = false;
        transform.position = respawnPoint;
        if (controller != null) controller.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PLAYER] Respawned at full health!");
    }
}