using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{

    public BaseHero hero;
    private BattleStateMachine BSM;

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;
    private float cur_cooldown = 0f;
    private float max_cooldown = 5f;
    private Image ProgressBar;
    public GameObject Selector;

    //Ienumerator
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;
    //Dead
    private bool alive = true;
    //heroPanel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;

    void Start()
    {
        //find spacer object make connection
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroBarSpacer");
        //create panel fill-in info
        CreateHeroPanel();
        startPosition = transform.position;
        cur_cooldown = Random.Range(0, 2.5f);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        currentState = TurnState.PROCESSING;
        Selector.SetActive(false);
    }

    void Update()
    {
        //Debug.Log(currentState);
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
                break;

            case (TurnState.ADDTOLIST):
                BSM.HerosToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;

            case (TurnState.WAITING):
                //idle
                break;

            case (TurnState.SELECTING):
                break;

            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;

            case (TurnState.DEAD):
                if (!alive)
                    return;
                else
                {
                    //change tag
                    this.gameObject.tag = "DeadHero";
                    //not attackable by enemy
                    BSM.HerosInBattle.Remove(this.gameObject);
                    //not manageble
                    BSM.HerosToManage.Remove(this.gameObject);
                    //deactivate the selector
                    Selector.SetActive(false);
                    //reset gui
                    BSM.AttackPanel.SetActive(false);
                    BSM.EnemySelectPanel.SetActive(false);
                    //remove item from performlist
                    if (BSM.HerosInBattle.Count > 0)
                    {
                        for (int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if (i != 0)
                            {
                                if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                                {
                                    BSM.PerformList.Remove(BSM.PerformList[i]);
                                }
                                if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                                {
                                    BSM.PerformList[i].AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];
                                }
                            }
                        }
                    }
                    //change color
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105,105,105,255);
                    //reset heroinput
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                    alive = false;

                }
                break;


        }
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        float calc_cooldown = cur_cooldown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if(cur_cooldown>=max_cooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }


    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }
        actionStarted = true;

        //animate the character
        Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x - 1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
        while (MoveTowardsEnemy(enemyPosition))
        {
            yield return null;
        }
        //wait a bit
        yield return new WaitForSeconds(0.5f);
        //do dmg
        DoDamage();
        //animate back to start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }
        BSM.PerformList.RemoveAt(0);
        if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        {
            BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
            //reset this enemy state
            cur_cooldown = 0f;
            currentState = TurnState.PROCESSING;
        }
        else
        {
            currentState = TurnState.WAITING;
        }
        actionStarted = false;

    }
    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));

    }
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));

    }
    public void TakeDamage(float damageToBeTaken)
    {
        hero.curHP -= damageToBeTaken;
        if(hero.curHP <= 0)
        {
            hero.curHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }

    public void DoDamage()
    {
        float calc_damage = hero.curATK + BSM.PerformList[0].choosenAttack.attackDamage;
        EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
    }

    void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.theName;
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;

        ProgressBar = stats.ProgressBar;
        HeroPanel.transform.SetParent(HeroPanelSpacer, false);
        //Vector3 newPosition = new Vector3(0, 0, 0);
        //newPosition = HeroPanelSpacer.transform.position;
        //newPosition.y -= (GameObject.Find("BattleCanvas").transform.Find("HeroPanel").Height / 2); 
        HeroPanel.transform.position = HeroPanelSpacer.transform.position;
    }
    void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;
    }
}
