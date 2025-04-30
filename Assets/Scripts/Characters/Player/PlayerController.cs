using Audio.SFX;
using Characters.NPC;
using Dialogue;
using UnityEngine;
using Environment;
using Combat;
using Characters.Player.Speech;
using System.Collections;
using FunkyCode.Utilities;
using UnityEngine.UIElements;

namespace Characters.Player {
    public class PlayerController : MonoBehaviour
    {
        public bool hasKey = false;
        private GameObject heldKey;
        private bool isHoldingBook = false;
        private GameObject heldBook;

        //for animation
        Animator anim;
        private Vector2 lastMoveDirection;
        private bool facingLeft = true;
        
        [Header("Audio")] 
        [SerializeField] AudioClip weaponSwingSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        public SoundExpert soundExpert {
            get => SoundExpert.instance;
        }
        
        [Header("Movement")]
        [SerializeField] public float speed = 3f;
        [SerializeField] public float rainingSpeed = 2.5f;
        [SerializeField] float moveLimiter = 0.7f;
        [SerializeField] private float sprintSpeedMultiplier = 1.5f;  // Sprint speed multiplier
        [SerializeField] RainController weather;
        [SerializeField] private float jumpForce = 5f;
        private float horizontal;
        private float vertical;
        private bool _isSprinting;
        private GameObject _heldObject;
        private bool isJumping = false;

        [Header("Components")]
        private Rigidbody2D body;

        public Inventory inventory
        {
            get => Inventory.instance;
        }
        
        [Header("Pickup Settings")]
        [SerializeField] private Transform holdPoint;  
        [SerializeField] private float pickupRange = .25f;  
        [SerializeField] private float dropDistance = 0.25f;
        
        [Header("Dialogue")]
        [SerializeField] private DialogueUI dialogueUI;

        private Voice voice;

        public DialogueUI DialogueUI => dialogueUI;
        private bool isFrozen = false;

        
        public IInteractable Interactable { get; set; }
        
        // Start is called before the first frame update
        private void Start() {
            if (dialogueUI == null) {
                dialogueUI = GameObject.Find("Canvas").GetComponent<DialogueUI>();
            }
            if (weather == null) {
                weather = GameObject.Find("Weather").GetComponent<RainController>();
            }
            
            body = GetComponent<Rigidbody2D>();
            inventory.ItemUsed += Inventory_ItemUsed; 
            //Cursor.lockState = CursorLockMode.Locked;

            //animation
            anim = GetComponent<Animator>();
            
            voice = gameObject.GetComponent<Voice>();
        }

        private void Inventory_ItemUsed(object sender, InventoryEventArgs e)
        {
            IInventoryItem item = e.Item;
            GameObject goItem = (item as MonoBehaviour).gameObject;

            goItem.SetActive(true);

            var pickupComp = goItem.GetComponent<PickUpObject>();
            if (pickupComp != null)
            {
                pickupComp.PickUp(holdPoint);
            }
            else
            {
                goItem.transform.parent   = holdPoint;
                goItem.transform.position = holdPoint.position;
            }

            Book bookFromInventory = goItem.GetComponent<Book>();
            if (bookFromInventory != null)
            {
                bookFromInventory.OnPickedUp();
                isHoldingBook = true;
                heldBook     = goItem;
            }
            
        }
    
        // Update is called once per frame
        private void Update()
        {
            if (!voice.displayBubble) {
                checkFrozen();
                OnMove();
                CheckForPickup();
                CheckForSprint();
                CheckForInventoryAdd();
                CheckForChestInteraction();
                CheckForBookInteraction();
                CheckForDialogue();
                CheckForAttack();
                UpdateHoldPointPosition();
        
    
                Animate();
            }
        }
    
        private void OnMove()
        {
         
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            // If there's movement, store the last direction
            if (horizontal != 0 || vertical != 0)
            {
                lastMoveDirection = new Vector2(horizontal, vertical).normalized;
            }
        }

