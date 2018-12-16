﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatJumping : StateBase<PlayerController>
{
    private bool hasJumped = false;

    public override void OnEnter(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", true);
        hasJumped = false;
        float absAngle = Mathf.Abs(player.CombatAngle);
        player.transform.rotation = absAngle > 45f && absAngle < 135f ?
            Quaternion.LookRotation(Vector3.Cross(player.transform.forward, Vector3.up))
            : Quaternion.LookRotation((absAngle <= 45f ? 1f : -1f) * 
            Vector3.Scale(new Vector3(1f, 0f, 1f), player.Velocity.normalized));

        player.Anim.SetFloat("AimAngle", 0f);
    }

    public override void OnExit(PlayerController player)
    {
        player.Anim.SetBool("isCombatJumping", false);
    }

    public override void Update(PlayerController player)
    {
        AnimatorStateInfo animState = player.Anim.GetCurrentAnimatorStateInfo(0);
        AnimatorTransitionInfo transInfo = player.Anim.GetAnimatorTransitionInfo(0);

        if (Combat.target != null)
        {
            player.Anim.SetFloat("AimAngle",
                Vector3.SignedAngle((Combat.target.position - player.transform.position).normalized,
                player.transform.forward, Vector3.up));
            Debug.Log("AimAngle: " + player.Anim.GetFloat("AimAngle"));
        }
        else
        {
            player.Anim.SetFloat("AimAngle",
                Vector3.SignedAngle(UMath.ZeroYInVector(player.Cam.forward).normalized,
                player.transform.forward, Vector3.up));
        }

        if (hasJumped)
        {
            if (player.Grounded && player.Velocity.y <= 0f)
                player.StateMachine.GoToState<Combat>();

            player.ApplyGravity(player.gravity);
        }
        else
        {
            if (transInfo.IsName("CombatCompress -> JumpR"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * 4f + Vector3.up * player.JumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpL"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.right * -4f + Vector3.up * player.JumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpB"))
            {
                player.ForceWaistRotation = false;
                player.Velocity = player.transform.forward * -4f + Vector3.up * player.JumpYVel;
                hasJumped = true;
            }
            else if (transInfo.IsName("CombatCompress -> JumpF"))
            {
                player.Velocity = player.transform.forward * 4f + Vector3.up * player.JumpYVel;
                hasJumped = true;
            }
            
        }
    }
}
