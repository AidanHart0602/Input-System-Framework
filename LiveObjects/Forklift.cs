using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        private PlayerInputs _input;
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void Start()
        {
            _input = new PlayerInputs();
            _input.Forklift.Escape.performed += Escape_performed;
        }

        private void Escape_performed(InputAction.CallbackContext context)
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            _input.Player.Enable();
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _input.Player.Disable();
                _input.Forklift.Enable();
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
            }
        }

        private void CalcutateMovement()
        {
            var movement = _input.Forklift.Movement.ReadValue<Vector2>();
            var direction = new Vector3(0, 0, movement.y);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(movement.y) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += movement.x * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            var forkValue = _input.Forklift.ForkControls.ReadValue<float>();
            if (forkValue == 1)
            {
                LiftUpRoutine();
            }

            if (forkValue == -1)
            {
                LiftDownRoutine();
            }
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }

    }
}