        // Animation function
        private void Animate()
        {
            anim.SetFloat("MoveX", horizontal);
            anim.SetFloat("MoveY", vertical);
            anim.SetFloat("MoveMagnitude", new Vector2(horizontal, vertical).magnitude);
            anim.SetFloat("LastMoveX", lastMoveDirection.x);
            anim.SetFloat("LastMoveY", lastMoveDirection.y);
            anim.SetBool("isJumping", isJumping);
        }

        private void CheckForSprint() {
            // Check if the Shift key is held down to enable sprinting
            _isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
 
        
        // Updated at a fixed framerate; used for physics calculations
        private void FixedUpdate() {
            if (isFrozen)
            {
                body.velocity = Vector2.zero;
                return;
            }

            float tmpSpeed;

            Vector2 input = new Vector2(horizontal, vertical);

      
            if (horizontal != 0 && vertical != 0)
            {
                input *= moveLimiter;
            }

            if (weather != null && weather.isRaining) {
                tmpSpeed = _isSprinting ? rainingSpeed * sprintSpeedMultiplier : rainingSpeed;
            } else {
                tmpSpeed = _isSprinting ? speed * sprintSpeedMultiplier : speed;
            }

            //Adjusted so that player moves diagonally in the same speed
            body.velocity = input * tmpSpeed;
        }

        private void CheckForPickup() {
            if (_heldObject == null && Input.GetKeyDown(KeyCode.E)) {
                
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pickupRange);

                foreach (var hitCollider in hitColliders) 
                {
                    
                    if(hitCollider.gameObject == this.gameObject)
                        continue;

                    if (hitCollider.CompareTag("Pickup") && hitCollider.isTrigger) {
                        Key keyScript = hitCollider.GetComponent<Key>();
                        if (keyScript != null) {
                            keyScript.OnPickedUp();                  
                            DontDestroyOnLoad(hitCollider.gameObject);       
                            hasKey = true;
                            heldKey = hitCollider.gameObject;
                        }
                        
                        Book bookInWorld = hitCollider.GetComponent<Book>();
                        if (bookInWorld != null)
                        {
                            isHoldingBook = true;
                            heldBook = hitCollider.gameObject;
                            bookInWorld.OnPickedUp();
                        }
                        
                        PickupObject(hitCollider.gameObject);
                        break;  
                    }
                }
            } else if (_heldObject != null && Input.GetKeyDown(KeyCode.R)) {
                Book bookBeingHeld = _heldObject.GetComponent<Book>();
                if (bookBeingHeld != null)
                    bookBeingHeld.OnDropped();
                DropObject();
                
            }
        }

        public void PickupObject(GameObject obj) {
            _heldObject = obj;
            
            
            PickUpObject target = obj.GetComponent<PickUpObject>();
            target.PickUp(holdPoint);
            
            MeleeWeapon weapon = obj.GetComponent<MeleeWeapon>();
            if (weapon != null) {
                weapon.EquipWeapon();
            }
            
        }

        private void DropObject() {
            if (_heldObject != null) {
                Vector3 dropPosition = holdPoint.position + new Vector3(0, -dropDistance, 0);
                
                PickUpObject target = _heldObject.GetComponent<PickUpObject>();
                if (target != null) {
                    target.Drop();
                }
                
                MeleeWeapon weapon = _heldObject.GetComponent<MeleeWeapon>();
                if (weapon != null) {
                    weapon.UnequipWeapon();
                }

                _heldObject.transform.parent = null;
                _heldObject.transform.position = dropPosition;
                _heldObject = null;
            }
        }

