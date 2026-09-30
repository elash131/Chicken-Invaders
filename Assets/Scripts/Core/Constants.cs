/// <summary>
/// Shared lookup names. Action and layer values must also match their Unity assets.
/// </summary>
public static class Constants
{
    // Tags
    public const string PlayerTag       = "Player";
    public const string ChickenTag      = "Chicken";
    public const string PlayerBulletTag = "PlayerBullet";
    public const string EggTag          = "Egg";
    public const string PickupTag       = "Pickup";

    // Physics layers
    public const string PlayerLayer          = "Player";
    public const string PlayerProjectileLayer = "PlayerProjectile";
    public const string EnemyLayer           = "Enemy";

    // Input System - action map and action names, as they appear in InputSystem_Actions
    public const string PlayerActionMap = "Player";
    public const string MoveAction      = "Move";
    public const string FireAction      = "Fire";
    public const string RestartAction   = "Restart";
    public const string PauseAction     = "Pause";

    // PlayerPrefs keys
    public const string HighScoreKey = "HighScore";

    // Animator parameters
    public const string ThrustParam = "Thrust";
}
