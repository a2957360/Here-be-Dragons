using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAIDefend : Mixin
{
    public Vector2 _defendPt;

    private void Start()
    {
        ChangeDefendPoint();
        InvokeRepeating("ChangeDefendPoint", 7.0f, 7.0f);
    }

    private void ChangeDefendPoint()
    {
        _defendPt = GameManager.Instance._guardpts[Random.Range(0, GameManager.Instance._guardpts.Count)].position;
    }
}
