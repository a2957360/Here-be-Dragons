using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * Basic Controller needed for every character.
 * Both player controller and AI
 * */
public class ChrController : NetworkBehaviour
{
    #region public variables
    public NetworkGameManager _netMgr;
    public enum CharacterNames
    {
        Hunter,
        Mage,
        Knight,
        Rogue,
        Elf_Princess,
        Elf_Prince,
        Elf_Warrior,
        Orc_Assassin,
        Orc_General,
        Orc_Grunt,
        Skeleton_Archer,
        Skeleton_Knight,
        Skeleton_Mage,
    };

    public CharacterNames _ChrName = CharacterNames.Hunter;

    public enum Directions
    {
        Up,
        Down,
        Left,
        Right,
    };

    [SyncVar]
    public Directions _direction = Directions.Right;

    public float _HP = 100.0f;
    public float _MP = 50.0f;
    public float _speed = 3.0f;
    public float _DEF_Slash = 5.0f;
    public float _DEF_Thrust = 5.0f;
    public float _DEF_Magic = 5.0f;

    public float _pActionCoolDown = 1.0f;
    public float _sActionCoolDown = 2.0f;

    public float _dmgSlash = 0;
    public float _dmgSlash2 = 0;
    public float _dmgThrust = 0;
    public float _dmgThrust2 = 0;

    // can this chr execute this attack?
    public bool _canSlash = false;
    public bool _canSlash2 = false;
    public bool _canThrust = false;
    public bool _canThrust2 = false;
    public bool _canUseBow = false;
    public bool _canUseSpell = false;

    public GameObject _ArrowObj;
    public GameObject _SpellObj;

    [SyncVar]
    public GameObject _lastEnemy;

    public GameObject _localPlayerIcon;

    public enum ChrControllerTypes
    {
        Player,
        NetWork_Player,
        AI_NPC,
        AI_Enemy,
        AI_Boss,
    };

    public ChrControllerTypes chrControllerType = ChrControllerTypes.Player;

    public enum PrimaryActionList
    {
        PSlash,
        PSlash2,
        PThrust,
        PThrust2,
    };

    // current primary move
    public PrimaryActionList CurrentPrimaryAction = PrimaryActionList.PSlash;

    public enum SecondaryActionList
    {
        SBow,
        SSpell,
    };

    // current seconday move
    public SecondaryActionList CurrentSecondaryAction = SecondaryActionList.SBow;

    public GameObject _healthBar;
    #endregion

    #region protected and private
    Animator _anim;
    NetworkAnimator _netAnim;
    Rigidbody2D _rb;
    //ChrNetworkController _chrNet;

    [HideInInspector]
    public float _h = 0;
    [HideInInspector]
    public float _v = 0;
    [HideInInspector]
    public bool _pAction = false;
    [HideInInspector]
    public bool _sAction = false;


    [SyncVar]
    public bool _isAttacking = false;
    [SyncVar]
    public bool _isDead = false;
    [SyncVar]
    public bool _isHurt = false;
    [SyncVar]
    private bool _canMove = true;

    [SyncVar(hook = "OnChangeHealth")]
    public float _curHP = 0;
    [SyncVar]
    public float _curMP = 0;

    private float _pActionTimer = 0.0f;
    private float _sActionTimer = 0.0f;

    private float _moveFreezeCountDown = 0.0f;

    private float _hurtDurationCountDown = 0.0f;
    private float _slashDuration = 0.38f;
    private float _slash2Duration = 0.5f;
    private float _thrustDuration = 0.5f;
    private float _thrust2Duration = 0.5f;
    private float _bowDuration = 1.0f;
    private float _spellDuration = 1.0f;

