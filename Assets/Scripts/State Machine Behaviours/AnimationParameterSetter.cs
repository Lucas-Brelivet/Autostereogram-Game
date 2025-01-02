using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenFrontDoorBehavior : StateMachineBehaviour
{
    public enum StateMessage
    {
        ENTER,
        UPDATE,
        EXIT,
        Move,
        IK
    };


    [SerializeField] private int parameterIndex;
    [SerializeField] private StateMessage changeParameterOn;
    [Tooltip("Value to be assigned to the given parameter when the given message is called.\nFor bool parameters, 0 is false, anything else is true. ignored for triggers")]
    [SerializeField] private float newValue;


    private void ChangeParameterIfInCorrectMessage(StateMessage currentMessage, Animator animator)
    {
        if(changeParameterOn == currentMessage)
        {
            AnimatorControllerParameter parameter = animator.GetParameter(parameterIndex);
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float :
                    animator.SetFloat(parameterIndex, newValue);
                    break;
                case AnimatorControllerParameterType.Int :
                    animator.SetInteger(parameterIndex, (int) newValue);
                    break;
                case AnimatorControllerParameterType.Bool :
                    animator.SetBool(parameterIndex, newValue != 0);
                    break;
                case AnimatorControllerParameterType.Trigger :
                    animator.SetTrigger(parameterIndex);
                    break;
            }
        }
    }


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ChangeParameterIfInCorrectMessage(StateMessage.ENTER, animator);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ChangeParameterIfInCorrectMessage(StateMessage.UPDATE, animator);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ChangeParameterIfInCorrectMessage(StateMessage.EXIT, animator);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ChangeParameterIfInCorrectMessage(StateMessage.Move, animator);
    }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ChangeParameterIfInCorrectMessage(StateMessage.IK, animator);
    }
}
