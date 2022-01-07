using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerControls : MonoBehaviour
{
    /* COMPONENTS */
    Rigidbody2D rb;
    SpriteRenderer sprite;

    /* EDITOR VARIABLES */
    [Header("Editor Variables")]
    [SerializeField] int currentHealth = 200;
    [SerializeField] int maximumHealth = 200;
    [SerializeField] int healthRegenRate = 1;
    [SerializeField] int speedHealthDrainRate = 2;
    [SerializeField] int baseBlastJumpHealthCost = 5;

    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float speedLimit = 18f;
    [SerializeField] Color speedLimitCharColor;

    [SerializeField] float jumpSpeed = 8f;
    [SerializeField] float wallSlideSpeed = 12f;
    [SerializeField] float risingGravity = 1f;
    [SerializeField] float fallingGravity = 3f;

    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float blastJumpWindupTime = 0.15f;
    [SerializeField] float blastJumpRecoveryTime = 0.25f;
    [SerializeField] float blastJumpCooldownTime = 0.8f;
    [SerializeField] float windupJumpSpeed = 4f;
    [SerializeField] float blastJumpSpeed = 16f;

    /* DRAG AND DROP */
    [Header("Drag and Drop")]
    [SerializeField] Transform groundCheckObject;
    [SerializeField] Transform wallCheckObjectR;
    [SerializeField] Transform wallCheckObjectL;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_Text speedText;
    [SerializeField] TrailRenderer speedTrail;
    [SerializeField] GameObject burstParticles;
    [SerializeField] GameObject postBurstParticles;
    [SerializeField] GameObject wallSlideParticles;
    [SerializeField] ParticleSystem burstReadyParticles;

    /* SCRIPT VARIABLES */
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isSliding = false;
    private bool isTouchingWallR = false;
    private bool isTouchingWallL = false;
    private bool isTouchingWall = false;
    private bool isBlastJumping = false;
    private bool isBlastJumpCooldownActive = false;
    private int consecutiveBlastJumps = 0;

    void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        sprite = this.gameObject.GetComponent<SpriteRenderer>();
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        DoGroundCheck();
        DoWallCheck();
        DoMovement();
        DoJumping();
        DoBlastJump();
        DoFalling();
        DoHealthDrainRegen();
        DoUIUpdate();
    }

    void DoMovement()
    {
        if (isDead)
        {
            rb.simulated = false;
            return;
        }

        if (!isBlastJumping)
        {
            if (Mathf.Abs(rb.velocity.x) <= moveSpeed)
            {
                rb.velocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, rb.velocity.y);
            }
            else
            {
                if (Input.GetAxis("Horizontal") * rb.velocity.x <= 0f)
                {
                    rb.velocity += new Vector2(Input.GetAxis("Horizontal") * moveSpeed, 0f);
                }
            }
        }
    }

    void DoGroundCheck()
    {
        if (isDead) { return; }

        isGrounded = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckObject.position, groundCheckRadius, groundLayer);
        isGrounded = (colliders.Length > 0);

        if (isGrounded && isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
            if (burstReadyParticles != null && !isDead)
            {
                burstReadyParticles.Play();
            }
        }

        if (isGrounded)
        {
            consecutiveBlastJumps = 0;
        }
    }

    void DoWallCheck()
    {
        if (isDead) { return; }

        isTouchingWall = false;
        Collider2D[] collidersR = Physics2D.OverlapCircleAll(wallCheckObjectR.position, groundCheckRadius, groundLayer);
        Collider2D[] collidersL = Physics2D.OverlapCircleAll(wallCheckObjectL.position, groundCheckRadius, groundLayer);
        isTouchingWallR = collidersR.Length > 0;
        isTouchingWallL = collidersL.Length > 0;
        isTouchingWall = (isTouchingWallR || isTouchingWallL);

        if (isTouchingWall && isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
            if (burstReadyParticles != null && !isDead)
            {
                burstReadyParticles.Play();
            }
        }
    }

    void DoJumping()
    {
        if (isDead) { return; }

        if (isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    void DoFalling()
    {
        if (isDead) { return; }

        if (!isGrounded && rb.velocity.y < 0f)
        {
            rb.gravityScale = fallingGravity;
        }
        else
        {
            rb.gravityScale = risingGravity;
        }

        ParticleSystem tempParticleSys = null;
        if (!isGrounded && rb.velocity.y < 0f && isTouchingWall && Input.GetKey(KeyCode.Z))
        {
            rb.velocity = new Vector2(0f, -wallSlideSpeed);
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }
    }

    void DoBlastJump()
    {
        if (isDead) { return; }

        if (!isBlastJumping && !isBlastJumpCooldownActive && Input.GetKeyDown(KeyCode.X))
        {
            isBlastJumping = true;
            StartCoroutine(DoBlastJumpCR());
        }
    }

    IEnumerator DoBlastJumpCR()
    {
        if (isDead) { yield break; }

        rb.velocity = new Vector2(rb.velocity.x, windupJumpSpeed);
        yield return new WaitForSeconds(blastJumpWindupTime);

        float horizontalDirection;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalDirection = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalDirection = 1f;
        }
        else
        {
            horizontalDirection = 0f;
        }

        float verticalDirection;
        if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalDirection = -1f;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            verticalDirection = 1f;
        }
        else
        {
            verticalDirection = 0f;
        }

        if (horizontalDirection != 0f && verticalDirection == 0f)
        {
            verticalDirection = 0.5f;
        }

        GameObject postBurstObj = null;
        if (horizontalDirection != 0f || verticalDirection != 0f)
        {
            rb.velocity = new Vector2(horizontalDirection * blastJumpSpeed, verticalDirection * blastJumpSpeed);
            Instantiate(burstParticles, this.transform.position, Quaternion.identity);
            postBurstObj = Instantiate(postBurstParticles, this.transform);

            ++consecutiveBlastJumps;

            changeHealthBy((int)(-(baseBlastJumpHealthCost + (baseBlastJumpHealthCost * Mathf.Pow(2f, (float)consecutiveBlastJumps)))));
        }

        yield return new WaitForSeconds(blastJumpRecoveryTime);

        isBlastJumping = false;
        isBlastJumpCooldownActive = true;
        if (postBurstObj != null)
        {
            postBurstObj.transform.parent = null;
        }

        yield return new WaitForSeconds(blastJumpCooldownTime);

        if (isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
            if (burstReadyParticles != null && !isDead)
            {
                burstReadyParticles.Play();
            }
        }

        yield break;
    }

    void DoHealthDrainRegen()
    {
        if (isDead) { return; }

        if (rb.velocity.magnitude >= speedLimit)
        {
            sprite.color = speedLimitCharColor;
            if (speedTrail != null) { speedTrail.emitting = true; }
            changeHealthBy(-speedHealthDrainRate);
        }
        else
        {
            sprite.color = Color.white;
            if (speedTrail != null) { speedTrail.emitting = false; }
            changeHealthBy(healthRegenRate);
        }
    }

    void changeHealthBy(int amount)
    {
        if (isDead) { return; }

        currentHealth += amount;
        if (amount > 0)
        {
            if (currentHealth > maximumHealth)
            {
                currentHealth = maximumHealth;
            }
        }
        else
        {
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
            }
        }
    }

    void DoUIUpdate()
    {
        healthSlider.maxValue = maximumHealth;
        healthSlider.value = currentHealth;
        healthText.text = $"{currentHealth} HP";
        speedText.text = $"Speed: {(int)rb.velocity.magnitude} / {speedLimit}";
    }
}
