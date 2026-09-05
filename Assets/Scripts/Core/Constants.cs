/// <summary>
/// Every string the game looks something up by. Keeping them here means a rename is one edit and
/// a typo is a compile error instead of a silent failure at runtime.
/// </summary>
public static class Constants
{
    // Tags
    public const string PlayerTag       = "Player";
    public const string ChickenTag      = "Chicken";
    public const string PlayerBulletTag = "PlayerBullet";
    public const string EggTag          = "Egg";
    public const string PickupTag       = "Pickup";

    // Input System - action map and action names, as they appear in InputSystem_Actions
    public const string PlayerActionMap = "Player";
    public const string MoveAction      = "Move";
    public const string FireAction      = "Attack";
    public const string RestartAction   = "Interact";

    // PlayerPrefs keys
    public const string HighScoreKey = "HighScore";

    // Animator parameters
    public const string ThrustParam = "Thrust";
}
