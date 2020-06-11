using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public RegionData curRegion;

    public string nextSpawnPoint;

    public GameObject heroCharacter;

    public Vector3 nextHeroPosition;
    public Vector3 lastHeroPosition;

    public string sceneToLoad;
    public string lastScene;

    public bool isWalking = false;
    public bool canGetEncounter = false;
    public bool gotAttacked = false;

    public enum GameStates
    {
        WORLD_STATE,
        CITY_STATE,
        BATTLE_STATE,
        IDLE
    }

    public int enemyAmount;
    public List<GameObject> enemysToBattle = new List<GameObject>();

    public GameStates gameState;

    void Awake()
    {
        // check if instance exists
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        if (!GameObject.Find("HeroCharcter"))
        {
            GameObject Hero = Instantiate(heroCharacter, nextHeroPosition, Quaternion.identity) as GameObject;
            Hero.name = "HeroCharacter";
        }
    }

    void Update()
    {
        switch (gameState)
        {
            case (GameStates.WORLD_STATE):
                if (isWalking)
                {
                    RandomEncounter();
                }
                if (gotAttacked)
                {
                    gameState = GameStates.BATTLE_STATE;
                }
                break;
            case (GameStates.CITY_STATE):
                break;
            case (GameStates.BATTLE_STATE):
                StartBattle();
                gameState = GameStates.IDLE;
                break;
            case (GameStates.IDLE):
                break;

        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
    public void LoadSceneAfterBattle()
    {
        SceneManager.LoadScene(lastScene);
    }

    void RandomEncounter()
    {
        if(isWalking && canGetEncounter)
        {
            if (Random.Range(0, 1000) < 10)
            {
                //Debug.Log("I got attacked");
                gotAttacked = true;
            }
        }
    }

    void StartBattle()
    {
        enemyAmount = Random.Range(1, curRegion.maxAmountEnemys + 1);
        
        for (int i = 0; i < enemyAmount; i++)
        {
            enemysToBattle.Add(curRegion.possibleEnemys[Random.Range(0, curRegion.possibleEnemys.Count)]);
        }

        lastHeroPosition = GameObject.Find("HeroCharacter").gameObject.transform.position;
        nextHeroPosition = lastHeroPosition;
        lastScene = SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(curRegion.BattleScene);
        isWalking = false;
        gotAttacked = false;
        canGetEncounter = false;

    }
}
