using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IAttacker
{
    public bool CanAttack(Transform target);

    public float GetDamage();

    public float GetAttackSpeed();

    public void Attacked();

    public void StopMoving();
}