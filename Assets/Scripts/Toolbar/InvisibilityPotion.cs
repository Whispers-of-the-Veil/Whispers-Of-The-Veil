using UnityEngine;
using Characters.Player;

public class InvisibilityPotion : InventoryItemBase
{
    public float duration = 5f;
    public float holdDuration = 2f;
    public float bounceAmplitude = 0.1f;
    public float bounceSpeed = 5f;
    public AudioClip consumeSound;

    private Transform lastParent;
    private PlayerStats playerStats;

    private float holdTimer = 0f;
    private bool isConsuming = false;
    private Vector3 originalLocalPosition;
    
    private AudioSource audioSource;

    public override string Name => "Invisibility Potion";
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (transform.parent != null && transform.parent != lastParent)
        {
            lastParent = transform.parent;
            playerStats = transform.GetComponentInParent<PlayerStats>();

            originalLocalPosition = transform.localPosition;
        }

        bool isHeldByPlayer = playerStats != null && transform.IsChildOf(playerStats.transform);

        if (isHeldByPlayer)
        {
            if (Input.GetMouseButton(0))
            {
                holdTimer += Time.deltaTime;

                if (!isConsuming)
                {
                    isConsuming = true;
                    
                    if (consumeSound != null && audioSource != null)
                    {
                        audioSource.clip = consumeSound;
                        audioSource.Play();
                    }
                }

                float bounceOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
                transform.localPosition = originalLocalPosition + Vector3.up * bounceOffset;

                if (holdTimer >= holdDuration)
                {
                    Debug.Log("Player consumed Invisibility Potion");

                    playerStats.SetInvisibility(true, duration);
                    Destroy(gameObject);
                }
            }
            else
            {
                holdTimer = 0f;
                isConsuming = false;
                transform.localPosition = originalLocalPosition;
                
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
        else
        {
            holdTimer = 0f;
            isConsuming = false;
        }
    }
}