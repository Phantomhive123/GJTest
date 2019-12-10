using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    private Rigidbody2D rigid;
    private float h = 0f;
    private float v = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        Vector3 deltaMovement = new Vector3(h, v, 0);
        rigid.MovePosition(transform.position + deltaMovement * Time.fixedDeltaTime);
        Debug.Log("1:"+transform.position);
        Debug.Log("2:" + rigid.velocity);
        Debug.Log("3:" + deltaMovement);
    }
}
