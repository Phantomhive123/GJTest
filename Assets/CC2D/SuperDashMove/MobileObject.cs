using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#region 外部类和接口
public interface IInteractable
{
    void Interaction(Vector3 deltaMovement);
    void EndInteraction();
    bool IsConstant();
}

public class CollisionState2D
{
    public bool right;
    public bool left;
    public bool above;
    public bool below;
    public bool becameGroundedThisFrame;
    public bool wasGroundLastFrame;

    public bool HasCollision()
    {
        return below || right || left || above;
    }

    public void Reset()
    {
        right = left = above = below = false;
    }
}
#endregion

public abstract class MobileObject : MonoBehaviour
{
    #region 字段与属性

    #region 序列化非public变量
    [SerializeField]
    protected float gravityModifier = 20;   
    #endregion

    #region 共有字段
    public bool interactFlag = false;
    public bool isSleep = false;
    public LayerMask platformMask = 0;
    public LayerMask triggerMask = 0;
    public Vector3 targetVelocity;
    public Vector3 velocity;
    public float dampingModifier = 0.002f;
    #endregion

    #region protected字段
    protected new Transform transform;
    protected CollisionState2D collisionState = new CollisionState2D();
    protected List<Vector3> forceList;
    #endregion

    #endregion

    #region 方法

    #region Mono方法
    protected virtual void Awake()
    {
        transform = GetComponent<Transform>();
        SaveState();
        forceList = new List<Vector3>();
        //—————————这里删掉了层级的ignore———————————
    }

    protected virtual void Update()
    {
        if (isSleep)
        {
            forceList.Clear();
            return;
        }
        //继承上一帧的速度
        targetVelocity += velocity;
        CalculateGravity();
        CalculateOutsideForce();
        CalculateDamping();
        CancelShake();
    }

    protected virtual void LateUpdate()
    {
        if (isSleep) return;
        Move(targetVelocity * Time.deltaTime);
        targetVelocity = Vector3.zero;
    }
    #endregion

    #region Move方法
    public virtual void Move(Vector3 deltaMovement)
    {
        deltaMovement = MaxMoveableDis(deltaMovement);
        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);
        velocity = deltaMovement / Time.deltaTime;
    }

    protected virtual void MoveHorizontally(ref Vector3 deltaMovement)
    {

    }

    protected virtual void MoveVertically(ref Vector3 deltaMovement)
    {

    }

    public virtual Vector3 MaxMoveableDis(Vector3 deltaMovement)
    {
        MoveHorizontally(ref deltaMovement);
        MoveVertically(ref deltaMovement);
        return deltaMovement;
    }
    #endregion

    #region 计算力的作用
    protected void CalculateGravity()
    {
        //这里可以后续更改重力方向
        targetVelocity += gravityModifier * Vector3.down * Time.deltaTime;
    }

    protected virtual void CalculateDamping()
    {
        float vScalar = targetVelocity.magnitude;
        Vector3 f = dampingModifier * vScalar * targetVelocity;
        float fScalar = f.magnitude;

        if (vScalar > fScalar)
            targetVelocity -= f;
        else
            targetVelocity = Vector3.zero;
    }

    protected virtual void CancelShake()
    {

    }
    
    protected virtual void CalculateOutsideForce()
    {
        foreach (Vector3 v in forceList)
            targetVelocity += v;
        forceList.Clear();
    }

    public void AddForce(Vector3 force)
    {
        forceList.Add(force);
    }
    
    #endregion

    #region 存储状态与状态重置
    private ObjState initState;

    [Serializable]
    private struct ObjState
    {
        public LayerMask platformMask;
        public LayerMask triggerMask;
        public bool interactFlag;
        public bool isSleep;
        public Vector3 pos;

        public int currentLayer;
        public SpriteMaskInteraction maskInteraction;
    }

    public virtual void ResetState()
    {
        transform.position = initState.pos;
        platformMask = initState.platformMask;
        triggerMask = initState.triggerMask;
        interactFlag = initState.interactFlag;
        isSleep = initState.isSleep;
        velocity = Vector3.zero;
        targetVelocity = Vector3.zero;
        gameObject.layer = initState.currentLayer;
        GetComponent<SpriteRenderer>().maskInteraction = initState.maskInteraction;
        collisionState.Reset();
    }

    public void SaveState()
    {
        initState = new ObjState();
        initState.platformMask = platformMask;
        initState.triggerMask = triggerMask;
        initState.interactFlag = interactFlag;
        initState.isSleep = isSleep;
        initState.pos = transform.position;
        initState.currentLayer = gameObject.layer;
        initState.maskInteraction = GetComponent<SpriteRenderer>().maskInteraction;
    }
    #endregion

    #endregion
}
