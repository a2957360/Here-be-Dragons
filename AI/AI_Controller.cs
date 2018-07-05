using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Controller : AI_Base
{
    // lower Left of the navigatable space
    public Vector2 _pos_LowerLeft = new Vector2(0.0f, 0.0f);
    // upper Right of the navigatable space
    public Vector2 _pos_UpperRight = new Vector2(10.0f, 10.0f);

    public float _velocity = 10.0f;

    // how long before AI decide where to go next?
    public float _decisionInterval = 0.5f;
    private float _decisionTimer = 0;

    public float _chaseDist = 6.0f;

    public float _meleeThreshold = 1.5f;
    public float _RangeThreshold = 5.0f;
    public float _RetreatThreshold = 3.5f;

    private float _h = 0.2f;
    private float _v = 0.2f;
    private bool _pAction = false;
    private bool _sAction = false;

    private float _areaWidth;
    private float _areaHeight;

    Vector2 _curPos;
    Vector2 _targetPos;
    // distance between this chr and enemy
    float _dist;

    private AI_Controller _friendCtrl;

    void Start()
    {
        _areaWidth = _pos_UpperRight.x - _pos_LowerLeft.x;
        _areaHeight = _pos_UpperRight.y - _pos_LowerLeft.y;
        _h = Random.Range(-0.3f, 0.3f);
        _v = Random.Range(-0.3f, 0.3f);
        _dist = 0;
        _curPos.Set(0, 0);
        _targetPos.Set(0, 0);

        _pos_UpperRight.Set(GameManager.Instance._mapWidth, GameManager.Instance._mapHeight);
    }

    void Update()
    {
        if (!hasAuthority)
        {
            return;
        }
        if (_detectionTimer <= _detectionCoolDown)
            _detectionTimer += Time.deltaTime;
        else if (!_gameOn)
            _gameOn = true;

        if (_AIState == AIStates.Explore || _AIState == AIStates.Patrol)
            _decisionTimer += Time.deltaTime;
        else
            _decisionTimer += Time.deltaTime * 1.5f;

        if (_gameOn && _decisionTimer > _decisionInterval)
        {
            _pAction = false;
            _sAction = false;
            if (Random.Range(0, 10) > 5)
            {
                _decisionTimer = 0;
                situationAssessment();

                // decision tree for AI
                switch (_AIState)
                {
                    case AIStates.Explore:
                        RandomState();
                        break;
                    case AIStates.Alert:
                        AlertState();
                        break;
                    case AIStates.Retreat:
                        RetreatState();
                        break;
                    case AIStates.Patrol:
                        PatrolState();
                        break;
                    case AIStates.Escort:
                        EscortState();
                        break;
                    case AIStates.Panic:
                        PanicState();
                        break;
                    case AIStates.Guard:
                        GuardState();
                        break;
                    default:
                        break;
                }

                _chr._h = _h;
                _chr._v = _v;
                _chr._pAction = _pAction;
                _chr._sAction = _sAction;
            }
            else
            {
                _decisionTimer -= 0.1f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_gameOn)
        {
            if (_Enemy == null && _detectionTimer >= _detectionCoolDown)
            {
                if (!_isEscorting && other.gameObject.tag == "Player" && gameObject.tag == "Enemy")
                {
                    _detectionTimer = 2.0f;
                    _Enemy = other.gameObject;
                    _AIState = AIStates.Alert;

                }
                else if (!_isEscorting && other.gameObject.tag == "Enemy" && gameObject.tag == "Player")
                {
                    _detectionTimer = 2.0f;
                    _Enemy = other.gameObject;
                    _AIState = AIStates.Alert;
                }
            }

            if (_Friend == null && _canEscort && !_isPanicing && !_isRaging && _detectionTimer >= _detectionCoolDown)
            {
                if ((other.gameObject.tag == "Enemy" && gameObject.tag == "Enemy") || (other.gameObject.tag == "Player" && gameObject.tag == "Player"))
                {
                    _detectionTimer = 2.0f;
                    AI_Controller friend = other.gameObject.GetComponent<AI_Controller>();
                    if (friend._EnemyName == _SupporterName)
                    {
                        _Friend = other.gameObject;
                    }
                }
            }
        }
    }
    #region states
    void RandomState()
    {
        if (_canPatrol && _Enemy == null)
            _AIState = AIStates.Patrol;
        else if (_canDefend && _Enemy == null)
            _AIState = AIStates.Guard;

        RandomWander();
    }

    void AlertState()
    {
        if (_Enemy != null)
        {
            if (_dist > _chaseDist)
            {
                _Enemy = null;
            }
            else
            {
                int patternNum = 0;
                if (_canMelee && Random.Range(0, 12) > 1)
                    patternNum = 1;
                if (!_isRaging && _canRange && Random.Range(0, 10) > 6)
                    patternNum = 2;

                switch (patternNum)
                {
                    case 0:
                        FleeEnemy();
                        break;
                    case 1:
                        MeleePattern();
                        break;
                    case 2:
                        RangePattern();
                        break;
                }
            }
        }
        else
        {
            _AIState = AIStates.Explore;
        }
    }

    void PatrolState()
    {
        if (_patrolCtrl != null && _patrolCtrl._waypts.Count != 0)
        {
            if (_Enemy == null)
                ApproachWayPoint();
            else
                _AIState = AIStates.Alert;
        }
        else
        {
            _canPatrol = false;
            _AIState = AIStates.Explore;
        }
    }

    void GuardState()
    {
        if (_defendCtrl != null && _defendCtrl._defendPt != null)
        {
            if (_Enemy == null)
                GuardDefendPoint();
            else
                _AIState = AIStates.Alert;
        }
        else
        {
            _canDefend = false;
            _AIState = AIStates.Explore;
        }
    }

    void RetreatState()
    {
        if (_Enemy != null && _dist < _chaseDist)
        {
            int patternNum = 0;
            if (_dist < _RetreatThreshold)
            {
                if (_canMelee && Random.Range(0, 12) > 7)
                    patternNum = 1;
                if (_canRange && Random.Range(0, 10) > 8)
                    patternNum = 2;
            }
            else
            {
                if (_canMelee && Random.Range(0, 12) > 7)
                    patternNum = 1;
                if (_canRange && Random.Range(0, 10) > 5)
                    patternNum = 2;
            }
            switch (patternNum)
            {
                case 0:
                    FleeEnemy();
                    break;
                case 1:
                    MeleePattern();
                    break;
                case 2:
                    RangePattern();
                    break;
            }
        }
        else
        {
            _Enemy = null;
            _isRetreating = false;
            _AIState = AIStates.Explore;
        }
    }

    void EscortState()
    {
        if (_Friend != null && _friendCtrl != null)
        {
            _Enemy = _friendCtrl._Enemy;
            if (_Enemy == null || Vector3.Magnitude(_curPos - (Vector2)_Friend.transform.position) > _RetreatThreshold)
            {
                ApproachFriend();
            }
            else
            {
                int patternNum = 0;
                if (_canMelee && Random.Range(0, 12) > 1)
                    patternNum = 1;
                if (!_isRaging && _canRange && Random.Range(0, 10) > 6)
                    patternNum = 2;

                switch (patternNum)
                {
                    case 0:
                        ApproachFriend();
                        break;
                    case 1:
                        MeleePattern();
                        break;
                    case 2:
                        RangePattern();
                        break;
                }
            }
        }
        else
        {
            _Friend = null;
            _isEscorting = false;
            _AIState = AIStates.Alert;
        }
    }

    void PanicState()
    {
        if (_Enemy != null && Random.Range(0, 10) > 1)
        {
            int patternNum = 0;
            if (_canMelee && Random.Range(0, 10) > 7)
                patternNum = 1;
            if (!_isRaging && _canRange && Random.Range(0, 10) > 7)
                patternNum = 2;

            switch (patternNum)
            {
                case 0:
                    if (Random.Range(0, 10) > 3)
                        FleeEnemy();
                    else
                        RandomWander();
                    break;
                case 1:
                    _pAction = true;
                    break;
                case 2:
                    _sAction = true;
                    break;
            }
        }
        else
        {
            _isPanicing = false;
            _AIState = AIStates.Alert;
        }
    }
    #endregion

    #region action Patterns
    void MeleePattern()
    {
        if (_dist <= (_meleeThreshold + Random.Range(-0.2f, 0.3f)))
        {
            if (IsFacingEnemy() || Random.Range(0, 10) > 7)
            {
                _pAction = true;
            }
            else
            {
                if (Random.Range(0, 12) > 5)
                    ApproachEnemy();
                else
                    RandomWander();
            }
        }
        else
        {
            ApproachEnemy();
        }
    }

    void RangePattern()
    {
        if (_dist <= (_meleeThreshold + Random.Range(0.0f, 0.5f)) && Random.Range(0, 10) > 3)
        {
            FleeEnemy();
        }
        else if (_dist <= (_RangeThreshold + Random.Range(-0.1f, 0.5f)))
        {
            if (IsFacingEnemy() || Random.Range(0, 10) > 7)
            {
                _sAction = true;
            }
            else
            {
                if (Random.Range(0, 10) > 5)
                    ApproachEnemy();
                else
                    RandomWander();
            }
        }
        else
        {
            ApproachEnemy();
        }
    }

    #endregion

    #region helper functions
    void RandomWander()
    {
        _h = RandomDirection(_h);
        _v = RandomDirection(_v);
        _h = RandomChangeSpeed(_h);
        _v = RandomChangeSpeed(_v);
        CheckWalls();
    }

    void ApproachTarget(Vector2 target, bool straight)
    {
        if (straight)
        {
            _h = Vector3.Project((_curPos - target).normalized, Vector2.right).magnitude;
            _v = Vector3.Project((_curPos - target).normalized, Vector2.up).magnitude;
        }
        else
        {
            _h = RandomChangeSpeed(_h);
            _v = RandomChangeSpeed(_v);
        }

        if (_curPos.x > target.x)
        {
            if (_h > 0)
                _h = -_h;
        }
        else
        {
            if (_h < 0)
                _h = -_h;
        }

        if (_curPos.y > target.y)
        {
            if (_v > 0)
                _v = -_v;
        }
        else
        {
            if (_v < 0)
                _v = -_v;
        }
    }

    void FleeTarget(Vector2 target)
    {
        _h = RandomChangeSpeed(_h) * 1.3f;
        _v = RandomChangeSpeed(_v) * 1.3f;

        if (_curPos.x > target.x)
        {
            if (_h < 0)
                _h = -_h;
        }
        else
        {
            if (_h > 0)
                _h = -_h;
        }

        if (_curPos.y > target.y)
        {
            if (_v < 0)
                _v = -_v;
        }
        else
        {
            if (_v > 0)
                _v = -_v;
        }
    }

    void ApproachEnemy()
    {
        int num = Random.Range(0, 10);
        if (_chr.chrControllerType == ChrController.ChrControllerTypes.AI_NPC)
        {
            ApproachTarget(_targetPos, num > 5);
        }
        else
        {
            ApproachTarget(_targetPos, num > 2);
        }
        if (Random.Range(0, 10) > 5)
            CheckWalls();
    }

    void FleeEnemy()
    {
        FleeTarget(_targetPos);
        CheckWalls();
    }

    void ApproachFriend()
    {
        float dist = Vector3.Magnitude(_curPos - (Vector2)_Friend.transform.position);
        if (dist > 1.5f)
        {
            ApproachTarget(_Friend.transform.position, true);
        }
        else if (dist < 1.0f)
        {
            FleeTarget(_Friend.transform.position);
        }
        else
        {
            _h = 0;
            _v = 0;
        }
        CheckWalls();
    }

    void ApproachWayPoint()
    {
        if (Vector3.Magnitude(_curPos - (Vector2)_patrolCtrl._waypts[_patrolCtrl._curWayPt].position) <= 1.5f)
        {
            _patrolCtrl._curWayPt = (_patrolCtrl._curWayPt + 1) % _patrolCtrl._waypts.Count;
        }
        ApproachTarget(_patrolCtrl._waypts[_patrolCtrl._curWayPt].position, true);
    }

    void GuardDefendPoint()
    {
        float dist = Vector3.Magnitude(_curPos - _defendCtrl._defendPt);
        if (dist > 3.0f)
        {
            ApproachTarget(_defendCtrl._defendPt, true);
        }
        else if (dist > 1.0f)
        {
            ApproachTarget(_defendCtrl._defendPt, false);
        }
        else
        {
            if (Random.Range(0, 10) > 3)
            {
                _h = 0;
                _v = 0;
            }
            else
                FleeTarget(_defendCtrl._defendPt);
        }
        CheckWalls();
    }

    void situationAssessment()
    {
        if (_Enemy != null && !_isPanicing && _canPanic && (_chr._curHP / _chr._HP < 0.3f))
        {
            if (Random.Range(0, 10) > 7)
            {
                _isEscorting = false;
                _isRetreating = false;
                _isPanicing = true;
                _AIState = AIStates.Panic;
                _Friend = null;
            }
        }

        if (_canEscort && _Friend != null && !_isEscorting && !_isRaging && !_isPanicing)
        {
            _isEscorting = true;
            _AIState = AIStates.Escort;
            _friendCtrl = _Friend.GetComponent<AI_Controller>();
        }

        if (!_isPanicing && !_isRaging && _canRage && (_chr._curHP / _chr._HP < 0.5f))
        {
            _isRaging = true;
            _Friend = null;
            _isEscorting = false;
            _AIState = AIStates.Alert;
        }

        if (!_isPanicing && !_isRaging && _Enemy != null && !_isEscorting && !_isRetreating && _canRetreat && (_chr._curHP / _chr._HP < 0.7f))
        {
            _isRetreating = true;
            _AIState = AIStates.Retreat;
        }

        _curPos = transform.position;
        if (_Enemy != null)
        {
            _targetPos = _Enemy.transform.position;
            _dist = Vector3.Magnitude(_curPos - _targetPos);

            if (!_isEscorting && Random.Range(0, 10) > 5 && _chr._lastEnemy != null && _Enemy != _chr._lastEnemy)
                _Enemy = _chr._lastEnemy;
        }
    }

    float RandomDirection(float i)
    {
        float ret = i;
        if (Random.Range(0, 10) > 7)
        {
            ret = -ret;
        }
        return ret;
    }

    void CheckWalls()
    {
        if (_curPos.x < _pos_LowerLeft.x + _areaWidth * 0.2f)
        {
            if (_h < 0)
            {
                _h = -_h * Random.Range(0.2f, 0.7f);
            }
        }
        else if (_curPos.x > _pos_UpperRight.x - _areaWidth * 0.2f)
        {
            if (_h > 0)
            {
                _h = -_h * Random.Range(0.2f, 0.7f);
            }
        }

        if (_curPos.y < _pos_LowerLeft.y + _areaHeight * 0.2f)
        {
            if (_v < 0)
            {
                _v = -_v * Random.Range(0.2f, 0.7f);
            }
        }
        else if (_curPos.y > _pos_UpperRight.y - _areaHeight * 0.2f)
        {
            if (_v > 0)
            {
                _v = -_v * Random.Range(0.2f, 0.7f);
            }
        }
    }

    float RandomChangeSpeed(float i)
    {
        float ret = i;
        int radNum = Random.Range(0, 10);

        if (radNum < 6)
        {
            if (i >= 0 && i <= 1)
                i += Time.deltaTime * _velocity;
            else if (i <= 0 && i >= -1)
                i -= Time.deltaTime * _velocity;
        }
        else if (radNum < 7)
        {
            i = 0;
        }
        else if (radNum < 9)
        {
            i = Random.Range(0.2f, 0.5f);
        }
        else
        {
            if (i >= 1)
                i -= Time.deltaTime * _velocity;
            else if (i <= -1)
                i += Time.deltaTime * _velocity;
        }
        ret = i;
        return ret;
    }

    bool IsFacingEnemy()
    {
        bool ret = true;
        float xDist = Mathf.Abs(_curPos.x - _targetPos.x);
        float yDist = Mathf.Abs(_curPos.y - _targetPos.y);

        if (xDist > yDist)
        {
            if (_curPos.x > _targetPos.x)
                ret = _chr._direction == ChrController.Directions.Left;
            else
                ret = _chr._direction == ChrController.Directions.Right;
        }
        else
        {
            if (_curPos.y > _targetPos.y)
                ret = _chr._direction == ChrController.Directions.Down;
            else
                ret = _chr._direction == ChrController.Directions.Up;
        }
        return ret;
    }
    #endregion
}
