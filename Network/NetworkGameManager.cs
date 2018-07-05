using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkGameManager : NetworkBehaviour
{
    public GameObject _gameOverText;
    public GameObject _gameWinText;
    public GameObject _debugText;
    public Text _clockText;
    public Text _scoreText;
    public Text _highText;
    public GameObject _roundText;
    public GameObject _roundEndBG;
    public GameObject _roundStartBG;

    public int _roundDuration = 90;
    [SyncVar]
    public int _ClockCurrent = 90;
    public float _roundStartDuration = 3.0f;
    private float _gameStartTimer = 0;
    public float _gameOverDuration = 5.0f;
    private float _gameOverTimer = 0;
    [SyncVar]
    public bool _gameOverProcessStarted = false;
    [SyncVar]
    public bool _roundStarted = false;
    [SyncVar]
    public bool _roundProcessStarted = false;

    [SyncVar]
    public int _level = 7;
    [SyncVar]
    public int _numEnemies = 0;
    //public int _numHeroes = 0;
    [SyncVar]
    public int _score = 0;
    [SyncVar]
    public int _highScore = 0;

    [SyncVar]
    public bool _lvReset = false;
    [SyncVar]
    public bool _gameOver = false;
    [SyncVar]
    public bool _gameWin = false;

    // Use this for initialization

    public override void OnStartServer()
    {
        InvokeRepeating("ClockUpdate", 0, 1.0f);
    }

    public void Update()
    {
#if UNITY_ANDROID
        if (_debugText != null && _debugText.activeInHierarchy)
        {
            _debugText.GetComponent<Text>().text = "W: " + Screen.width + "  H: " + Screen.height;
        }
#endif
        if (!isServer)
            return;

        if (_gameOver || _gameWin)
        {
            GameOverProcess();
        }
        else if (!_roundStarted)
        {
            RoundStartProcess();
        }
    }

    void RoundStartProcess()
    {
        if (_gameStartTimer > _roundStartDuration)
        {
            _roundStarted = true;
            _ClockCurrent = _roundDuration;
            _gameStartTimer = 0;
            _roundProcessStarted = false;
            Rpc_RoundStarted();
        }
        else
        {
            if (!_roundProcessStarted)
            {
                _roundProcessStarted = true;
                Rpc_RoundStarting();
                Rpc_ClockUpdate();
            }
            _gameStartTimer += Time.deltaTime;
        }

    }

    void GameRestart()
    {
        _gameOverTimer += Time.deltaTime;

        if (_gameOverTimer >= _gameOverDuration)
        {
            _gameOverProcessStarted = false;
            _gameOverTimer = 0.0f;
            if (_gameOver)
            {
                _score = 0;
                Rpc_DisplayScore();
                _level = 1;
            }
            else
            {
                _level++;
            }
            _gameOver = false;
            _gameWin = false;
            _roundStarted = false;
            _ClockCurrent = _roundDuration;

            if (isServer)
            {
                _lvReset = true;
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in players)
                {
                    ChrController chrCtrl = obj.GetComponent<ChrController>();
                    if (chrCtrl != null && chrCtrl.chrControllerType == ChrController.ChrControllerTypes.Player)
                    {
                        chrCtrl.Respawn();
                    }
                }
            }
            Rpc_RoundRestart();
        }
    }

    void GameOverProcess()
    {
        if (!_gameOverProcessStarted)
        {
            if (isServer)
            {
                Googlegameserver.Addacheivement(GPGSIds.achievement_finish_first_battle);
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject obj in enemies)
                {
                    ChrController chrCtrl = obj.GetComponent<ChrController>();
                    if (chrCtrl != null && !chrCtrl._isDead)
                        chrCtrl.Death(false);
                }
                GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");
                foreach (GameObject obj in allies)
                {
                    ChrController chrCtrl = obj.GetComponent<ChrController>();
                    if (chrCtrl != null && !chrCtrl._isDead && chrCtrl.chrControllerType == ChrController.ChrControllerTypes.AI_NPC)
                        chrCtrl.Death(false);
                }
                if (_score > _highScore)
                {
                    _highScore = _score;
                    Rpc_DisplayScore();
                }
                _gameOverProcessStarted = true;
                Rpc_RoundEnd();
                if (_gameOver)
                {
                    Googlegameserver.Addacheivement(GPGSIds.achievement_first_death);
                }
                else if (_gameWin)
                {
                    Googlegameserver.Addacheivement(GPGSIds.achievement_win_your_first_battle);
                }
            }
        }
        else
        {
            GameRestart();
        }
    }

    [ClientRpc]
    void Rpc_RoundEnd()
    {
        if (_gameOver)
        {
            _gameOverText.SetActive(true);
        }
        else if (_gameWin)
        {
            _gameWinText.SetActive(true);
        }
        _roundEndBG.SetActive(true);
    }

    [ClientRpc]
    void Rpc_RoundRestart()
    {
        if (_gameWinText != null)
            _gameWinText.SetActive(false);
        if (_gameOverText != null)
            _gameOverText.SetActive(false);
        _roundEndBG.SetActive(false);
    }

    [ClientRpc]
    void Rpc_RoundStarting()
    {
        _roundText.GetComponent<Text>().text = "ROUND " + _level;
        _roundText.SetActive(true);
        _roundStartBG.SetActive(true);
    }

    [ClientRpc]
    void Rpc_RoundStarted()
    {
        _roundText.SetActive(false);
        _roundStartBG.SetActive(false);
    }

    [ClientRpc]
    void Rpc_DisplayScore()
    {
        string formatString = System.String.Format("{0:D5}", _score);
        _scoreText.text = formatString;
        formatString = System.String.Format("{0:D5}", _highScore);
        _highText.text = formatString;
    }

    //public void HeroDied()
    //{
    //    _numHeroes--;
    //    if (_numHeroes == 0 && _spectorMode)
    //        _gameOver = true;
    //    else if (_numHeroes == -1 && !_spectorMode)
    //        _gameOver = true;
    //}

    public void EnemyDied()
    {
        if (!isServer)
            return;
        if (!_gameOver && !_gameWin)
        {
            if (--_numEnemies <= 0)
                SyncGameWin(true);
            _score += 10;
            Googlegameserver.Addacheivement(GPGSIds.achievement_kill_the_first_enemy);
            Googlegameserver.OnAddScoreToLeaderBorad(_score * 10);
        }
    }

    public void ClockUpdate()
    {
        if (isServer && _roundStarted)
        {
            Googlegameserver.Addacheivement(GPGSIds.achievement_start_the_game);
            if (_ClockCurrent <= 0)
            {
                if (!_gameOver && !_gameWin)
                    _gameOver = true;
            }
            else
            {
                _ClockCurrent--;
            }
            Rpc_ClockUpdate();
            Rpc_DisplayScore();
        }
    }

    [ClientRpc]
    public void Rpc_ClockUpdate()
    {
        _clockText.text = _ClockCurrent.ToString();
    }

    [Command]
    public void Cmd_SyncGameOver(bool val)
    {
        if (!isServer)
            return;
        _gameOver = val;
    }

    [Command]
    public void Cmd_SyncGameWin(bool val)
    {
        if (!isServer)
            return;
        _gameWin = val;
    }

    [ClientRpc]
    public void Rpc_SyncGameOver(bool val)
    {
        _gameOver = val;
    }

    [ClientRpc]
    public void Rpc_SyncGameWin(bool val)
    {
        _gameWin = val;
    }

    public void SyncGameOver(bool val)
    {
        Cmd_SyncGameOver(val);
        Rpc_SyncGameOver(val);
    }

    public void SyncGameWin(bool val)
    {
        Cmd_SyncGameWin(val);
        Rpc_SyncGameWin(val);
    }
}

