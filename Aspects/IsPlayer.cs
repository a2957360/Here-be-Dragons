using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;

public class IsPlayer : NetworkBehaviour
{
    ChrController _chr;

    private void Awake()
    {
        _chr = GetComponent<ChrController>();
        if (_chr == null)
        {
            this.enabled = false;
        }
    }

    void Start()
    {
        if (_chr.chrControllerType != ChrController.ChrControllerTypes.Player)
        {
            this.enabled = false;
        }

        if (isLocalPlayer)
        {
            CameraManager.Instance._target = gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_chr != null)
        {
#if UNITY_STANDALONE || UNITY_WEBPLAYER
            _chr._h = Input.GetAxis("Horizontal");
            _chr._v = Input.GetAxis("Vertical");
            _chr._pAction = Input.GetButton("Fire1");
            _chr._sAction = Input.GetButton("Fire2");
#elif UNITY_ANDROID
            _chr._h = CrossPlatformInputManager.GetAxis("Horizontal");
            _chr._v = CrossPlatformInputManager.GetAxis("Vertical");
            _chr._pAction = CrossPlatformInputManager.GetButton("AButton");
            _chr._sAction = CrossPlatformInputManager.GetButton("BButton");
#endif
        }
    }
}
