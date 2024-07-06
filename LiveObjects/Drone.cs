using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;
namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }
        private PlayerInputs _input;
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void Start()
        {
            _input = new PlayerInputs();
            _input.Drone.Escape.performed += Escape_performed;
        }

        private void Escape_performed(InputAction.CallbackContext obj)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _input.Drone.Enable();
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);

        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            var rotate = _input.Drone.Rotate.ReadValue<float>();
            var tempRot = transform.localRotation.eulerAngles;
            if (rotate == 1) 
            {
                tempRot.y -= _speed / 5;
                transform.localRotation = Quaternion.Euler(tempRot);
            }

            if(rotate == -1) 
            {
                tempRot.y += _speed / 5;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate()
        {
            var _movement = _input.Drone.Movement.ReadValue<Vector3>();
            if (_movement == Vector3.up)
            {
                Debug.Log("Going Up");
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (_movement == Vector3.down)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            var _movement = _input.Drone.Movement.ReadValue<Vector3>();
            switch (_movement)
            {
                case Vector3 m when m.Equals(Vector3.forward):
                    transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
                    break;
                case Vector3 m when m.Equals(Vector3.back):
                    transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
                    break;
                case Vector3 m when m.Equals(Vector3.left):
                    transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 30);
                    break;
                case Vector3 m when m.Equals(Vector3.right):
                    transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
                    break;
                default:
                    transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                    break;
            }  
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
