using Dialogue;
using UnityEngine;

namespace Characters.Player {
    public class PlayerController : MonoBehaviour
    {
        private bool hasKey = false;
        private GameObject heldKey;
        private bool isHoldingBook = false;
        private GameObject heldBook;
        [Header("Movement")]
        [SerializeField] public float speed;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;  // Sprint speed multiplier
        private Vector2 _movementVector;
        private float _movementX, _movementZ;
        private Vector3 _movement, _direction, _velocity;
        private bool _isSprinting;

        [Header("Components")]
        private Rigidbody _rb;
        
        [Header("Inventory")]
        public Inventory inventory;
        
        [Header("Pickup Settings")]
        [SerializeField] private Transform holdPoint;  
        [SerializeField] private float pickupRange = .25f;  
        private GameObject _heldObject;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueUI dialogueUI;

        public DialogueUI DialogueUI => dialogueUI;
        
        public IInteractable Interactable { get; set; }
        
        // Start is called before the first frame update
        private void Start() {
            this._rb = GetComponent<Rigidbody>();
            inventory.ItemUsed += Inventory_ItemUsed; 
            //Cursor.lockState = CursorLockMode.Locked;
        }

        private void Inventory_ItemUsed(object sender, InventoryEventArgs e)
        {
            IInventoryItem item = e.Item;

            GameObject goItem = (item as MonoBehaviour).gameObject;
            goItem.SetActive(true);
            
            goItem.transform.parent = holdPoint.transform;
            goItem.transform.position = holdPoint.position;
        }
    
        // Update is called once per frame
        private void Update() {
            if (DialogueUI.IsOpen) return;
            OnMove();
            CheckForPickup();
            CheckForSprint();
            CheckForInventoryAdd();
            CheckForChestInteraction();
            CheckForBookInteraction();
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
                
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRange);
                
                foreach (var hitCollider in hitColliders) 
                {
                    if (hitCollider.CompareTag("Pickup") && hitCollider.isTrigger) {
                        Key key = hitCollider.GetComponent<Key>();
                        hasKey = true;
                        heldKey = hitCollider.gameObject;
                        
                        Book book = hitCollider.GetComponent<Book>();
                        if (book != null)
                        {
                            isHoldingBook = true;
                            heldBook = hitCollider.gameObject;
                        }
                        
                        PickupObject(hitCollider.gameObject);
                        break;  
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
            Book book = target.GetComponent<Book>();
            if (book != null)
            {
                isHoldingBook = false;
            }
        }

        private void CheckForInventoryAdd()
        {
            if (_heldObject != null && Input.GetKeyDown(KeyCode.Q))
            {
                IInventoryItem item = _heldObject.GetComponent<IInventoryItem>();
                if (item != null)
                {
                    inventory.AddItem(item);
                    
                    Book book = _heldObject.GetComponent<Book>();
                    if (book != null) { // 
                        isHoldingBook = false; 
                    }
                    
                }
            }
        }
        
        private void CheckForChestInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                // Check for objects within interaction range
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, (float).25);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Chest"))
                    {
                        Chest chest = hitCollider.GetComponent<Chest>();
                        if (chest != null)
                        {
                            if (hasKey)
                            {
                                chest.Open(heldKey);
                                hasKey = false; // Use the key
                            }
                            else
                            {
                                Debug.Log("You need a key to open this chest!");
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        private void CheckForBookInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F) && isHoldingBook && heldBook != null)
            {
                // Access the Book script and toggle the book UI
                Book bookScript = heldBook.GetComponent<Book>();
                if (bookScript != null)
                {
                    bookScript.OpenBook();
                }
                else
                {
                    Debug.LogError("No Book script attached to the held book!");
                }
            }
        }
        
        




        
        
    
    }
}