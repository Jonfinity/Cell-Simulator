using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICloneConsumeDetection : MonoBehaviour
{
    public static AICloneConsumeDetection instance;

    [SerializeField] private AIClone aiClone;

    [SerializeField] public CircleCollider2D circle;

    private void Awake()
    {
        instance = this;
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if(!aiClone.fakeParent.spawned)
        {
            return;
        }

        float mass = aiClone.rigidBody.mass;
        bool b = Utils.CanEat(other.transform, aiClone.transform);
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
                if(!b)
                {
                    return;
                }

                other.GetComponent<PlayerClone>().grow(mass);
                break;
            case "AI":
                AIBlob aicb = other.GetComponent<AIBlob>();

                if(aicb.GetInstanceID() == aiClone.fakeParent.GetInstanceID())
                {
                    if(aiClone.recombine)
                    {
                        aiClone.kill(true);
                    }

                    return;
                }

                if(!b)
                {
                    return;
                }
                
                aicb.grow(mass);
                break;
            case "AIClone":
                AIClone aicc = other.GetComponent<AIClone>();

                if(aicc.fakeParent.GetInstanceID() == aiClone.fakeParent.GetInstanceID())
                {
                    if(aiClone.recombine)
                    {
                        //aicc.grow(mass, false);
                        //aiClone.kill();
                        //TODO
                    }

                    return;
                }

                if(!b)
                {
                    return;
                }
                
                aicc.grow(mass);
                break;
            default:
                return;
        }

        aiClone.kill();
    }*/
}
