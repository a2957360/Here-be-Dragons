using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IsNetwork : NetworkBehaviour
{
    public GameObject _localPlayerIcon;

    ChrController _chr;

    public override void OnStartLocalPlayer()
    {
        _chr = GetComponent<ChrController>();

        if (_localPlayerIcon != null && _chr.chrControllerType == ChrController.ChrControllerTypes.Player)
        {
            _localPlayerIcon.SetActive(true);
        }

        //GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        //Renderer[] rens = gameObject.GetComponentsInChildren<Renderer>();
        //foreach (Renderer ren in rens)
        //{
        //    ren.enabled = false;
        //}
    }

    //public override void PreStartClient()
    //{
    //    GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    //}
}
