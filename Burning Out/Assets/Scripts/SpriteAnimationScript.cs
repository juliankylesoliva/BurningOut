using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationScript : MonoBehaviour
{
    SpriteRenderer playerSprite;
    Animator anim;
    PlayerControls playerCtrl;

    private PlayerStates states;

    void Awake()
    {
        playerSprite = this.gameObject.GetComponent<SpriteRenderer>();
        anim = this.gameObject.GetComponent<Animator>();
        playerCtrl = this.gameObject.GetComponent<PlayerControls>();
    }

    // Update is called once per frame
    void Update()
    {
        states = playerCtrl.getPlayerStatesObject();

        playerSprite.flipX = !states.getIsFacingRight();

        if (states.getIsMoving() && states.getIsGrounded())
        {
            if (states.getIsAboveSpeedLimit())
            {
                anim.Play("PlayerMoveFuse");
            }
            else
            {
                anim.Play("PlayerMove");
            }
        }
        else if (!states.getIsGrounded())
        {
            if (states.getIsFalling())
            {
                if (states.getIsSliding())
                {
                    anim.Play("PlayerWallSlide");
                }
                else
                {
                    if (states.getIsAboveSpeedLimit())
                    {
                        anim.Play("PlayerFallingFuse");
                    }
                    else
                    {
                        anim.Play("PlayerFalling");
                    }
                }
            }
            else
            {
                if (states.getIsBlastJumping() && !states.getIsBlastJumpRecoveryActive())
                {
                    anim.Play("PlayerPreBlastJump");
                }
                else
                {
                    if (states.getIsAboveSpeedLimit())
                    {
                        anim.Play("PlayerBlastJump");
                    }
                    else
                    {
                        anim.Play("PlayerJump");
                    }
                }
            }
        }
        else
        {
            anim.Play("PlayerIdle");
        }
    }
}
