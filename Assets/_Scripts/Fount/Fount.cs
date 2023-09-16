using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fount : MonoBehaviour
{
    private const int EXP = 50;
    private const int SCALE_MIN = 1;
    private const int SCALE_MAX = 4;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Rigidbody2D rigidBody;

    private int lifetimeTimestamp;

    private Vector2 nextScale = new Vector2(SCALE_MIN, SCALE_MIN);

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
        transform.Rotate(Vector3.back * 15 * Time.fixedDeltaTime);

        if(transform.localScale.x != nextScale.x)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, nextScale, 4f * Time.fixedDeltaTime);
            spriteRenderer.sortingOrder = 100 + (int)transform.localScale.x;
        }

        Map.BorderCheck(rigidBody);
    }

    private void LateUpdate()
    {
        if(transform.localScale == Vector3.zero)
        {
            lifetimeTimestamp = GenerateTimestamp();

            float c = Mathf.Sqrt(rigidBody.mass / 2);
            nextScale = new Vector2(c, c);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(transform.localScale.x < SCALE_MAX)
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
        if(transform.localScale.x < SCALE_MAX)
        {
            return;
        }

        float mass = rigidBody.mass;
        switch(other.gameObject.tag)
        {
            case "Player":
                Game.instance.eatWhiteHole(gameObject);

                PlayerBlob playerComponent = other.GetComponent<PlayerBlob>();
                playerComponent.playerStats.AddExperience(EXP);

                playerComponent.grow(mass, false);
                break;
            case "PlayerClone":
                Game.instance.eatWhiteHole(gameObject);

                PlayerClone playerCloneComponent = other.GetComponent<PlayerClone>();
                playerCloneComponent.fakeParent.playerStats.AddExperience(EXP);

                playerCloneComponent.grow(mass, false);
                break;
            case "AI":
                Game.instance.eatWhiteHole(gameObject);

                other.GetComponent<AIBlob>().grow(mass, false);
                break;
             case "AIClone":
                Game.instance.eatWhiteHole(gameObject);

                other.GetComponent<AIClone>().grow(mass, false);
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

        float c = Mathf.Sqrt(rigidBody.mass / 2);
        nextScale = new Vector2(c, c);
    }

    private int GenerateTimestamp()
    {
        return Utils.secondsSinceEpoch() + Random.Range(200, 300);
    }
}