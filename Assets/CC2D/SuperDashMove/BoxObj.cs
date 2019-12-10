using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxObj : MobileObject
{
    #region 字段与属性

    #region 内部类/结构体
    private struct BoxRaycastOrigins
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
    }
    #endregion

    #region protected变量
    [SerializeField]
    protected int totalHorizontalRays = 8;

    [SerializeField]
    protected int totalVerticalRays = 8;

    [SerializeField]
    protected float skinWidth = 0.02f;
    #endregion

    #region private变量
    const float kSkinWidthFloatFudgeFactor = 0.001f;
    private float _verticalDistanceBetweenRays;
    private float _horizontalDistanceBetweenRays;
    private BoxCollider2D boxCollider;
    private BoxRaycastOrigins _raycastOrigins;
    private RaycastHit2D _raycastHit;
    #endregion

    #region public变量
    public bool isGrounded { get { return collisionState.below; } }
    #endregion

    #endregion

    #region 方法

    #region 重载方法
    protected override void Awake()
    {
        base.Awake();
        boxCollider = GetComponent<BoxCollider2D>();
        CalculateDistanceBetweenRays();
    }

    protected override void MoveHorizontally(ref Vector3 deltaMovement)
    {
        bool isGoingRight = deltaMovement.x > 0;
        float rayDistance = Mathf.Abs(deltaMovement.x) + skinWidth;
        Vector2 rayDirection = isGoingRight ? Vector2.right : Vector2.left;
        Vector2 initialRayOrigin = isGoingRight ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
        for (int i = 0; i < totalHorizontalRays; i++)
        {
            //射线起点
            Vector2 ray = new Vector2(initialRayOrigin.x, initialRayOrigin.y + i * _verticalDistanceBetweenRays);
            Debug.DrawRay(ray, rayDirection * rayDistance, Color.red);

            _raycastHit = IsCurrentObj(ray, rayDirection, rayDistance, platformMask);
            if (_raycastHit)
            {
                deltaMovement.x = _raycastHit.point.x - ray.x;

                if (isGoingRight)
                {
                    deltaMovement.x -= skinWidth;
                    collisionState.right = true;
                }
                else
                {
                    deltaMovement.x += skinWidth;
                    collisionState.left = true;
                }

                //如果这条射线已经检测到皮肤，之后的射线不用检测了
                if (Mathf.Abs(deltaMovement.x) < skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    protected override void MoveVertically(ref Vector3 deltaMovement)
    {
        bool isGoingUp = deltaMovement.y > 0;
        float rayDistance = Mathf.Abs(deltaMovement.y) + skinWidth;
        Vector2 rayDirection = isGoingUp ? Vector2.up : Vector2.down;
        Vector2 initialRayOrigin = isGoingUp ? _raycastOrigins.topLeft : _raycastOrigins.bottomLeft;

        //水平方向已经进行过处理，这里用到新的处理结果
        initialRayOrigin.x += deltaMovement.x;

        for (int i = 0; i < totalVerticalRays; i++)
        {
            //射线起点
            var ray = new Vector2(initialRayOrigin.x + i * _horizontalDistanceBetweenRays, initialRayOrigin.y);
            Debug.DrawRay(ray, rayDirection * rayDistance, Color.red);

            _raycastHit = IsCurrentObj(ray, rayDirection, rayDistance, platformMask);
            if (_raycastHit)
            {
                deltaMovement.y = _raycastHit.point.y - ray.y;
                rayDistance = Mathf.Abs(deltaMovement.y);

                if (isGoingUp)
                {
                    deltaMovement.y -= skinWidth;
                    collisionState.above = true;
                }
                else
                {
                    deltaMovement.y += skinWidth;
                    collisionState.below = true;
                    CheckObjBelow(_raycastHit);
                }

                if (rayDistance < skinWidth + kSkinWidthFloatFudgeFactor)
                    break;
            }
        }
    }

    public override void Move(Vector3 deltaMovement)
    {
        collisionState.wasGroundLastFrame = collisionState.below;
        collisionState.Reset();

        PrimeRaycastOrigins();

        if (deltaMovement.x != 0f)
            MoveHorizontally(ref deltaMovement);
        if (deltaMovement.y != 0f)
            MoveVertically(ref deltaMovement);

        deltaMovement.z = 0;
        transform.Translate(deltaMovement, Space.World);

        //计算实际速度
        velocity = deltaMovement / Time.deltaTime;
        collisionState.becameGroundedThisFrame = (!collisionState.wasGroundLastFrame && collisionState.below);
    }
    #endregion

    #region 新写方法
    private void CalculateDistanceBetweenRays()
    {
        var colliderHeight = boxCollider.size.y * Mathf.Abs(transform.localScale.y) - (skinWidth);
        var colliderWidth = boxCollider.size.x * Mathf.Abs(transform.localScale.x) - (skinWidth);
        _verticalDistanceBetweenRays = colliderHeight / (totalHorizontalRays - 1);
        _horizontalDistanceBetweenRays = colliderWidth / (totalVerticalRays - 1);
    }

    private void PrimeRaycastOrigins()
    {
        var modifiedBounds = boxCollider.bounds;
        modifiedBounds.Expand(-skinWidth);

        _raycastOrigins.topLeft = new Vector2(modifiedBounds.min.x, modifiedBounds.max.y);
        _raycastOrigins.topRight = modifiedBounds.max;
        _raycastOrigins.bottomLeft = modifiedBounds.min;
        _raycastOrigins.bottomRight = new Vector2(modifiedBounds.max.x, modifiedBounds.min.y);
    }

    private RaycastHit2D IsCurrentObj(Vector2 ray, Vector3 direction, float distance, LayerMask mask)
    {
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray, direction, distance, mask);
        foreach(RaycastHit2D raycastHit in raycastHits)
        {
            if (raycastHit.collider.gameObject != gameObject)
            {
                return raycastHit;
            }
        }

        if (raycastHits.Length > 2)
            Debug.LogWarning("射线检测到两个以上物体！");        

        return new RaycastHit2D();
    }

    protected virtual void CheckObjBelow(RaycastHit2D raycast)
    {

    }
    #endregion

    #endregion
}
