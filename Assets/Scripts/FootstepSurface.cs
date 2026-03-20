using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Raycasts downward to detect the ground tag and fires FMOD footstep events
/// with a matching surface parameter. Call TriggerFootstep() from an animation event.
/// </summary>
public class FootstepSurface : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("FMOD")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference JumpEvent;
    [SerializeField] private EventReference HurtEvent;

    [Header("Surface Tags → FMOD Parameter Values")]
    [SerializeField] private SurfaceEntry[] surfaces;

    [System.Serializable]
    public struct SurfaceEntry
    {
        public string tag;
        [Tooltip("Value passed to the FMOD 'Surface' parameter.")]
        public float parameterValue;
    }

    private const string SurfaceParam = "Surface";
    private float _currentParamValue;

    private void Update()
    {
        DetectSurface();
    }

    private void DetectSurface()
    {
        Vector2 origin = groundCheck != null ? groundCheck.position : transform.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);

        if (!hit) return;

        string hitTag = hit.collider.tag;

        foreach (SurfaceEntry entry in surfaces)
        {
            if (entry.tag == hitTag)
            {
                _currentParamValue = entry.parameterValue;
                return;
            }
        }
    }

    /// <summary>Call this from an Animation Event on footstep frames.</summary>
    public void TriggerFootstep()
    {
        if (footstepEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);
        instance.setParameterByName(SurfaceParam, _currentParamValue);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Footstep");
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/CatStepB");
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Backpack");
        instance.release();

    }

    public void Jump()
    {
        if (JumpEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(JumpEvent);
        instance.setParameterByName(SurfaceParam, _currentParamValue);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");
        instance.release();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 origin = groundCheck != null ? groundCheck.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, Vector2.down * raycastDistance);
    }
#endif
}