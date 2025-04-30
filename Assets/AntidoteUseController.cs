using UnityEngine;
using UnityEngine.SceneManagement;
using Characters.Player;

public class AntidoteUseController : MonoBehaviour
{
    [Header("Hold‐to‐Use Settings")]
    [Tooltip("How long you must hold left‐click to consume")]
    public float holdDuration     = 2f;
    [Tooltip("Visual bounce height")]
    public float bounceAmplitude  = 0.1f;
    [Tooltip("Bounce speed multiplier")]
    public float bounceSpeed      = 5f;

    [Header("Boss Cutscene")]
    [Tooltip("Exact name of your Boss GameObject")]
    public string bossObjectName    = "Boss(Clone)";
    [Tooltip("How close you must be to the boss")]
    public float effectRadius       = 2f;
    [Tooltip("Name of the scene to load for the cutscene")]
    public string cutsceneSceneName;

    // internal state
    private PlayerStats playerStats;
    private Transform   playerTransform;
    private Vector3     originalLocalPos;
    private float       holdTimer;
    private bool        isConsuming;

    void Update()
    {
        // only run when this object is already held by the player
        if (transform.parent == null)
            return;

        // cache player references on first frame
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
            playerStats     = GetComponentInParent<PlayerStats>();
            originalLocalPos = transform.localPosition;
            holdTimer       = 0f;
            isConsuming     = false;
        }

        // require left‐click hold
        if (Input.GetMouseButton(0))
        {
            holdTimer += Time.deltaTime;
            isConsuming = true;

            // bounce effect
            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
            transform.localPosition = originalLocalPos + Vector3.up * bounce;

            if (holdTimer >= holdDuration)
                TryUseOnBoss();
        }
        else if (isConsuming)
        {
            // reset if released early
            ResetUse();
        }
    }

    private void TryUseOnBoss()
    {
        if (playerTransform == null) return;

        var bossGO = GameObject.Find(bossObjectName);
        if (bossGO != null)
        {
            float dist = Vector3.Distance(playerTransform.position, bossGO.transform.position);
            if (dist <= effectRadius)
            {
                // success: load cutscene
                SceneManager.LoadScene(cutsceneSceneName);
                Destroy(gameObject);
                return;
            }
        }

        // too far (or boss not found): reset so the player can try again
        ResetUse();
    }

    private void ResetUse()
    {
        holdTimer = 0f;
        isConsuming = false;
        transform.localPosition = originalLocalPos;
    }

    // visualize the effect radius around the player
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, effectRadius);
        }
    }
}
