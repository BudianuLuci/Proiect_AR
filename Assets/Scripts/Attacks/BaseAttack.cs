using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BaseAttack : MonoBehaviour
{
    public string attackName;
    public string attackDescription;
    public float attackDamage;//Base damage 15, mellee lvl 10 stamina 35 = basedmg + stamina + lvl = 60
    public float attackCost;//ManaCost
}
