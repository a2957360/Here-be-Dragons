using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAIPatrol : Mixin
{
    public int _path = 0;
    public int _curWayPt = 0;
    public List<Transform> _waypts = new List<Transform>();

    private void Start()
    {

        if (_path == 0)
        {
            _waypts = GameManager.Instance._wayptsG1;
        }
        else if (_path == 1)
        {
            _waypts = GameManager.Instance._wayptsG2;
        }
        else if (_path == 2)
        {
            _waypts = GameManager.Instance._wayptsG3;
        }
        else if (_path == 3)
        {
            _waypts = GameManager.Instance._guardpts;
        }
        else
        {
            _waypts = GameManager.Instance._wayptsG4;
        }

    }
}
