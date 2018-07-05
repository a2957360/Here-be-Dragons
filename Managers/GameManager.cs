using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    BoardManager _boardScript;

    public int _mapWidth = 15;
    public int _mapHeight = 15;
    public float _mapCenterX = 7.0f;
    public float _mapCenterY = 7.0f;

    public List<Transform> _wayptsG1 = new List<Transform>();
    public List<Transform> _wayptsG2 = new List<Transform>();
    public List<Transform> _wayptsG3 = new List<Transform>();
    public List<Transform> _wayptsG4 = new List<Transform>();
    public List<Transform> _guardpts = new List<Transform>();
    // Use this for initialization
    void Awake()
    {
        _boardScript = GetComponent<BoardManager>();
        InitGame();
#if UNITY_ANDROID
        Screen.orientation = ScreenOrientation.LandscapeRight;
#endif
    }

    void InitGame()
    {
        //_numEnemies = (int)(_level * 0.5f + 2);
        //_numHeroes = (int)(_level * 0.5f);
        if (_boardScript != null)
        {
            _boardScript.columns = _mapWidth;
            _boardScript.rows = _mapHeight;
            _boardScript.SetupScene(0);
            //_boardScript.heroCount = _numHeroes;
            //_boardScript.enemyCount = _numEnemies;
        }
    }

    //    public void Update()
    //    {
    //#if UNITY_ANDROID
    //        if (_debugText != null && _debugText.activeInHierarchy)
    //        {
    //            _debugText.GetComponent<Text>().text = "W: " + Screen.width + "  H: " + Screen.height;
    //        }
    //#endif      
    //    }
}

