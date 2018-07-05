using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AI_Base : NetworkBehaviour
{
    public GameObject _Enemy;
    public GameObject _Friend;

    protected float _detectionCoolDown = 3.0f;
    protected float _detectionTimer = 0;

    public bool _canMelee = false;
    public bool _canRange = false;

    public bool _canRage = false;
    public bool _canRetreat = false;
    public bool _canPatrol = false;
    public bool _canEscort = false;
    public bool _canDefend = false;
    public bool _canPanic = false;


    protected bool _isRaging = false;
    protected bool _isRetreating = false;
    protected bool _isEscorting = false;
    protected bool _isDefending = false;
    protected bool _isPanicing = false;

    protected IsAIPatrol _patrolCtrl;
    protected IsAIDefend _defendCtrl;

    [HideInInspector]
    public bool _gameOn = false;

    public enum AIStates
    {
        Explore,
        Patrol,
        Alert,
        Retreat,
        Escort,
        Panic,
        Guard,
    };

    public AIStates _AIState = AIStates.Explore;

    public enum EnemyNames
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

    public EnemyNames _EnemyName = EnemyNames.Elf_Prince;
    public EnemyNames _SupporterName = EnemyNames.Elf_Prince;

    protected ChrController _chr;

    void Awake()
    {
        _chr = GetComponentInParent<ChrController>();
        if (_chr == null)
        {
            this.enabled = false;
        }
        else
        {
            if (_chr.chrControllerType != ChrController.ChrControllerTypes.AI_Enemy && _chr.chrControllerType != ChrController.ChrControllerTypes.AI_Boss && _chr.chrControllerType != ChrController.ChrControllerTypes.AI_NPC)
            {
                this.enabled = false;
            }
            else
            {
                _canMelee = GetComponent<IsAIMelee>();
                _canRange = GetComponent<IsAIRange>();
                _canRage = GetComponent<IsAIRager>();
                _canRetreat = GetComponent<IsAITactician>();
                _canEscort = GetComponent<IsAISupporter>();
                _canPatrol = GetComponent<IsAIPatrol>();
                _canDefend = GetComponent<IsAIDefend>();
                _canPanic = GetComponent<IsAIPanic>();

                if (_canPatrol)
                    _patrolCtrl = GetComponent<IsAIPatrol>();
                if (_canDefend)
                    _defendCtrl = GetComponent<IsAIDefend>();
            }
        }
    }
}
