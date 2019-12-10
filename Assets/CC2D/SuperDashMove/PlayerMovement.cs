using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : BoxObj
{
    #region 私有变量
    [SerializeField]
    private float runSpeed = 8f;
    [SerializeField]
    private float jumpHeight = 3f;
    [SerializeField]
    private float groundDamping = 20f;
    [SerializeField]
    private float airDamping = 5f;
    [SerializeField]
    private float jumpScope = 8f;

    private Vector3 sumOfOutsideForce = Vector3.zero;
    private int verticalMoveSign = 0;
    private bool isAttaching = false;
    private IInteractable interactableObj = null;
    private bool isInRight;
    private float yJudge = 0.02f;
    #endregion

    #region 重载/实现的方法
    protected override void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (!PlayerController.Instance.isMovingRight && isAttaching)
                EndAttach();
            verticalMoveSign = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (PlayerController.Instance.isMovingRight && isAttaching)
                EndAttach();
            verticalMoveSign = -1;
        }
        else
            verticalMoveSign = 0;

        if (Input.GetKeyDown(KeyCode.LeftControl) && interactableObj != null && isGrounded)
        {
            isAttaching = true;
        }
        else if ((Input.GetKeyUp(KeyCode.LeftControl)) && interactableObj != null && isGrounded)
        {
            isAttaching = false;
            interactableObj.EndInteraction();
        }

        GetSumOfOutsideForce();

        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && isGrounded && !isAttaching)
        {
            if (sumOfOutsideForce.y < jumpScope)
                targetVelocity.y = Mathf.Sqrt(2f * jumpHeight * gravityModifier);
        }

        base.Update();

        if (isAttaching && interactableObj != null && verticalMoveSign != 0 && isGrounded)
        {
            if ((verticalMoveSign == 1 && isInRight) || (verticalMoveSign == -1 && !isInRight))//推
            {
                interactableObj.Interaction(targetVelocity * Time.deltaTime);//箱子先走
                if(!interactableObj.IsConstant())//如果非持续可交互，交互一次便断开
                {
                    EndAttach();
                }
            }
        }
    }

    protected override void CalculateDamping()
    {
        var smoothedMovementFactor = isGrounded ? groundDamping : airDamping; 
        targetVelocity.x = Mathf.Lerp(targetVelocity.x, verticalMoveSign * runSpeed, Time.deltaTime * smoothedMovementFactor);
    }

    protected override void CalculateOutsideForce()
    {
        targetVelocity += sumOfOutsideForce;
    }

    public override void Move(Vector3 deltaMovement)
    {
        base.Move(deltaMovement);
        if (Mathf.Abs(velocity.y) > yJudge && interactFlag)
            EndAttach();
        if (!isGrounded) transform.parent = null;
        PlayerController.Instance.SetPlayerState(velocity, collisionState);
    }

    protected override void CheckObjBelow(RaycastHit2D raycast)
    {
        if (raycast.collider.GetComponent<MobileObject>())
        {
            /*
            if(!raycast.collider.GetComponent<Spring>())
            {
                transform.SetParent(raycast.collider.transform);
                return;
            }*/
        }
        transform.parent = null;
    }
    #endregion

    #region 新写的Mono方法
    private void OnTriggerEnter2D(Collider2D collider)//———TODO———这一步也要检测layer
    {
        IInteractable obj = collider.GetComponent<IInteractable>();
        if (obj == null) return;
        if (collider.GetComponent<MobileObject>().isSleep) return;
        if (interactableObj != null && interactableObj != obj && isAttaching)
            EndAttach();
        interactableObj = obj;
        isInRight = collider.transform.position.x > transform.position.x;
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (interactableObj != null && interactableObj == collider.GetComponent<IInteractable>())
        {
            if (isAttaching)
                EndAttach();
            else
            {
                isAttaching = false;
                interactableObj = null;
            }         
        }
    }
    #endregion

    #region 新写的普通方法
    private void EndAttach()
    {
        isAttaching = false;
        interactableObj.EndInteraction();
        interactableObj = null;
    }

    private void GetSumOfOutsideForce()
    {
        sumOfOutsideForce = Vector3.zero;
        foreach (Vector3 v in forceList)
            sumOfOutsideForce += v;
        forceList.Clear();
    }
    #endregion
}
