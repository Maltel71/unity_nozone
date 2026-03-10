public interface IPickupSystem
{
    bool CanJump { get; }
    bool CanRun { get; }
    bool IsCarrying { get; }
}