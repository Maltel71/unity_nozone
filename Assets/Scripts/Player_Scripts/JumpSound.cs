using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class JumpSound : MonoBehaviour
{
    [SerializeField] private EventReference JumpEvent;
    public void Jump()
    {
        if (JumpEvent.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(JumpEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");
        instance.release();
    }
}