using UnityEngine;

public class FootstepRelay : MonoBehaviour
{
    private FootstepSurface _footstepSurface;

    private void Awake()
    {
        _footstepSurface = GetComponentInParent<FootstepSurface>();
    }

    // Called by Animation Event
    public void TriggerFootstep()
    {
        _footstepSurface?.TriggerFootstep();
    }
}