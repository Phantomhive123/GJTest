using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim
{
    private Animator anim;
    private string currentName = null;

   public PlayerAnim(Animator animator)
   {
        anim = animator;
   }

    public void PlayAnim(string name)
    {
        if(currentName == null || currentName != name)
            anim.Play(Animator.StringToHash(name));
    }

}
