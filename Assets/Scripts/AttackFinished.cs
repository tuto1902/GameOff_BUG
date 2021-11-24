using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackFinished : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       animator.SetBool("isAttacking", false);
    }
}
