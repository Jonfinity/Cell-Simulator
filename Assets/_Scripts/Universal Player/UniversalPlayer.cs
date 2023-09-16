using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalPlayer : MonoBehaviour
{
    public static UniversalPlayer instance;

    public string username;
    public float totalMass;

    [SerializeField] public PlayerBlob playerBlob;
    [SerializeField] public GameObject parent;
    [SerializeField] public Rigidbody2D rigidBody;

    public int position;

    private void Awake()
    {
        instance = this;
    }

    public void AddToGame()
    {
        if(!Game.instance.HasPlayer(this))
        {
            Game.instance.AddPlayer(this);
        }
    }

    public void RemoveFromGame()
    {
        if(Game.instance.HasPlayer(this))
        {
            Game.instance.RemovePlayer(this);
        }
    }

    public bool IsHuman()
    {
        return playerBlob != null;
    }

    public string GetUsername()
    {
        if(username == "")
        {
            return "Unnamed";
        }

        return username;
    }
}
