using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIConsumeDetection : MonoBehaviour
{
    public static AIConsumeDetection instance;

    [SerializeField] private AIBlob aiBlob;

    [SerializeField] public CircleCollider2D circle;

    private void Awake()
    {
        instance = this;
    }

    /*private void OnTriggerStay2D(Collider2D other)
    {
        if(!aiBlob.spawned)
        {
            return;
        }

        float mass = aiBlob.rigidBody.mass;
        bool b = Utils.CanEat(other.transform, aiBlob.transform);
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
                if(!b)
                {
                    return;
                }

                other.GetComponent<AIBlob>().grow(mass);
                break;
            case "AIClone":
                AIClone aicc = other.GetComponent<AIClone>();

                if(aicc.fakeParent.GetInstanceID() == aiBlob.GetInstanceID())
                {
                    if(aicc.recombine)
                    {
                        aicc.kill(true);
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

        if(aiBlob.clones.Count > 0)
        {
            AIClone biggestClone = null;
            float minimumSize = 0;
            foreach (AIClone cloneObject in aiBlob.clones)
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
                aiBlob.SwapWithClone(biggestClone);//what the fuck am i doing wrong?
                biggestClone.kill();
            }
            
            return;
        }

        aiBlob.kill();
    }*/
}
