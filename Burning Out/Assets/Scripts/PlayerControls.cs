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
    Scorekeeper score;
    ScreenShake shake;
    SoundPlayer sounds;
    PlayerParticles particles;

    /* EDITOR VARIABLES */
    [Header("Editor Variables")]
    [SerializeField] int FPS = 60;

    [SerializeField] Vector2 respawnPosition;

    [SerializeField] int currentHealth = 200;
    [SerializeField] int maximumHealth = 200;
    [SerializeField] int healthRegenRate = 1;
    [SerializeField] int wallSlideHealthRegen = 3;
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
    [SerializeField] int midairJumpPenaltyMultiplier = 2;

    /* DRAG AND DROP */
    [Header("Drag and Drop")]
    [SerializeField] Transform groundCheckObject;
    [SerializeField] Transform wallCheckObjectR;
    [SerializeField] Transform wallCheckObjectL;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_Text speedText;
    [SerializeField] Slider speedSlider;
    [SerializeField] Image redFXPanel;
    [SerializeField] GameObject losePanel;

    /* SCRIPT VARIABLES */
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isLevelWon = false;
    private bool isGrounded = false;
    private bool isMoving = false;
    private bool isFalling = false;
    private bool isSliding = false;
    private bool isAboveSpeedLimit = false;
    private bool isTouchingWallR = false;
    private bool isTouchingWallL = false;
    private bool isTouchingWall = false;
    private bool isBlastJumping = false;
    private bool isBlastJumpRecoveryActive = false;
    private bool isBlastJumpCooldownActive = false;
    private int consecutiveBlastJumps = 0;

    public PlayerStates getPlayerStatesObject()
    {
        return new PlayerStates(isFacingRight, isDead, isLevelWon, isGrounded, isMoving, isFalling, isSliding, isAboveSpeedLimit, isTouchingWallR, isTouchingWallL, isTouchingWall, isBlastJumping, isBlastJumpRecoveryActive, isBlastJumpCooldownActive);
    }

    void Awake()
    {
        Application.targetFrameRate = FPS;
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        sprite = this.gameObject.GetComponent<SpriteRenderer>();
        shake = this.gameObject.GetComponent<ScreenShake>();
        score = this.gameObject.GetComponent<Scorekeeper>();
        sounds = this.gameObject.GetComponent<SoundPlayer>();
        particles = this.gameObject.GetComponent<PlayerParticles>();
        respawnPosition = this.transform.position;
    }

    void Update()
    {
        DoGroundCheck();
        DoFalling();
        DoWallCheck();
        DoWallSlide();
        DoMovement();
        DoMovementCheck();
        DoJumping();
        DoBlastJumpCooldownReset();
        DoBlastJump();
        DoHealthDrainRegen();
        DoDirectionCheck();
        DoUIUpdate();
        DoTimerStart();
        DoRespawn();
        DoQuit();
    }

    void DoMovement()
    {
        if (isLevelWon) { return; }

        if (isDead)
        {
            rb.simulated = false;
            return;
        }
        else
        {
            if (!rb.simulated)
            {
                rb.simulated = true;
            }
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
                    rb.velocity = new Vector2(Input.GetAxis("Horizontal") * moveSpeed, rb.velocity.y);
                }
            }
        }
    }

    void DoDirectionCheck()
    {
        if (rb.velocity.x != 0f)
        {
            isFacingRight = rb.velocity.x > 0f;
        }
    }

    void DoMovementCheck()
    {
        isMoving = rb.velocity.magnitude > 0f;
    }

    void DoGroundCheck()
    {
        isGrounded = false;
        if (isDead) { return; }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheckObject.position, groundCheckRadius, groundLayer);
        isGrounded = (colliders.Length > 0);

        if (isGrounded)
        {
            consecutiveBlastJumps = 0;
        }
    }

    void DoWallCheck()
    {
        isTouchingWall = false;
        if (isDead) { return; }

        Collider2D[] collidersR = Physics2D.OverlapCircleAll(wallCheckObjectR.position, groundCheckRadius, groundLayer);
        Collider2D[] collidersL = Physics2D.OverlapCircleAll(wallCheckObjectL.position, groundCheckRadius, groundLayer);
        isTouchingWallR = collidersR.Length > 0;
        isTouchingWallL = collidersL.Length > 0;
        isTouchingWall = (isTouchingWallR || isTouchingWallL);
    }

    void DoJumping()
    {
        if (isDead || isLevelWon) { return; }

        if (isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            sounds.PlaySound(0, 0.5f);
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    void DoFalling()
    {
        isFalling = false;
        if (isDead || isLevelWon) { return; }

        if (!isGrounded && rb.velocity.y < 0f)
        {
            isFalling = true;
            rb.gravityScale = fallingGravity;
        }
        else
        {
            isFalling = false;
            rb.gravityScale = risingGravity;
        }
    }

    void DoWallSlide()
    {
        isSliding = false;
        if (isDead || isLevelWon) { return; }

        if (isFalling && Input.GetKey(KeyCode.Z) && ((isTouchingWallR && Input.GetAxis("Horizontal") > 0f) || (isTouchingWallL && Input.GetAxis("Horizontal") < 0f)))
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
        if (isDead || isLevelWon) { return; }

        if (!isBlastJumping && !isBlastJumpRecoveryActive && !isBlastJumpCooldownActive && Input.GetKeyDown(KeyCode.X))
        {
            isBlastJumping = true;
            StartCoroutine("DoBlastJumpCR");
        }
    }

    IEnumerator DoBlastJumpCR()
    {
        if (isDead || isLevelWon) { yield break; }

        sounds.PlaySound(1, 0.5f);
        rb.velocity = new Vector2(rb.velocity.x, windupJumpSpeed);

        yield return new WaitForSeconds(blastJumpWindupTime);

        float horizontalDirection;
        if (Input.GetAxis("Horizontal") < 0f)
        {
            horizontalDirection = -1f;
        }
        else if (Input.GetAxis("Horizontal") > 0f)
        {
            horizontalDirection = 1f;
        }
        else
        {
            horizontalDirection = 0f;
        }

        float verticalDirection;
        if (Input.GetAxis("Vertical") < 0f)
        {
            verticalDirection = -1f;
        }
        else if (Input.GetAxis("Vertical") > 0f)
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

        if (horizontalDirection != 0f || verticalDirection != 0f)
        {
            sounds.PlaySound(2, 0.6f);
            rb.velocity = new Vector2(horizontalDirection * blastJumpSpeed, verticalDirection * blastJumpSpeed);

            shake.DoShake(16f, 0.05f);

            ++consecutiveBlastJumps;

            changeHealthBy((int)(-baseBlastJumpHealthCost * Mathf.Pow((float)midairJumpPenaltyMultiplier, (float)(consecutiveBlastJumps - 1))));

            isBlastJumpRecoveryActive = true;
            yield return new WaitForSeconds(blastJumpRecoveryTime);
        }

        isBlastJumpRecoveryActive = false;
        isBlastJumping = false;
        isBlastJumpCooldownActive = true;
        

        yield return new WaitForSeconds(blastJumpCooldownTime);

        if (isBlastJumpCooldownActive)
        {
            isBlastJumpCooldownActive = false;
            if (!isDead)
            {
                particles.PlayBlastJumpRefresh();
                sounds.PlaySound(3, 0.7f);
            }
        }

        yield break;
    }

    void DoBlastJumpCooldownReset()
    {
        if (isDead) { return; }

        if (isBlastJumpCooldownActive && (isGrounded || isSliding))
        {
            StopCoroutine("DoBlastJumpCR");
            isBlastJumpCooldownActive = false;
            if (!isDead)
            {
                particles.PlayBlastJumpRefresh();
                sounds.PlaySound(3, 0.7f);
            }
        }
    }

    void DoHealthDrainRegen()
    {
        if (isDead || isLevelWon) { return; }

        if (rb.velocity.magnitude >= speedLimit)
        {
            isAboveSpeedLimit = true;
            changeHealthBy((int)(-speedHealthDrainRate * Time.deltaTime * FPS));
        }
        else
        {
            sprite.color = Color.white;
            isAboveSpeedLimit = false;
            if (isSliding)
            {
                changeHealthBy((int)(wallSlideHealthRegen * Time.deltaTime * FPS));
            }
            else
            {
                changeHealthBy((int)(healthRegenRate * Time.deltaTime * FPS));
            }
        }
    }

    void changeHealthBy(int amount)
    {
        if (isDead || isLevelWon) { return; }

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
                sounds.PlaySound(4, 0.4f);
                shake.DoShake(32f, 0.6f);
                currentHealth = 0;
                sprite.color = Color.clear;
                isDead = true;
                losePanel.SetActive(true);
                score.StopScorekeeper();
            }
        }
    }

    void DoRespawn()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StopCoroutine("DoBlastJumpCR");

            shake.DoShake(0f, 0f);

            losePanel.SetActive(false);
            score.HideResultsScreen();
            score.StopScorekeeper();
            score.ResetScorekeeper();

            sprite.color = Color.white;

            this.transform.position = respawnPosition;
            currentHealth = maximumHealth;
            rb.velocity = Vector2.zero;

            isFacingRight = true;
            isDead = false;
            isLevelWon = false;
            isGrounded = false;
            isMoving = false;
            isFalling = false;
            isSliding = false;
            isAboveSpeedLimit = false;
            isTouchingWallR = false;
            isTouchingWallL = false;
            isTouchingWall = false;
            isBlastJumping = false;
            isBlastJumpRecoveryActive = false;
            isBlastJumpCooldownActive = false;
            consecutiveBlastJumps = 0;
        }
    }

    void DoTimerStart()
    {
        if (!isDead && !isLevelWon && !score.getIsActive() && rb.velocity.magnitude > 0f)
        {
            score.StartScorekeeper();
        }
    }

    void DoUIUpdate()
    {
        healthSlider.maxValue = maximumHealth;
        healthSlider.value = currentHealth;
        healthText.text = $"Fuse: {currentHealth}";
        speedSlider.maxValue = speedLimit;
        speedSlider.value = (int)rb.velocity.magnitude;
        speedText.text = $"Speed: {(int)rb.velocity.magnitude}";

        if (!isLevelWon)
        {
            redFXPanel.color = Color.Lerp(new Color(1f, 0f, 0f, 0f), new Color(1f, 0f, 0f, 0.5f), (1f - (((float)currentHealth) / ((float)maximumHealth))));
        }
        else
        {
            redFXPanel.color = Color.clear;
        }
    }

    void DoQuit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Goal" && !isDead)
        {
            isLevelWon = true;
            score.StopScorekeeper();
            score.CalculateFinalScore(currentHealth, maximumHealth);
            score.ShowResultsScreen();
        }
    }
}
