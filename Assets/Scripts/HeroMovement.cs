using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    float moveSpeed = 10f;

    Vector3 curPos, lastPos;

    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.instance.nextSpawnPoint != "")
        {
            GameObject spawnPoint = GameObject.Find(GameManager.instance.nextSpawnPoint);
            transform.position = spawnPoint.transform.position;

            GameManager.instance.nextSpawnPoint = "";
        }
        else if(GameManager.instance.lastHeroPosition != Vector3.zero)
        {
            transform.position = GameManager.instance.lastHeroPosition;
            GameManager.instance.lastHeroPosition = Vector3.zero;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0.0f,moveZ);

        GetComponent<Rigidbody>().velocity = movement * moveSpeed; //* Time.deltaTime;
        curPos = transform.position;
        if(curPos == lastPos)
        {
            GameManager.instance.isWalking = false;
        }
        else
        {
            GameManager.instance.isWalking = true;
        }
        lastPos = curPos;
    }

    void OnTriggerEnter(Collider coli)
    {
       if(coli.tag == "Teleporter")
        {
            CollisionHandler col = coli.gameObject.GetComponent<CollisionHandler>();
            GameManager.instance.nextSpawnPoint = col.spawnPointName;
            GameManager.instance.sceneToLoad = col.sceneToLoad;
            GameManager.instance.LoadNextScene();
        }
        
        if(coli.tag == "EncounterZone")
        {
            RegionData region = coli.gameObject.GetComponent<RegionData>();
            GameManager.instance.curRegion = region;
        }
    }

    void OnTriggerStay(Collider coli)
    {
        if (coli.tag == "EncounterZone")
        {
            GameManager.instance.canGetEncounter = true;
        }
    }

    void OnTriggerExit(Collider coli)
    {
        if (coli.tag == "EncounterZone")
        {
            GameManager.instance.canGetEncounter = false;
        }
    }

}
