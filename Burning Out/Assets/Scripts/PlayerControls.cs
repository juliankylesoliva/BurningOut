using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    /* COMPONENTS */
    Rigidbody2D rb;
    SpriteRenderer sprite;

    /* EDITOR VARIABLES */
    [SerializeField] int currentHealth = 200;
    [SerializeField] int maximumHealth = 200;
    [SerializeField] int healthRegenRate = 1;
    [SerializeField] int speedHealthDrainRate = 2;
    [SerializeField] int baseBlastJumpHealthCost = 5;

    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float speedLimit = 18f;
    [SerializeField] Color speedLimitCharColor;

    [SerializeField] float jumpSpeed = 8f;
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
    [SerializeField] Transform groundCheckObject;
    [SerializeField] Transform wallCheckObjectR;
    [SerializeField] Transform wallCheckObjectL;

    /* SCRIPT VARIABLES */
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isTouchingWall = false;
    private bool isBlastJumping = false;
    private bool isBlastJumpCooldownActive = false;
    private int midairBlastJumps = 0;

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
        Debug.Log(currentHealth);
    }

    void DoMovement()
    {
        if (isDead) { return; }

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
        }
    }

    void DoWallCheck()
    {
        if (isDead) { return; }

        isTouchingWall = false;
        Collider2D[] collidersR = Physics2D.OverlapCircleAll(wallCheckObjectR.position, groundCheckRadius, groundLayer);
        Collider2D[] collidersL = Physics2D.OverlapCircleAll(wallCheckObjectL.position, groundCheckRadius, groundLayer);
        isTouchingWall = (collidersR.Length > 0 || collidersL.Length > 0);

        if (isTouchingWall && isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
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

        if (horizontalDirection == 0f && verticalDirection == 0f)
        {
            verticalDirection = 1f;
        }
        else if (verticalDirection == 0f)
        {
            verticalDirection = 0.5f;
        }
        else
        {
            /* Nothing */
        }

        rb.velocity = new Vector2(horizontalDirection * blastJumpSpeed, verticalDirection * blastJumpSpeed);

        if (!isGrounded)
        {
            ++midairBlastJumps;
        }

        changeHealthBy(-(baseBlastJumpHealthCost + (baseBlastJumpHealthCost * midairBlastJumps)));

        yield return new WaitForSeconds(blastJumpRecoveryTime);

        isBlastJumping = false;
        isBlastJumpCooldownActive = true;

        yield return new WaitForSeconds(blastJumpCooldownTime);

        if (isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
        }
    }

    void DoHealthDrainRegen()
    {
        if (isDead) { return; }

        if (rb.velocity.magnitude >= speedLimit)
        {
            sprite.color = speedLimitCharColor;
            changeHealthBy(-speedHealthDrainRate);
        }
        else
        {
            sprite.color = Color.white;
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
}
