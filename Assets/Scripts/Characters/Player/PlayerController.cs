using Dialogue;
using UnityEngine;

namespace Characters.Player {
    public class PlayerController : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] public float speed;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;  // Sprint speed multiplier
        private Vector2 _movementVector;
        private float _movementX, _movementZ;
        private Vector3 _movement, _direction, _velocity;
        private bool _isSprinting;

        [Header("Components")]
        private Rigidbody _rb;
        
        [Header("Pickup Settings")]
        [SerializeField] private Transform holdPoint;  
        [SerializeField] private float pickupRange = 2f;  
        private GameObject _heldObject;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueUI dialogueUI;

        public DialogueUI DialogueUI => dialogueUI;
        
        public IInteractable Interactable { get; set; }
        
        
        // Start is called before the first frame update
        private void Start() {
            this._rb = GetComponent<Rigidbody>();
            //Cursor.lockState = CursorLockMode.Locked; removed for mouse interaction on dialogue options
        }
    
        // Update is called once per frame
        private void Update() {
            if (DialogueUI.IsOpen) return;
            OnMove();
            CheckForPickup();
            CheckForSprint();

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (Interactable != null)
                {
                    Interactable.Interact(this);
                }
            }
        }
    
        private void OnMove() {
            this._movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            this._movementX = this._movementVector.x;
            this._movementZ = this._movementVector.y;
        }

        private void CheckForSprint() {
            // Check if the Shift key is held down to enable sprinting
            _isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    
        // Updated at a fixed framerate; used for physics calculations
        private void FixedUpdate() {
            _movement = new Vector3(this._movementX, 0, this._movementZ);
            _direction = transform.right * _movementX + transform.forward * _movementZ;
            _direction.Normalize();

            // Apply sprint multiplier if sprinting
            float currentSpeed = _isSprinting ? speed * sprintSpeedMultiplier : speed;
            _velocity = _direction * currentSpeed;

            // Set velocity directly instead of using AddForce to prevent unintended movement
            _rb.velocity = new Vector3(_velocity.x, _rb.velocity.y, _velocity.z);
        }

        private void CheckForPickup() {
            if (_heldObject == null && Input.GetKeyDown(KeyCode.E)) {
                // Check for objects within a spherical radius around the player
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRange);
                foreach (var hitCollider in hitColliders) {
                    if (hitCollider.CompareTag("Pickup") && hitCollider.isTrigger) {
                        PickupObject(hitCollider.gameObject);
                        break;  // Exit the loop after picking up the first object
                    }
                }
            } else if (_heldObject != null && Input.GetKeyDown(KeyCode.R)) {
                DropObject();
            }
        }

        private void PickupObject(GameObject obj) {
            _heldObject = obj;
            PickUpObject target = obj.GetComponent<PickUpObject>();
            target.PickUp(holdPoint);
        }

        private void DropObject() {
            PickUpObject target = _heldObject.GetComponent<PickUpObject>();
            target.Drop();
            _heldObject = null;
        }
    }
}