    private NetworkStartPosition[] spawnPoints;
    #endregion

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _netAnim = GetComponent<NetworkAnimator>();
        _rb = GetComponent<Rigidbody2D>();
        _netAnim.SetParameterAutoSend(0, true);
        _netAnim.SetParameterAutoSend(1, true);
        _netAnim.SetParameterAutoSend(2, true);
        _netAnim.SetParameterAutoSend(3, true);
        _netAnim.SetParameterAutoSend(4, true);
        _netAnim.SetParameterAutoSend(5, true);
    }

    // Use this for initialization
    void Start()
    {
        _curHP = _HP;
        _curMP = _MP;
        if (isLocalPlayer)
        {
            spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        }
        InvokeRepeating("GetNetworkGameManager", 1.0f, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        if (!_canMove)
        {
            if (_moveFreezeCountDown <= 0)
            {
                Cmd_SyncCanMove(true);
                Cmd_SyncIsAttack(false);
                //_canMove = true;
                //_isAttacking = false;
            }
            else
            {
                _moveFreezeCountDown -= Time.deltaTime;
            }
        }
        if (_isHurt)
        {
            if (_hurtDurationCountDown <= 0)
            {
                Cmd_SyncIsHurt(false);
                //_isHurt = false;
            }
            else
            {
                _hurtDurationCountDown -= Time.deltaTime;
            }
        }
        _pActionTimer += Time.deltaTime;
        _sActionTimer += Time.deltaTime;

        if (_canMove && !_isHurt && !_isDead)
        {
            SetAnimation(_h, _v);
            Move(_h, _v);
        }

        AttackProcessing(_pAction, _sAction);
    }

    [Command]
    void Cmd_SyncIsAttack(bool val)
    {
        if (!isServer)
            return;
        _isAttacking = val;
    }

    [Command]
    void Cmd_SyncIsDead(bool val)
    {
        if (!isServer)
            return;
        _isDead = val;
    }

    [Command]
    void Cmd_SyncIsHurt(bool val)
    {
        if (!isServer)
            return;
        _isHurt = val;
    }

    [Command]
    void Cmd_SyncCanMove(bool val)
    {
        if (!isServer)
            return;
        _canMove = val;
    }

    void SetAnimation(float h, float v)
    {
        _anim.SetFloat("Horizontal", h);
        _anim.SetFloat("Vertical", v);

        if (h > 0 && h >= Mathf.Abs(v))
        {
            FaceRight();
        }
        else if (h < 0 && Mathf.Abs(h) >= Mathf.Abs(v))
        {
            FaceLeft();
        }
        else if (v > 0)
        {
            FaceUp();
        }
        else if (v < 0)
        {
            FaceDown();
        }
    }

    void Move(float h, float v)
    {
        Vector2 move = new Vector2(h, v);
        if (Vector3.Magnitude(move) > 1)
            move = move.normalized;
        transform.Translate(move * _speed * Time.deltaTime);
    }

    void AttackProcessing(bool pAction, bool sAction)
    {
        if (pAction && _pActionTimer >= _pActionCoolDown)
        {
            if (_canSlash && CurrentPrimaryAction == PrimaryActionList.PSlash)
            {
                //_anim.SetTrigger("TgPSlash");
                _netAnim.SetTrigger("TgPSlash");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgPSlash");
                _moveFreezeCountDown = _slashDuration;
                Cmd_SyncCanMove(false);
                Cmd_SyncIsAttack(true);
                //_canMove = false;
                //_isAttacking = true;
            }
            else if (_canSlash2 && CurrentPrimaryAction == PrimaryActionList.PSlash2)
            {
                // _anim.SetTrigger("TgPSlash2");
                _netAnim.SetTrigger("TgPSlash2");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgPSlash2");
                //_canMove = false;
                _moveFreezeCountDown = _slash2Duration;
                //_isAttacking = true;
                Cmd_SyncCanMove(false);
                Cmd_SyncIsAttack(true);
            }
            else if (_canThrust && CurrentPrimaryAction == PrimaryActionList.PThrust)
            {
                //_anim.SetTrigger("TgPThrust");
                _netAnim.SetTrigger("TgPThrust");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgPThrust");
                //_canMove = false;
                _moveFreezeCountDown = _thrustDuration;
                //_isAttacking = true;
                Cmd_SyncCanMove(false);
                Cmd_SyncIsAttack(true);
            }
            else if (_canThrust2 && CurrentPrimaryAction == PrimaryActionList.PThrust2)
            {
                // _anim.SetTrigger("TgPThrust2");
                _netAnim.SetTrigger("TgPThrust2");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgPThrust2");
                //_canMove = false;
                _moveFreezeCountDown = _thrust2Duration;
                //_isAttacking = true;
                Cmd_SyncCanMove(false);
                Cmd_SyncIsAttack(true);
            }
            _pActionTimer = 0;
            _sActionTimer -= 0.3f;
        }
        else if (sAction && _sActionTimer >= _sActionCoolDown)
        {
            if (_canUseBow && CurrentSecondaryAction == SecondaryActionList.SBow)
            {
                //_anim.SetTrigger("TgSBow");
                _netAnim.SetTrigger("TgSBow");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgSBow");
                //_canMove = false;
                Cmd_SyncCanMove(false);
                _moveFreezeCountDown = _bowDuration;
                Invoke("ReleaseArrow", 0.5f);
            }
            else if (_canUseSpell && CurrentSecondaryAction == SecondaryActionList.SSpell)
            {
                // _anim.SetTrigger("TgSSpell");
                _netAnim.SetTrigger("TgSSpell");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgSSpell");
                //_canMove = false;
                Cmd_SyncCanMove(false);
                _moveFreezeCountDown = _spellDuration;
                Invoke("ReleaseSpell", 0.5f);
            }
            _pActionTimer -= 0.3f;
            _sActionTimer = 0;
        }
    }

    void OnChangeHealth(float _curHP)
    {
        if (_healthBar != null)
        {
            Vector3 lScale = _healthBar.transform.localScale;
            float healthPercent = 0;
            if (_curHP > 0)
                healthPercent = _curHP / _HP;
            _healthBar.transform.localScale = new Vector3(healthPercent, lScale.y, lScale.z);
        }
    }

    /*
     * type:
     *      0: Slash
     *      1: Thrust
     *      2: Magic
     *      3: Status
     *      */
    public void TakeDmg(GameObject lastEnemy, int type, float amount)
    {
        if (!isServer)
            return;
        if (!_isDead && !_isHurt)
        {
            _lastEnemy = lastEnemy;
            float dmgReduce = 0;
            switch (type)
            {
                case 0:
                    dmgReduce = Random.Range(0.0f, _DEF_Slash);
                    break;
                case 1:
                    dmgReduce = Random.Range(0.0f, _DEF_Thrust);
                    break;
                case 2:
                    dmgReduce = Random.Range(0.0f, _DEF_Magic);
                    break;
                case 3:
                    break;
            }
            _curHP -= amount * (1.0f - dmgReduce * 0.1f);
            if (_curHP <= 0)
            {
                Death(true);
            }
            else
            {
                Rpc_LocalTakeDmg();
            }

        }
    }

    [ClientRpc]
    void Rpc_LocalTakeDmg()
    {
        if (hasAuthority)
        {
            _anim.SetFloat("Horizontal", 0);
            _anim.SetFloat("Vertical", 0);
            _netAnim.SetTrigger("TgHurt");
            if (NetworkServer.active)
                _anim.ResetTrigger("TgHurt");
            //_anim.SetTrigger("TgHurt");
            _hurtDurationCountDown = 0.5f;
            //_isHurt = true;
            //_isAttacking = false;
            Cmd_SyncIsHurt(true);
            Cmd_SyncIsAttack(false);
        }
    }

    public void Death(bool animate)
    {
        if (!isServer)
            return;
        _isDead = true;
        if (chrControllerType == ChrControllerTypes.Player || _ChrName == CharacterNames.Hunter)
        {
            if (!_netMgr._gameWin && !_netMgr._gameOver)
            {
                _netMgr.SyncGameOver(true);
            }
            if (chrControllerType == ChrControllerTypes.AI_NPC)
                Destroy(gameObject, 3.0f);
        }
        else
        {
            if (chrControllerType == ChrControllerTypes.AI_Enemy || chrControllerType == ChrControllerTypes.AI_Boss)
            {
                if (!_netMgr._gameOver && !_netMgr._gameWin)
                    _netMgr.EnemyDied();
            }
            Destroy(gameObject, 3.0f);
        }
        Rpc_Death(animate);
    }

    [ClientRpc]
    void Rpc_Death(bool animate)
    {
        //Destroy(gameObject, 3.0f);
        if (hasAuthority)
        {
            _anim.SetFloat("Horizontal", 0);
            _anim.SetFloat("Vertical", 0);
            //if (chrControllerType == ChrControllerTypes.Player)
            //{
            //    if (!GameManager.Instance._gameWin)
            //        GameManager.Instance._gameOver = true;
            //}
            Cmd_SyncIsDead(true);
            //_isDead = true;

            if (animate)
            {
                _netAnim.SetTrigger("TgDie");
                //_anim.SetTrigger("TgDie");
                if (NetworkServer.active)
                    _anim.ResetTrigger("TgDie");
            }
            //_anim.SetTrigger("TgDie");
            //if (gameObject.tag == "Player")
            //{
            //    GameManager.Instance.HeroDied();
            //}
            //else
            //{
            //    GameManager.Instance.EnemyDied();
            //}
        }
    }

    void ReleaseArrow()
    {
        Cmd_ReleaseArrow();
    }

    [Command]
    void Cmd_ReleaseArrow()
    {
        if (_ArrowObj != null && !_isHurt && !_isDead)
        {
            GameObject spawn = (GameObject)Instantiate(
                   _ArrowObj,
                   transform.position,
                   Quaternion.identity);
            ProjectileController pCtrl = spawn.GetComponent<ProjectileController>();
            if (pCtrl != null)
            {
                pCtrl._caster = gameObject;
                if (gameObject.tag == "Player")
                    pCtrl._dmgTarget = ProjectileController.DamageTargets.Enemy;
                else
                    pCtrl._dmgTarget = ProjectileController.DamageTargets.Player;

                switch (_direction)
                {
                    case Directions.Up:
                        pCtrl._direction = ProjectileController.Directions.Up;
                        break;
                    case Directions.Down:
                        pCtrl._direction = ProjectileController.Directions.Down;
                        break;
                    case Directions.Left:
                        pCtrl._direction = ProjectileController.Directions.Left;
                        break;
                    case Directions.Right:
                        pCtrl._direction = ProjectileController.Directions.Right;
                        break;
                }
            }
            NetworkServer.Spawn(spawn);
        }
    }

    void ReleaseSpell()
    {
        Cmd_ReleaseSpell();
    }

    [Command]
    void Cmd_ReleaseSpell()
    {
        if (_SpellObj != null && !_isHurt && !_isDead)
        {
            GameObject spawn = (GameObject)Instantiate(
                  _SpellObj,
                  transform.position,
                  Quaternion.identity);
            ProjectileController pCtrl = spawn.GetComponent<ProjectileController>();
            if (pCtrl != null)
            {
                pCtrl._caster = gameObject;
                if (gameObject.tag == "Player")
                    pCtrl._dmgTarget = ProjectileController.DamageTargets.Enemy;
                else
                    pCtrl._dmgTarget = ProjectileController.DamageTargets.Player;

                switch (_direction)
                {
                    case Directions.Up:
                        pCtrl._direction = ProjectileController.Directions.Up;
                        break;
                    case Directions.Down:
                        pCtrl._direction = ProjectileController.Directions.Down;
                        break;
                    case Directions.Left:
                        pCtrl._direction = ProjectileController.Directions.Left;
                        break;
                    case Directions.Right:
                        pCtrl._direction = ProjectileController.Directions.Right;
                        break;
                }
            }
            NetworkServer.Spawn(spawn);
        }
    }

    public void FaceRight()
    {
        _anim.SetFloat("Left", 0.0f);
        _anim.SetFloat("Right", 1.0f);
        _anim.SetFloat("Up", 0.0f);
        _anim.SetFloat("Down", 0.0f);
        Cmd_changeDir(Directions.Right);
    }

    public void FaceLeft()
    {
        _anim.SetFloat("Right", 0.0f);
        _anim.SetFloat("Left", 1.0f);
        _anim.SetFloat("Up", 0.0f);
        _anim.SetFloat("Down", 0.0f);
        Cmd_changeDir(Directions.Left);
    }

    public void FaceUp()
    {
        _anim.SetFloat("Right", 0.0f);
        _anim.SetFloat("Left", 0.0f);
        _anim.SetFloat("Up", 1.0f);
        _anim.SetFloat("Down", 0.0f);
        Cmd_changeDir(Directions.Up);
    }

    public void FaceDown()
    {
        _anim.SetFloat("Right", 0.0f);
        _anim.SetFloat("Left", 0.0f);
        _anim.SetFloat("Up", 0.0f);
        _anim.SetFloat("Down", 1.0f);
        Cmd_changeDir(Directions.Down);
    }

    [Command]
    public void Cmd_changeDir(Directions newDir)
    {
        _direction = newDir;
    }

    public void PushBack(Vector2 force, UnityEngine.ForceMode2D type)
    {
        if (!isServer)
            return;
        Rpc_PushBack(force, type);
    }

    [ClientRpc]
    void Rpc_PushBack(Vector2 force, UnityEngine.ForceMode2D type)
    {
        if (hasAuthority)
        {
            if (_rb != null)
                _rb.AddForce(force, type);
        }
    }

    public void Respawn()
    {
        if (!isServer)
            return;
        _curHP = _HP;
        _isHurt = false;
        _moveFreezeCountDown = 0.0f;
        _hurtDurationCountDown = 0.0f;
        _isAttacking = false;
        _canMove = true;
        if (_isDead)
        {
            _isDead = false;
            Rpc_Respawn();
        }
        Rpc_ReLocate();
    }

    [ClientRpc]
    void Rpc_ReLocate()
    {
        if (isLocalPlayer)
        {
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            // Set the player’s position to the chosen spawn point
            transform.position = spawnPoint;
        }
    }

    [ClientRpc]
    void Rpc_Respawn()
    {
        if (isLocalPlayer)
        {
            _anim.ResetTrigger("TgDie");
            _netAnim.SetTrigger("TgSpawn");
            if (NetworkServer.active)
                _anim.ResetTrigger("TgSpawn");
        }
    }

    void GetNetworkGameManager()
    {
        if (_netMgr == null)
            _netMgr = GameObject.Find("NetworkGameManager").GetComponent<NetworkGameManager>();
    }
}
