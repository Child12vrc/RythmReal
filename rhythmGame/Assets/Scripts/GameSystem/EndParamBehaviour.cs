using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndParamBehaviour : StateMachineBehaviour  //스테이트 머신 Bechaviouer 
{
    public string parameter = "IsAttacking";                    //애니메이서에서 저장한 파라미터 값을 설정
    public bool IsTrue = false;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //애니메이션이 종료되고 전환되는 시점에서 선언한 애니메이션 파라미터 값은 ture -> false 시킨다. 
        animator.SetBool(parameter, IsTrue);
    }
}
