using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Game : MonoBehaviour
{
    public static Game instance;

    public bool paused = false;

    [SerializeField] private Transform player;
    [SerializeField] public Transform ai;

    [SerializeField] private GameObject food;
    [SerializeField] private GameObject whiteHole;
    [SerializeField] private GameObject virusPrefab;

    [SerializeField] public AudioClip foodConsumeSound;

    private int spawnedFood = 0;
    private int spawnedWhiteHoles = 0;
    private int spawnedViruses = 0;

    public List<UniversalPlayer> players;

    private void Awake() 
    {
        instance = this;

        if(Application.targetFrameRate < 60)
        {
            Application.targetFrameRate = 60;
        }
    }

    private void Start() 
    {
        Instantiate(player, Vector2.zero, Quaternion.identity);

        for(int i = 0; i < Map.maxFood - 100; i++)
        {
            spawnFood();
        }

        for(int i = 0; i < Map.maxWhiteHole - 2; i++)
        {
            spawnWhiteHole();
        }

        for(int i = 0; i < Map.maxViruses; i++)
        {
            spawnBlackHole();
        }

        StartCoroutine(PlayersCoroutine());
    }

    IEnumerator PlayersCoroutine()
    {
        yield return new WaitForSeconds(1);

        SortPlayers();

        StartCoroutine(PlayersCoroutine());
    }

    public void eatFood(GameObject obj)
    {
        obj.transform.localScale = Vector2.zero;
        obj.transform.position = Map.GenerateRandomPosition();
    }

    public void eatHeavyFood(GameObject obj)
    {
        obj.transform.localScale = Vector2.zero;
        obj.transform.Rotate(0, 0, Random.Range(0.0f, 360.0f));
        obj.transform.position = Map.GenerateRandomPosition();
    }

    public void eatWhiteHole(GameObject obj)
    {
        obj.transform.localScale = Vector2.zero;
        obj.transform.Rotate(0, 0, Random.Range(0.0f, 360.0f));
        obj.transform.position = Map.GenerateRandomPosition();
    }

    public void eatVirus(GameObject obj)
    {
        obj.transform.localScale = Vector2.zero;
        obj.transform.Rotate(0, 0, Random.Range(0.0f, 360.0f));
        obj.transform.position = Map.GenerateRandomPosition();
    }

    public void spawnFood()
    {
        if (spawnedFood >= Map.maxFood)
        {
            return;
        }

        Instantiate(food, Map.GenerateRandomPosition(), Quaternion.identity);

        spawnedFood++;
    }

    public void despawnFood(GameObject obj)
    {
        Destroy(obj);

        spawnedFood--;
    }

    public void spawnWhiteHole()
    {
        if (spawnedWhiteHoles >= Map.maxWhiteHole)
        {
            return;
        }
        
        Instantiate(whiteHole, Map.GenerateRandomPosition(), Quaternion.identity);

        spawnedWhiteHoles++;
    }

    public void despawnWhiteHole(GameObject obj)
    {
        Destroy(obj);

        spawnedWhiteHoles--;
    }

    public void spawnBlackHole()
    {
        if (spawnedViruses >= Map.maxViruses)
        {
            return;
        }

        Instantiate(virusPrefab, Map.GenerateRandomPosition(), Quaternion.identity);

        spawnedViruses++;
    }

    public void despawnBlackHole(GameObject obj)
    {
        Destroy(obj);

        spawnedViruses--;
    }

    public bool HasPlayer(UniversalPlayer p)
    {
        return players.Contains(p);
    }

    public void AddPlayer(UniversalPlayer p)
    {
        players.Add(p);
        SortPlayers();
    }

    public void RemovePlayer(UniversalPlayer p)
    {
        players.Remove(p);
        SortPlayers();
    }

    public void SortPlayers()
    {
        players = players.OrderByDescending(ch => ch.totalMass).ToList();

        int count = players.Count;

        UniversalPlayer first = count > 0 ? players[0] : null;
        UniversalPlayer second = count > 1 ? players[1] : null;
        UniversalPlayer third = count > 2 ? players[2] : null;
        UniversalPlayer fourth = count > 3 ? players[3] : null;
        UniversalPlayer fifth = count > 4 ? players[4] : null;

        for (int i = 0; i < count; i++)
        {
            UniversalPlayer player = players[i];
            if (player.playerBlob != null)
            {
                PlayerBlob blob = player.playerBlob;

                if(players.IndexOf(player) > 4)
                {
                    blob.playerHud.leaderboardMyPosition.text = string.Format("{0}. {1}", i + 1, blob.GetUsername());
                }
                else
                {
                    UniversalPlayer sixth = count > 5 ? players[5] : null;
                    if(sixth != null)
                    {
                        blob.playerHud.leaderboardMyPosition.text = string.Format("{0}. {1}", 6, sixth.GetUsername());
                    }
                }

                if(first != null) blob.playerHud.leaderboardFirst.text = string.Format("{0}. {1}", 1, first.GetUsername());
                if(second != null) blob.playerHud.leaderboardSecond.text = string.Format("{0}. {1}", 2, second.GetUsername());
                if(third != null) blob.playerHud.leaderboardThird.text = string.Format("{0}. {1}", 3, third.GetUsername());
                if(fourth != null) blob.playerHud.leaderboardFourth.text = string.Format("{0}. {1}", 4, fourth.GetUsername());
                if(fifth != null) blob.playerHud.leaderboardFifth.text = string.Format("{0}. {1}", 5, fifth.GetUsername());
            }
        }
        
    }
}
