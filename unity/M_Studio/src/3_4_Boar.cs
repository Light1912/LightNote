using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boar : Enemy
{
    // public override void Move()
    // {
    //     base.Move();
    //     anim.SetBool("walk", true);
    // }
    protected override void Awake()
    {
        base.Awake();
        patrolState = new BoarPatrolState();
    }
}
