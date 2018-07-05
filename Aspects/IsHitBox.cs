using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IsHitBox : NetworkBehaviour
{
    public ChrController _chr;
    public GameObject _self;
    private ChrController _oChr;

    public enum HitBoxDirections
    {
        Up,
        Down,
        Left,
        Right,
    };
    public HitBoxDirections _hitBoxDirection = HitBoxDirections.Up;

    public enum AttackTypes
    {
        PSlash,
        PSlash2,
        PThrust,
    };

    // current primary move
    public AttackTypes HitBoxAttackType = AttackTypes.PSlash;

    BoxCollider2D _boxy;

    // for Invoke
    private Rigidbody2D _rb;
    private Vector2 _force;

    // Use this for initialization
    void Awake()
    {
        _self = transform.root.gameObject;
        _boxy = GetComponent<BoxCollider2D>();
        _chr = GetComponentInParent<ChrController>();
        if (_boxy == null || _chr == null)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger)
            DetectOther(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.isTrigger)
            DetectOther(other);
    }

    void DetectOther(Collider2D other)
    {
        if (_boxy != null && _chr != null)
        {
            if ((int)_hitBoxDirection == (int)_chr._direction)
            {
                if (_chr._isAttacking)
                {
                    if (_chr.chrControllerType == ChrController.ChrControllerTypes.Player || _chr.chrControllerType == ChrController.ChrControllerTypes.AI_NPC)
                    {
                        if (other.gameObject.tag == "Enemy")
                        {
                            DmgOther(other);
                        }
                    }
                    else
                    {
                        if (other.gameObject.tag == "Player")
                        {
                            DmgOther(other);
                        }
                    }
                }
            }
        }
    }

    void DmgOther(Collider2D other)
    {
        ChrController otherCtrl = other.gameObject.GetComponent<ChrController>();
        if (otherCtrl != null && !otherCtrl._isDead && !otherCtrl._isHurt)
        {
            _oChr = otherCtrl;
            if (HitBoxAttackType == AttackTypes.PSlash && _chr.CurrentPrimaryAction == ChrController.PrimaryActionList.PSlash)
            {
                otherCtrl.TakeDmg(_self, 0, _chr._dmgSlash);
                //_chr._isAttacking = false;
                pushBack(1.0f, _chr._direction, other.gameObject);
            }
            else if (HitBoxAttackType == AttackTypes.PSlash2 && _chr.CurrentPrimaryAction == ChrController.PrimaryActionList.PSlash2)
            {
                otherCtrl.TakeDmg(_self, 0, _chr._dmgSlash2);
                //_chr._isAttacking = false;
                pushBack(1.2f, _chr._direction, other.gameObject);
            }
            else if (HitBoxAttackType == AttackTypes.PThrust && _chr.CurrentPrimaryAction == ChrController.PrimaryActionList.PThrust)
            {
                otherCtrl.TakeDmg(_self, 1, _chr._dmgThrust);
                //_chr._isAttacking = false;
                pushBack(1.2f, _chr._direction, other.gameObject);
            }
            else if (HitBoxAttackType == AttackTypes.PThrust && _chr.CurrentPrimaryAction == ChrController.PrimaryActionList.PThrust2)
            {
                otherCtrl.TakeDmg(_self, 1, _chr._dmgThrust2);
                //_chr._isAttacking = false;
                pushBack(1.5f, _chr._direction, other.gameObject);
            }
        }
    }

    private void pushBack(float v, ChrController.Directions direction, GameObject other)
    {
        switch (direction)
        {
            case ChrController.Directions.Up:
                {
                    _force = new Vector2(0, v * 3);
                }
                break;
            case ChrController.Directions.Down:
                {
                    _force = new Vector2(0, -v * 3);
                }
                break;
            case ChrController.Directions.Left:
                {
                    _force = new Vector2(-v * 3, 0);
                }
                break;
            case ChrController.Directions.Right:
                {
                    _force = new Vector2(v * 3, 0);
                }
                break;
        }
        Invoke("InvokePushBack", 0.3f);
    }

    private void InvokePushBack()
    {
        if (_oChr != null && !_oChr._isDead)
            _oChr.PushBack(_force, ForceMode2D.Impulse);
    }
}
