using UnityEngine;

[CreateAssetMenu(fileName = "RockConfig", menuName = "Configs/RockConfig")]
public class RockConfig : ScriptableObject
{
    [Tooltip("Time (s) before rock auto-destroys after landing")]
    public float destroyAfterSeconds = 5f;

    [Tooltip("Radius that attracts enemies (how far they'll notice a thrown rock)")]
    public float attractRadius = 8f;

    [Tooltip("Damage to enemy if rock directly hits (optional)")]
    public int damageToEnemy = 0;
}
