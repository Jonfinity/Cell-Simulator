using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConsumeDetection : MonoBehaviour
{
    public static PlayerConsumeDetection instance;

    [SerializeField] private PlayerBlob playerBlob;

    [SerializeField] public CircleCollider2D circle;

    private void Awake()
    {
        instance = this;
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if(!playerBlob.spawned)
        {
            return;
        }

        float mass = playerBlob.rigidBody.mass;
        bool b = Utils.CanEat(other.transform, playerBlob.transform);
        switch(other.tag)
        {
            case "Player":
                if(!b)
                {
                    return;
                }

                other.GetComponent<PlayerBlob>().grow(mass);
                break;
            case "PlayerClone":
                PlayerClone pcbc = other.GetComponent<PlayerClone>();

                if(pcbc.fakeParent.GetInstanceID() == playerBlob.GetInstanceID())
                {
                    if(pcbc.recombine)
                    {
                        pcbc.kill(true);
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

        if(playerBlob.clones.Count > 0)
        {
            PlayerClone biggestClone = null;
            float minimumSize = 0;
            foreach (PlayerClone cloneObject in playerBlob.clones)
            {
                if(cloneObject != null)
                {
                    float scale = transform.localScale.x;
                    if (scale > minimumSize)
                    {
                        biggestClone = cloneObject;
                        minimumSize = scale;
                    }
                }
            }

            if(biggestClone != null)
            {
                playerBlob.SwapWithClone(biggestClone);//what the fuck am i doing wrong?
                biggestClone.kill();
            }
            
            return;
        }

        playerBlob.kill();
    }*/
}
