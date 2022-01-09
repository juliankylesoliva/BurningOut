using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStates
{
    private bool isDead;
    private bool isLevelWon;
    private bool isGrounded;
    private bool isFalling;
    private bool isSliding;
    private bool isTouchingWallR;
    private bool isTouchingWallL;
    private bool isTouchingWall;
    private bool isBlastJumping;
    private bool isBlastJumpRecoveryActive;
    private bool isBlastJumpCooldownActive;

    public PlayerStates(bool isDead, bool isLevelWon, bool isGrounded, bool isFalling, bool isSliding, bool isTouchingWallR, bool isTouchingWallL, bool isTouchingWall, bool isBlastJumping, bool isBlastJumpRecoveryActive, bool isBlastJumpCooldownActive)
    {
        this.isDead = isDead;
        this.isLevelWon = isLevelWon;
        this.isGrounded = isGrounded;
        this.isFalling = isFalling;
        this.isSliding = isSliding;
        this.isTouchingWallR = isTouchingWallR;
        this.isTouchingWallL = isTouchingWallL;
        this.isTouchingWall = isTouchingWall;
        this.isBlastJumping = isBlastJumping;
        this.isBlastJumpRecoveryActive = isBlastJumpRecoveryActive;
        this.isBlastJumpCooldownActive = isBlastJumpCooldownActive;
    }

    public bool getIsDead() { return isDead; }
    public bool getIsLevelWon() { return isLevelWon; }
    public bool getIsGrounded() { return isGrounded; }
    public bool getIsFalling() { return isFalling; }
    public bool getIsSliding() { return isSliding; }
    public bool getIsTouchingWallR() { return isTouchingWallR; }
    public bool getIsTouchingWallL() { return isTouchingWallL; }
    public bool getIsTouchingWall() { return isTouchingWall; }
    public bool getIsBlastJumping() { return isBlastJumping; }
    public bool getIsBlastJumpRecoveryActive() { return isBlastJumpRecoveryActive; }
    public bool getIsBlastJumpCooldownActive() { return isBlastJumpCooldownActive; }
}

public class PlayerParticles : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
