using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseHero: BaseClass
{
    // Start is called before the first frame update
    
    public int stamina;
    public int intellect;
    public int dexterity;
    public int agility;
    public List<BaseAttack> MagicAttacks = new List<BaseAttack>();
}
