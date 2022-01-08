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

    /* DRAG AND DROP */
    [Header("Drag and Drop")]
    [SerializeField] ScreenShake screenShakeObject;
    [SerializeField] Scorekeeper scorekeeperObject;
    [SerializeField] Transform groundCheckObject;
    [SerializeField] Transform wallCheckObjectR;
    [SerializeField] Transform wallCheckObjectL;
    [SerializeField] TMP_Text healthText;
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_Text speedText;
    [SerializeField] Slider speedSlider;
    [SerializeField] TrailRenderer speedTrail;
    [SerializeField] TrailRenderer preBlastTrail;
    [SerializeField] GameObject burstParticles;
    [SerializeField] GameObject postBurstParticles;
    [SerializeField] GameObject wallSlideParticles;
    [SerializeField] GameObject deathExplosionParticles;
    [SerializeField] ParticleSystem burstReadyParticles;

    /* SCRIPT VARIABLES */
    private bool isDead = false;
    private bool isLevelWon = false;
    private bool isGrounded = false;
    private bool isSliding = false;
    private bool isTouchingWallR = false;
    private bool isTouchingWallL = false;
    private bool isTouchingWall = false;
    private ParticleSystem tempSlidingParticleSys = null;
    private bool isBlastJumping = false;
    private bool isBlastJumpCooldownActive = false;
    private int consecutiveBlastJumps = 0;

    void Awake()
    {
        rb = this.gameObject.GetComponent<Rigidbody2D>();
        sprite = this.gameObject.GetComponent<SpriteRenderer>();
        if (scorekeeperObject != null) { scorekeeperObject.ResetScorekeeper(); }
        respawnPosition = this.transform.position;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        DoRespawn();
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
                if (Input.GetAxis("Horizontal") * rb.velocity.x < 0f)
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
    }

    void DoJumping()
    {
        if (isDead || isLevelWon) { return; }

        if (isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    void DoFalling()
    {
        if (isDead || isLevelWon) { return; }

        if (!isGrounded && rb.velocity.y < 0f)
        {
            rb.gravityScale = fallingGravity;
        }
        else
        {
            rb.gravityScale = risingGravity;
        }

        if (!isGrounded && rb.velocity.y < 0f && Input.GetKey(KeyCode.Z) && ((isTouchingWallR && Input.GetAxis("Horizontal") > 0f) || (isTouchingWallL && Input.GetAxis("Horizontal") < 0f)))
        {
            rb.velocity = new Vector2(0f, -wallSlideSpeed);
            isSliding = true;

            if (tempSlidingParticleSys == null)
            {
                if (isTouchingWallR)
                {
                    GameObject tempObj = Instantiate(wallSlideParticles, wallCheckObjectR);
                    tempSlidingParticleSys = tempObj.GetComponent<ParticleSystem>();
                }
                else if (isTouchingWallL)
                {
                    GameObject tempObj = Instantiate(wallSlideParticles, wallCheckObjectL);
                    tempSlidingParticleSys = tempObj.GetComponent<ParticleSystem>();
                }
                else { /* Nothing */ }
            }

            if (isBlastJumpCooldownActive)
            {
                isBlastJumpCooldownActive = false;
                if (burstReadyParticles != null && !isDead)
                {
                    burstReadyParticles.Play();
                }
            }
        }
        else
        {
            isSliding = false;
            if (tempSlidingParticleSys != null)
            {
                tempSlidingParticleSys.gameObject.transform.parent = null;
                tempSlidingParticleSys.Stop();
                tempSlidingParticleSys = null;
            }
        }
    }

    void DoBlastJump()
    {
        if (isDead || isLevelWon) { return; }

        if (!isBlastJumping && !isBlastJumpCooldownActive && Input.GetKeyDown(KeyCode.X))
        {
            isBlastJumping = true;
            StartCoroutine("DoBlastJumpCR");
        }
    }

    IEnumerator DoBlastJumpCR()
    {
        if (isDead || isLevelWon) { yield break; }

        rb.velocity = new Vector2(rb.velocity.x, windupJumpSpeed);
        if (preBlastTrail != null) { preBlastTrail.emitting = true; }
        yield return new WaitForSeconds(blastJumpWindupTime);
        if (preBlastTrail != null) { preBlastTrail.emitting = false; }

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

        GameObject postBurstObj = null;
        if (horizontalDirection != 0f || verticalDirection != 0f)
        {
            rb.velocity = new Vector2(horizontalDirection * blastJumpSpeed, verticalDirection * blastJumpSpeed);
            Instantiate(burstParticles, this.transform.position, Quaternion.identity);
            postBurstObj = Instantiate(postBurstParticles, this.transform);
            if (screenShakeObject != null)
            {
                screenShakeObject.DoShake(16f, 0.05f);
            }

            ++consecutiveBlastJumps;

            changeHealthBy((int)(-(baseBlastJumpHealthCost + (baseBlastJumpHealthCost * Mathf.Pow(2f, (float)(consecutiveBlastJumps - 1))))));
        }

        yield return new WaitForSeconds(blastJumpRecoveryTime);

        isBlastJumping = false;
        isBlastJumpCooldownActive = true;
        if (postBurstObj != null)
        {
            postBurstObj.transform.parent = null;
            postBurstObj.GetComponent<ParticleSystem>().Stop();
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
        if (isDead || isLevelWon) { return; }

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
            if (isSliding)
            {
                changeHealthBy(wallSlideHealthRegen);
            }
            else
            {
                changeHealthBy(healthRegenRate);
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
                if (screenShakeObject != null)
                {
                    screenShakeObject.DoShake(32f, 0.6f);
                }
                Instantiate(deathExplosionParticles, this.transform.position, Quaternion.identity).gameObject.transform.parent = null;
                currentHealth = 0;
                sprite.color = Color.clear;
                isDead = true;
                if (scorekeeperObject != null) { scorekeeperObject.StopScorekeeper(); }
            }
        }
    }

    void DoRespawn()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StopCoroutine("DoBlastJumpCR");

            if (screenShakeObject != null) { screenShakeObject.DoShake(0f, 0f); }

            if (scorekeeperObject != null) { scorekeeperObject.ResetScorekeeper(); }

            if (tempSlidingParticleSys != null)
            {
                tempSlidingParticleSys.gameObject.transform.parent = null;
                tempSlidingParticleSys.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                tempSlidingParticleSys = null;
            }

            sprite.color = Color.white;
            if (speedTrail != null) { speedTrail.emitting = false; }
            if (preBlastTrail != null) { preBlastTrail.emitting = false; }

            this.transform.position = respawnPosition;
            currentHealth = maximumHealth;
            rb.velocity = Vector2.zero;
            isDead = false;
            isGrounded = false;
            isSliding = false;
            isTouchingWallR = false;
            isTouchingWallL = false;
            isTouchingWall = false;
            tempSlidingParticleSys = null;
            isBlastJumping = false;
            isBlastJumpCooldownActive = false;
            consecutiveBlastJumps = 0;
            isLevelWon = false;
            isDead = false;
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
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Goal" && !isDead)
        {
            isLevelWon = true;
            if (scorekeeperObject != null)
            {
                scorekeeperObject.StopScorekeeper();
                scorekeeperObject.CalculateFinalScore(currentHealth, maximumHealth);
                Debug.Log($"TIME: {scorekeeperObject.getTimeScore()}, FUSE: {scorekeeperObject.getFuseScore()}, TOTAL: {scorekeeperObject.getTotalScore()}");
            }
        }
    }
}
