using UnityEngine;

/// <summary>
/// Rotates the player's arm sprites toward the carried object when holding one,
/// and returns them to their rest angles when idle.
/// Attach to the player root alongside PickupSystem.
/// </summary>
public class PlayerArms : MonoBehaviour
{
    [Header("Arm Transforms")]
    [Tooltip("Pivot transform of the left arm sprite.")]
    [SerializeField] private Transform leftArm;
    [Tooltip("Pivot transform of the right arm sprite.")]
    [SerializeField] private Transform rightArm;

    [Header("Rest Angles (local Z)")]
    [SerializeField] private float leftArmRestAngle = 0f;
    [SerializeField] private float rightArmRestAngle = 0f;

    [Header("Hold Angles (local Z offset applied on top of look direction)")]
    [SerializeField] private float leftArmHoldOffset = 0f;
    [SerializeField] private float rightArmHoldOffset = 0f;

    [Header("Smoothing")]
    [SerializeField] private float rotationSpeed = 12f;

    private PickupSystem _pickup;

    private void Awake()
    {
        _pickup = GetComponent<PickupSystem>();
    }

    // LateUpdate so we always run after PickupSystem has positioned the held object
    private void LateUpdate()
    {
        if (leftArm != null)
            UpdateArm(leftArm, leftArmRestAngle, leftArmHoldOffset);

        if (rightArm != null)
            UpdateArm(rightArm, rightArmRestAngle, rightArmHoldOffset);
    }

    private void UpdateArm(Transform arm, float restAngle, float holdOffset)
    {
        Quaternion targetRotation;

        Transform carried = _pickup != null ? _pickup.CarriedObjectTransform : null;

        if (carried != null)
        {
            // World-space angle from this arm's pivot toward the held object
            Vector2 dir = (Vector2)carried.position - (Vector2)arm.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + holdOffset;
            targetRotation = Quaternion.Euler(0f, 0f, angle);

            // Smoothly rotate in world space
            arm.rotation = Quaternion.Lerp(arm.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly return to rest local rotation
            targetRotation = Quaternion.Euler(0f, 0f, restAngle);
            arm.localRotation = Quaternion.Lerp(arm.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}