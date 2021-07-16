using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IAttacker
{
    public bool CanAttack(Transform target);

    public float GetAttackSpeed();

    public float Attacked();

    public void StopMoving();
}