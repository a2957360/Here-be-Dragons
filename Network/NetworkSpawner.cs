using UnityEngine;
using UnityEngine.Networking;

public class NetworkSpawner : NetworkBehaviour
{
    public GameObject[] enemyTiles;
    public GameObject[] heroTiles;
    public GameObject[] AIPlayerTiles;
    public int numberOfEnemies = 5;
    public int lowerRange = 0;
    public int upperRange = 8;

    public int _level = 1;

    private float updateTimer = 0.0f;
    private float updateInterval = 5.0f;

    public bool _secondPlayer = false;

    public bool _canSpawn = true;

    public NetworkGameManager _netMgr;

    public override void OnStartServer()
    {
        Invoke("SpawnUnits", 0.3f);
        Invoke("SpawnEnemies", 0.1f);
        _netMgr = GameObject.Find("NetworkGameManager").GetComponent<NetworkGameManager>();
    }

    public void Update()
    {
        if (!isServer)
            return;
        if (updateTimer < updateInterval)
        {
            updateTimer += Time.deltaTime;
        }
        else
        {
            updateTimer = 0.0f;
            if (_netMgr._lvReset)
            {
                Invoke("SpawnUnits", 0.3f);
                Invoke("SpawnEnemies", 0.1f);
                _netMgr._lvReset = false;
            }
        }
    }

    void SpawnUnits()
    {
        if (!isServer || !_canSpawn)
            return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPosition = new Vector3(
                    Random.Range(1.5f, 6.0f),
                    Random.Range(1.5f, 6.0f),
                    0.0f);

                //int radNum = Random.Range(0, heroTiles.Length);
                var hero = (GameObject)Instantiate(heroTiles[i], spawnPosition, Quaternion.identity);
                NetworkServer.SpawnWithClientAuthority(hero, player);
            }

            if (!_secondPlayer)
            {
                var spawnPosition = new Vector3(
                    Random.Range(1.5f, 6.0f),
                    Random.Range(1.5f, 6.0f),
                    0.0f);

                //int radNum = Random.Range(0, heroTiles.Length);
                var hero = (GameObject)Instantiate(AIPlayerTiles[Random.Range(0, AIPlayerTiles.Length)], spawnPosition, Quaternion.identity);
                NetworkServer.SpawnWithClientAuthority(hero, player);
            }
        }
    }

    void SpawnEnemies()
    {
        if (!isServer)
            return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _level = _netMgr._level + 1;

            numberOfEnemies = Mathf.Clamp(Random.Range(3, _level + 2), 3, 7);
            _netMgr._numEnemies = numberOfEnemies;

            upperRange = Mathf.Clamp(_level + 2, 3, 9);
            lowerRange = 0;
            if (Random.Range(0, 10) > 5)
                lowerRange = Random.Range(0, upperRange / 2);

            for (int i = 0; i < numberOfEnemies; i++)
            {
                var spawnPosition = new Vector3(
                    Random.Range(5.0f, 13.5f),
                    Random.Range(10.0f, 13.5f),
                    0.0f);

                int radNum = Random.Range(lowerRange, upperRange);
                var enemy = (GameObject)Instantiate(enemyTiles[radNum], spawnPosition, Quaternion.identity);
                NetworkServer.SpawnWithClientAuthority(enemy, player);
            }
        }
    }
}