        private void CheckForInventoryAdd() {
            if (_heldObject != null && Input.GetKeyDown(KeyCode.Q)) {
                
                Book heldBookScript = _heldObject.GetComponent<Book>();
                if (heldBookScript != null && heldBookScript.IsBookOpen)
                {
                    heldBookScript.OpenBook(); 
                }
                
                IInventoryItem item = _heldObject.GetComponent<IInventoryItem>();
                if (item != null)
                {

                    inventory.AddItem(item);
                    
                    Book book = _heldObject.GetComponent<Book>();
                    if (book != null)
                    {
                        book.OnDropped();     
                    }
                    
                    MeleeWeapon weapon = _heldObject.GetComponent<MeleeWeapon>();
                    if (weapon != null)
                    {
                        weapon.UnequipWeapon();
                    }

                    _heldObject.SetActive(false);

                    if (book != null) isHoldingBook = false;
                    _heldObject = null;
                }
            }
        }
        
        private void CheckForChestInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, (float).25);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.CompareTag("Chest"))
                    {
                        Chest chest = hitCollider.GetComponent<Chest>();
                        if (chest != null)
                        {
                            if (hasKey)
                            {
                                chest.Open(transform);
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
        private void CheckForDialogue()
        {
            if (Input.GetKeyDown(KeyCode.U) && !dialogueUI.IsOpen)
            {
                if (Interactable != null)
                {
                    Interactable.Interact(this);
                }
            }
        }

        private void CheckForAttack()
        {
            if (_heldObject != null && Input.GetMouseButtonDown(0)) // Left click to attack
            {
                MeleeWeapon weapon = _heldObject.GetComponent<MeleeWeapon>();
                if (weapon != null)
                {
                    weapon.Attack();
                }
            }
        }
        
        private void checkFrozen()
        {
            if (DialogueUI.IsOpen) {
                isFrozen = true;  // Stop movement when dialogue starts
                return;
            } 
            isFrozen = false;
        }
        
        public void SetHeldObject(GameObject obj)
        {
            _heldObject = obj;
        }
        
        public GameObject GetHeldObject()
        {
            return _heldObject;
        }
        
                private void UpdateHoldPointPosition()
        {
            if (lastMoveDirection == Vector2.zero)
                return;

            Vector2 offset = Vector2.zero;
            string sortingLayer = "Pickup";
            bool flipX = false;

            if (lastMoveDirection.x > 0 && lastMoveDirection.y > 0)
            {
                offset = new Vector2(0.2f, -0.1f);
                sortingLayer = "BehindPlayer";
            }
            else if (lastMoveDirection.x < 0 && lastMoveDirection.y > 0)
            {
                offset = new Vector2(-0.18f, -0.1f);
                sortingLayer = "BehindPlayer";
            }
            else if (lastMoveDirection.x > 0 && lastMoveDirection.y < 0)
            {
                offset = new Vector2(0.1f, -0.2f);
                sortingLayer = "Pickup";
            }
            else if (lastMoveDirection.x < 0 && lastMoveDirection.y < 0)
            {
                offset = new Vector2(-0.21f, -0.15f);
                sortingLayer = "Pickup";
            }
            else if (lastMoveDirection.y > 0.5f)
            {
                offset = new Vector2(0.16f, -0.03f);
                sortingLayer = "BehindPlayer";
            }
            else if (lastMoveDirection.y < -0.6f)
            {
                offset = new Vector2(-0.08f, -0.14f);
                sortingLayer = "Pickup";
            }
            else if (lastMoveDirection.x > 0.5f)
            {
                offset = new Vector2(0.2f, -0.05f);
            }
            else if (lastMoveDirection.x < -0.5f)
            {
                offset = new Vector2(-0.15f, -0.06f);
            }

            holdPoint.localPosition = offset;

            if (_heldObject != null)
            {
                SpriteRenderer childRenderer = _heldObject.GetComponentInChildren<SpriteRenderer>();
                if (childRenderer != null)
                {
                    childRenderer.flipX = lastMoveDirection.x < 0;

                    childRenderer.sortingLayerName = sortingLayer;
                }
            }
        }

    }
}