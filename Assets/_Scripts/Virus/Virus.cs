using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Virus : MonoBehaviour
{
    public const float SCALE = 4.6f;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private SortingGroup sortingGroup;

    private int lifetimeTimestamp;

    private void Start()
    {
        ResetObject();
    }

    private void Update()
    {
        if(lifetimeTimestamp < Utils.secondsSinceEpoch())
        {
            ResetObject();
        }
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * 10 * Time.fixedDeltaTime);

        if(transform.localScale.x != Utils.virusScale.x)
        {
            transform.localScale = Utils.virusScale;
            sortingGroup.sortingOrder = 100 + (int)(transform.localScale.x * 2.5f);
        }

        Map.BorderCheck(rigidBody);
    }

    private void LateUpdate()
    {
        if(transform.localScale == Vector3.zero)
        {
            lifetimeTimestamp = GenerateTimestamp();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(transform.localScale.x < SCALE)
        {
            return;
        }

        switch(other.gameObject.tag)
        {
            case "PlayerMass":
                if(other.attachedRigidbody.velocity != Vector2.zero)
                {
                    rigidBody.AddForce(-(other.GetComponent<PlayerMass>().fakeParent.transform.position - transform.position) * 10, ForceMode2D.Impulse);
                }

                Destroy(other.gameObject);
                break;
            default:
                return;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        float scale = transform.localScale.x;
        if(scale < SCALE || other.transform.localScale.x < scale * 2.5f)
        {
            return;
        }

        switch(other.gameObject.tag)
        {
            case "Player":
                Game.instance.eatVirus(gameObject);

                PlayerBlob playerComponent = other.GetComponent<PlayerBlob>();
                playerComponent.playerStats.virusesEaten++;

                playerComponent.explode(100);
                break;
            case "PlayerClone":
                Game.instance.eatVirus(gameObject);

                PlayerClone playerCloneComponent = other.GetComponent<PlayerClone>();
                playerCloneComponent.fakeParent.playerStats.virusesEaten++;

                playerCloneComponent.explode(100);
                break;
            case "AI":
                Game.instance.eatVirus(gameObject);

                other.GetComponent<AIBlob>().explode(100);
                break;
             case "AIClone":
                Game.instance.eatVirus(gameObject);

                other.GetComponent<AIClone>().explode(100);
                break;
            default:
                return;
        }
    }

    public void ResetObject()
    {
        transform.localScale = Vector2.zero;
        transform.Rotate(0, 0, Random.Range(0.0f, 360.0f));
        lifetimeTimestamp = GenerateTimestamp();

        transform.position = Map.GenerateRandomPosition();
    }

    private int GenerateTimestamp()
    {
        return Utils.secondsSinceEpoch() + Random.Range(200, 300);
    }
}