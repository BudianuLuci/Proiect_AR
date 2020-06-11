
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy Enemy;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;
    private float cur_cooldown;
    private float max_cooldown = 5f;
    private Vector3 startposition;
    //timeforaction stuff
    public GameObject Selector;
    private bool actionStarted = false;
    public GameObject HeroToAttack;
    private float animSpeed = 10f;

    //alive
    private bool alive = true;

    void Start()
    {
        Selector.SetActive(false);
        cur_cooldown = Random.Range(0, 2.5f);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startposition = transform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
                break;

            case (TurnState.CHOOSEACTION):
                ChooseAction();
                currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                break;

          

            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;

            case (TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    //change tag of enemy
                    this.gameObject.tag = "DeadEnemy";
                    //not attackable by heroes
                    BSM.EnemysInBattle.Remove(this.gameObject);
                    //disable select
                    Selector.SetActive(false);
                    if (BSM.EnemysInBattle.Count > 0) 
                    { 
                        for(int i=0; i<BSM.PerformList.Count; i++)
                        {
                            if (i != 0)
                            {
                                if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                                {
                                    BSM.PerformList.Remove(BSM.PerformList[i]);
                                }
                                if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                                {
                                    BSM.PerformList[i].AttackersTarget = BSM.EnemysInBattle[Random.Range(0, BSM.EnemysInBattle.Count)];
                                }
                            }
                        }
                    }
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);
                    alive = false;
                    BSM.EnemyButtons();
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                }
                break;

        }
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        //float calc_cooldown = cur_cooldown / max_cooldown;
        //ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if (cur_cooldown >= max_cooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }


    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = Enemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];
        int num = Random.Range(0, Enemy.attacks.Count);
        myAttack.choosenAttack = Enemy.attacks[num];
        Debug.Log(this.gameObject.name + " has choosen " + myAttack.choosenAttack.attackName + " and does " + myAttack.choosenAttack.attackDamage);
        
        BSM.CollectActions(myAttack);
    }
    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
            yield break;
        }
        actionStarted = true;

        //animate the character
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x+1.5f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while (MoveTowardsEnemy(heroPosition))
        {
            yield return null;
        }
        //wait a bit
        yield return new WaitForSeconds(0.5f);
        //do dmg
        DoDamage();
        //animate back to start position
        Vector3 firstPosition = startposition;
        while (MoveTowardsStart(startposition))
        {
            yield return null;
        }
        BSM.PerformList.RemoveAt(0);
        BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
        actionStarted = false;
        //reset this enemy state
        cur_cooldown = 0;
        currentState = TurnState.PROCESSING;

    }
    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position,target,animSpeed*Time.deltaTime));

    }
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));

    }
    public void TakeDamage(float damageToBeTaken)
    {
        Enemy.curHP -= damageToBeTaken;
        if (Enemy.curHP <= 0)
        {
            Enemy.curHP = 0;
            currentState = TurnState.DEAD;
        }
    }
    void DoDamage()
    {
        float calc_damage = Enemy.curATK + BSM.PerformList[0].choosenAttack.attackDamage;
        HeroToAttack.GetComponent<HeroStateMachine>().TakeDamage(calc_damage);
    }
}
