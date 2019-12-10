using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement),typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    #region 单例
    private static PlayerController m_instance;
    public static PlayerController Instance
    {
        get
        {
            return m_instance;
        }
    }
    #endregion

    #region private变量
    private PlayerMovement playerMovement;
    private Animator anim;

    private float moveJudge = 0.01f;
    private float directJudge = 0.001f;

    private AudioSource audioSource;
    #endregion

    #region public变量
    [HideInInspector]
    public bool isMovingRight = true;
    #endregion

    #region 方法

    #region Mono方法
    // Start is called before the first frame update
    void Awake()
    {
        if (m_instance != null)
            DestroyImmediate(this);
        else
            m_instance = this;

        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    #endregion

    #region 普通Public方法
    public void SetPlayerState(Vector3 velocity, CollisionState2D state)
    {
        if ((isMovingRight && velocity.x < -directJudge) || (!isMovingRight && velocity.x > directJudge))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            isMovingRight = !isMovingRight;
        }

        if (state.below && Mathf.Abs(velocity.x) > moveJudge)
        {
            if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Player_Run")
                anim.Play("Player_Run");
        }
        else if (!state.below && state.wasGroundLastFrame)
        {
            anim.Play("Player_JumpStart");
            //audioSource.Play();
        }
        else if (state.below && !state.wasGroundLastFrame)
            anim.Play("Player_JumpLand");
        else if (state.below && velocity.magnitude < moveJudge)
        {
            anim.Play("Player_Idle");
        }
    }
    
    public void TurnTheDirection()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
    #endregion

    #endregion
}
