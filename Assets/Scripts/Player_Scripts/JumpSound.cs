using UnityEngine;

public class JumpSound : MonoBehaviour
{
    public void Jump()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");
    }
}