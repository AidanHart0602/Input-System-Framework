using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        private PlayerInputs _input;
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;
        private bool _correctZoneID = false;
        private List<Rigidbody> _brakeOff = new List<Rigidbody>();
        [SerializeField]
        private int _pieceCount;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;

        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {

            if (zone.GetZoneID() == 6) //Crate zone            
            {
                _correctZoneID = true;
                if (_brakeOff.Count > 0)
                {
                    StartCoroutine(PunchDelay());
                }

                if (_isReadyToBreak == false && _brakeOff.Count > 0)
                {
                    _wholeCrate.SetActive(false);
                    _brokenCrate.SetActive(true);
                    _isReadyToBreak = true;
                }
            }
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
            _input = new PlayerInputs();
            _input.Crate.Enable();
            _input.Crate.Punch.performed += Punch_performed;
            _input.Crate.Punch.canceled += Punch_canceled;
        }

        private void Punch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (_isReadyToBreak && _correctZoneID == true && _brakeOff.Count > 4)
            {
                Debug.Log("Charged");
                int rng = Random.Range(4, _brakeOff.Count);
                _brakeOff[rng].constraints = RigidbodyConstraints.None;
                _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff [rng]);
            }
            else if(_isReadyToBreak && _correctZoneID == true)
            {
                Debug.Log("Reduced Charge");
                int rng = Random.Range(0, _brakeOff.Count);
                _brakeOff[rng].constraints = RigidbodyConstraints.None;
                _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff[rng]);
            }
        }

        private void Punch_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (_isReadyToBreak == true && _correctZoneID == true)
            {
                Debug.Log("Not fully charged");
                int rng = Random.Range(0, _brakeOff.Count);
                _brakeOff[rng].constraints = RigidbodyConstraints.None;
                _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff[rng]);
            }
        }


        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }
        private void Update()
        {
            _pieceCount = _brakeOff.Count;

            if (_brakeOff.Count == 0)
            {
                _isReadyToBreak = false;
                _crateCollider.enabled = false;
                _interactableZone.CompleteTask(6);
                Debug.Log("Completely Busted");
            }
        }
        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete; _input.Crate.Enable();
            _input.Crate.Punch.performed -= Punch_performed;
            _input.Crate.Punch.canceled -= Punch_canceled;
        }
    }
}
