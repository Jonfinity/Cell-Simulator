using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCloneConsumeDetection : MonoBehaviour
{
    public static PlayerCloneConsumeDetection instance;

    [SerializeField] private PlayerClone playerClone;

    [SerializeField] public CircleCollider2D circle;

    private void Awake()
    {
        instance = this;
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if(!playerClone.fakeParent.spawned)
        {
            return;
        }

        float mass = playerClone.rigidBody.mass;
        bool b = Utils.CanEat(other.transform, playerClone.transform);
        switch(other.tag)
        {
            case "Player":
                PlayerBlob pbc = other.GetComponent<PlayerBlob>();

                if(pbc.GetInstanceID() == playerClone.fakeParent.GetInstanceID())
                {
                    if(playerClone.recombine)
                    {
                        playerClone.kill(true);
                    }

                    return;
                }

                if(!b)
                {
                    return;
                }
                
                pbc.grow(mass);
                break;
            case "PlayerClone":
                PlayerClone pcbc = other.GetComponent<PlayerClone>();

                if(pcbc.fakeParent.GetInstanceID() == playerClone.fakeParent.GetInstanceID())
                {
                    if(playerClone.recombine)
                    {
                        //pcbc.grow(mass, false);
                        //playerClone.kill();
                        //TODO
                        //playerClone.fakeParent.HandleClones(pcbc, playerClone);
                    }

                    return;
                }

                if(!b)
                {
                    return;
                }

                pcbc.grow(mass);
                break;
            case "AI":
                if(!b)
                {
                    return;
                }

                other.GetComponent<AIBlob>().grow(mass);
                break;
            case "AIClone":
                if(!b)
                {
                    return;
                }

                other.GetComponent<AIClone>().grow(mass);
                break;
            default:
                return;
        }

        playerClone.kill();
    }*/
}
