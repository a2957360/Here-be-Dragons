using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public GameObject _target;

    float _z;
    Vector2 _offset;

    float _findCoolDown = 2.0f;
    float _findTimer = 1.5f;

    // camera size depending on screen size and aspect ratio
    float _cameraHeight;
    float _cameraWidth;

    Vector2 _targetPos;
    Vector2 _camPos;

    public float _mapWidth;
    public float _mapHeight;

    float _mapCenterX;
    float _mapCenterY;

    float _cameraUpperY;
    float _cameraLowerY;
    float _cameraUpperX;
    float _cameraLowerX;

    // Use this for initialization
    void Start()
    {
        _z = transform.position.z;
        _offset.Set(0.0f, 0.0f);

        _cameraHeight = GetComponent<Camera>().orthographicSize * 2;
        _cameraWidth = _cameraHeight * ((float)Screen.width / (float)Screen.height);
        _mapWidth = GameManager.Instance._mapWidth + 2;
        _mapHeight = GameManager.Instance._mapHeight + 2;
        _mapCenterX = GameManager.Instance._mapCenterX;
        _mapCenterY = GameManager.Instance._mapCenterY;
        _cameraUpperY = _mapCenterY + _mapHeight * 0.5f - _cameraHeight * 0.5f;
        _cameraLowerY = _mapCenterY - _mapHeight * 0.5f + _cameraHeight * 0.5f;
        _cameraUpperX = _mapCenterX + _mapWidth * 0.5f - _cameraWidth * 0.5f;
        _cameraLowerX = _mapCenterX - _mapWidth * 0.5f + _cameraWidth * 0.5f;

        //Debug.Log("Camera Height : " + _cameraHeight);
        //Debug.Log("Camera Width : " + _cameraWidth);
        //Debug.Log("Camera Upper Y : " + _cameraUpperY);
        //Debug.Log("Camera Lower Y : " + _cameraLowerY);
        //Debug.Log("Camera Upper X : " + _cameraUpperX);
        //Debug.Log("Camera Lower X : " + _cameraLowerX);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (_target != null && _target.activeInHierarchy)
        {
            _targetPos = _target.transform.position;
            _camPos = transform.position;

            _targetPos.x = Mathf.Clamp(_targetPos.x, _cameraLowerX, _cameraUpperX);
            _targetPos.y = Mathf.Clamp(_targetPos.y, _cameraLowerY, _cameraUpperY);

            // Hysteresis
            _camPos += 5.0f * (_targetPos - _camPos - _offset) * Time.deltaTime;
            transform.position = new Vector3(_camPos.x, _camPos.y, _z);
        }
        else if (_findTimer >= _findCoolDown)
        {
            _target = GameObject.FindGameObjectWithTag("Player");
            _findTimer = 0;
        }
        else
        {
            _findTimer += Time.deltaTime;
        }
    }
}
