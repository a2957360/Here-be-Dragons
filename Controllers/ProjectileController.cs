using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour
{
    public enum DamageTargets
    {
        Player,
        Enemy,
    };

    [SyncVar]
    public DamageTargets _dmgTarget = DamageTargets.Enemy;

    public enum Directions
    {
        Up,
        Down,
        Left,
        Right,
    };

    [SyncVar]
    public Directions _direction = Directions.Right;

    public float _life = 1.5f;
    public float _speed = 3.0f;
    public float _damage = 20f;
    // if not spell, then its an arrow
    public bool _isSpell = true;

    private float _timer = 0;
    private Vector2 _moveDir;

    private ChrController _oChr;

    [SyncVar]
    public GameObject _caster;

    // Use this for initialization
    void Start()
    {
        Transform thisTrans = gameObject.transform;

        Quaternion newRotation = Quaternion.identity;
        //int dir = (int)_caster.GetComponent<ChrController>()._direction;
        //_direction = (Directions)dir;
        switch (_direction)
        {
            case Directions.Up:
                newRotation = Quaternion.Euler(0, 0, 90.0f);
                _moveDir.Set(0.0f, 1.0f);
                break;
            case Directions.Down:
                newRotation = Quaternion.Euler(0, 0, -90.0f);
                _moveDir.Set(0.0f, -1.0f);
                break;
            case Directions.Left:
                newRotation = Quaternion.Euler(0, 0, 180.0f);
                _moveDir.Set(-1.0f, 0.0f);
                break;
            case Directions.Right:
                newRotation = Quaternion.Euler(0, 0, 0.0f);
                _moveDir.Set(1.0f, 0.0f);
                break;
        }
        thisTrans.rotation = newRotation;
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _life)
            Destroy(gameObject);
        transform.Translate(_moveDir.x * _speed * Time.deltaTime, _moveDir.y * _speed * Time.deltaTime, 0, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string nameTag = other.gameObject.tag;
        if (other.gameObject.layer == 8)
        {
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            if ((_dmgTarget == DamageTargets.Player && nameTag == "Player") || (_dmgTarget == DamageTargets.Enemy && nameTag == "Enemy"))
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                _oChr = other.gameObject.GetComponent<ChrController>();
                if (_oChr != null)
                {
                    _oChr.TakeDmg(_caster, _isSpell ? 2 : 1, _damage);
                    Invoke("InvokePushBack", 0.1f);
                    Destroy(gameObject, 0.15f);
                }
            }
        }
    }

    private void InvokePushBack()
    {
        if (_oChr != null && !_oChr._isDead)
            _oChr.PushBack(_moveDir * 7.0f, ForceMode2D.Impulse);
    }
}
