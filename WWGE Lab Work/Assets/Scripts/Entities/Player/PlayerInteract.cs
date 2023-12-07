using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Transform _mainTransform;
    [SerializeField] private Camera _camera;

    [Space(5)]

    [SerializeField] private LayerMask _interactionMask;
    [SerializeField] private float _interactionUpdateDelay = 0.05f;
    private SphereCollider _collider;

    
    private IInteractable _currentInteractable;

    private Coroutine _interactionDetectionCoroutine;


    private void Awake() => _collider = GetComponent<SphereCollider>();
    private void OnEnable()
    {
        if (_interactionDetectionCoroutine == null)
            _interactionDetectionCoroutine = StartCoroutine(DetectCurrentInteractable());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            Interact();
    }


    private void Interact()
    {
        if (_currentInteractable != null)
            _currentInteractable.Interact(_mainTransform);
    }

    // Get the interactable currently in the player's view.
    private IEnumerator DetectCurrentInteractable()
    {
        while (true)
        {
            if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, _collider.radius, _interactionMask))
            {
                if (hit.transform.TryGetComponent<IInteractable>(out IInteractable interactable))
                {
                    _currentInteractable = interactable;
                    _currentInteractable.Focused(_mainTransform);
                }
            }
            else if (_currentInteractable != null)
            {
                _currentInteractable.Unfocused(_mainTransform);
                _currentInteractable = null;
            }

            yield return new WaitForSeconds(_interactionUpdateDelay);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if ((_interactionMask & (1 << other.gameObject.layer)) == 0)
            return;
        
        if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactable.InRange(_mainTransform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if ((_interactionMask & (1 << other.gameObject.layer)) == 0)
            return;

        if (other.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactable.OutOfRange(_mainTransform);
        }
    }
}
