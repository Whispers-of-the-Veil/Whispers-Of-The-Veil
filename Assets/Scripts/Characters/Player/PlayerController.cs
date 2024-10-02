using UnityEngine;

namespace Characters.Player {
    public class PlayerController : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] public float speed;
        private Vector2 movementVector;
        private float movementX, movementZ;
        private Vector3 movement, direction, velocity;
        
        [Header("Components")]
        private Rigidbody rb;
    
        // Start is called before the first frame update
        private void Start() {
            this.rb = GetComponent<Rigidbody>();
    
            Cursor.lockState = CursorLockMode.Locked;
        }
    
        // Update is called once per frame
        private void Update() {
            OnMove();
        }
    
        private void OnMove() {
            this.movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    
            this.movementX = this.movementVector.x;
            this.movementZ = this.movementVector.y;
        }
    
        // Updated at a fixed framerate; used for physics calculations
        private void FixedUpdate() {
            movement = new Vector3(this.movementX, 0, this.movementZ);
            direction = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
    
            direction.Normalize();
            velocity = direction * speed;
    
            this.rb.AddForce(movement + velocity);
        }
    }
}

