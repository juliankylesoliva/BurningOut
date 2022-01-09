using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates
{
    private bool isFacingRight;
    private bool isDead;
    private bool isLevelWon;
    private bool isGrounded;
    private bool isMoving;
    private bool isFalling;
    private bool isSliding;
    private bool isAboveSpeedLimit;
    private bool isTouchingWallR;
    private bool isTouchingWallL;
    private bool isTouchingWall;
    private bool isBlastJumping;
    private bool isBlastJumpRecoveryActive;
    private bool isBlastJumpCooldownActive;

    public PlayerStates(bool isFacingRight, bool isDead, bool isLevelWon, bool isGrounded, bool isMoving, bool isFalling, bool isSliding, bool isAboveSpeedLimit, bool isTouchingWallR, bool isTouchingWallL, bool isTouchingWall, bool isBlastJumping, bool isBlastJumpRecoveryActive, bool isBlastJumpCooldownActive)
    {
        this.isFacingRight = isFacingRight;
        this.isDead = isDead;
        this.isLevelWon = isLevelWon;
        this.isGrounded = isGrounded;
        this.isMoving = isMoving;
        this.isFalling = isFalling;
        this.isSliding = isSliding;
        this.isAboveSpeedLimit = isAboveSpeedLimit;
        this.isTouchingWallR = isTouchingWallR;
        this.isTouchingWallL = isTouchingWallL;
        this.isTouchingWall = isTouchingWall;
        this.isBlastJumping = isBlastJumping;
        this.isBlastJumpRecoveryActive = isBlastJumpRecoveryActive;
        this.isBlastJumpCooldownActive = isBlastJumpCooldownActive;
    }

    public bool getIsFacingRight() { return isFacingRight; }
    public bool getIsDead() { return isDead; }
    public bool getIsLevelWon() { return isLevelWon; }
    public bool getIsGrounded() { return isGrounded; }
    public bool getIsMoving() { return isMoving; }
    public bool getIsFalling() { return isFalling; }
    public bool getIsSliding() { return isSliding; }
    public bool getIsAboveSpeedLimit() { return isAboveSpeedLimit; }
    public bool getIsTouchingWallR() { return isTouchingWallR; }
    public bool getIsTouchingWallL() { return isTouchingWallL; }
    public bool getIsTouchingWall() { return isTouchingWall; }
    public bool getIsBlastJumping() { return isBlastJumping; }
    public bool getIsBlastJumpRecoveryActive() { return isBlastJumpRecoveryActive; }
    public bool getIsBlastJumpCooldownActive() { return isBlastJumpCooldownActive; }
}

public class PlayerParticles : MonoBehaviour
{
    PlayerControls player;

    [SerializeField] Transform playerTransformCenter;
    [SerializeField] Transform playerTransformLeft;
    [SerializeField] Transform playerTransformRight;

    [SerializeField] GameObject blastJumpExplosionPrefab;
    [SerializeField] GameObject blastJumpFirePrefab;
    [SerializeField] GameObject blastJumpReadyPrefab;
    [SerializeField] GameObject blastJumpCooldownPrefab;
    [SerializeField] GameObject deathExplosionPrefab;
    [SerializeField] GameObject wallSlidePrefab;
    [SerializeField] GameObject speedTrailPrefab;
    [SerializeField] GameObject blastJumpWindupTrailPrefab;

    private PlayerStates states;
    private bool previousBlastJumpRecoveryState = false;
    private bool previousDeathState = false;

    private ParticleSystem blastJumpFire;
    private ParticleSystem blastJumpReady;
    private ParticleSystem blastJumpCooldown;
    private ParticleSystem wallSlide;
    private TrailRenderer speedTrail;
    private TrailRenderer windupTrail;

    void Awake()
    {
        player = this.gameObject.GetComponent<PlayerControls>();
        blastJumpReady = Instantiate(blastJumpReadyPrefab, playerTransformCenter).GetComponent<ParticleSystem>();
        speedTrail = Instantiate(speedTrailPrefab, playerTransformCenter).GetComponent<TrailRenderer>();
        windupTrail = Instantiate(blastJumpWindupTrailPrefab, playerTransformCenter).GetComponent<TrailRenderer>();
    }

    void Update()
    {
        states = player.getPlayerStatesObject();

        if (!previousBlastJumpRecoveryState && states.getIsBlastJumpRecoveryActive())
        {
            GameObject tempObj = Instantiate(blastJumpExplosionPrefab, playerTransformCenter.position, Quaternion.identity);
        }

        if (!previousDeathState && states.getIsDead())
        {
            GameObject tempObj = Instantiate(deathExplosionPrefab, playerTransformCenter.position, Quaternion.identity);
        }

        if (states.getIsSliding())
        {
            if ((wallSlide == null || !wallSlide.isPlaying) && states.getIsTouchingWallR())
            {
                wallSlide = Instantiate(wallSlidePrefab, playerTransformRight).GetComponent<ParticleSystem>();
            }
            else if ((wallSlide == null || !wallSlide.isPlaying) && states.getIsTouchingWallL())
            {
                wallSlide = Instantiate(wallSlidePrefab, playerTransformLeft).GetComponent<ParticleSystem>();
            }
            else { /* Nothing */ }
        }
        else
        {
            if (wallSlide != null)
            {
                wallSlide.Stop();
                wallSlide = null;
            }
        }

        if (states.getIsBlastJumpRecoveryActive())
        {
            if (blastJumpFire == null || !blastJumpFire.isPlaying)
            {
                blastJumpFire = Instantiate(blastJumpFirePrefab, playerTransformCenter).GetComponent<ParticleSystem>();
            }
        }
        else
        {
            if (blastJumpFire != null)
            {
                blastJumpFire.Stop();
                blastJumpFire = null;
            }
        }

        if (states.getIsBlastJumpCooldownActive())
        {
            if (blastJumpCooldown == null || !blastJumpCooldown.isPlaying)
            {
                blastJumpCooldown = Instantiate(blastJumpCooldownPrefab, playerTransformCenter).GetComponent<ParticleSystem>();
            }
        }
        else
        {
            if (blastJumpCooldown != null)
            {
                blastJumpCooldown.Stop();
                blastJumpCooldown = null;
            }
        }

        speedTrail.emitting = states.getIsAboveSpeedLimit();

        windupTrail.emitting = (states.getIsBlastJumping() && !states.getIsBlastJumpRecoveryActive());

        previousDeathState = states.getIsDead();
        previousBlastJumpRecoveryState = states.getIsBlastJumpRecoveryActive();
    }

    public void PlayBlastJumpRefresh()
    {
        blastJumpReady.Play();
    }
}
