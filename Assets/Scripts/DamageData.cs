using UnityEngine;

public enum DamageType 
{ 
    Physical, 
    Fire, 
    Toxic, 
    Electric, 
    Frost 
}

[System.Serializable]
public struct DamagePayload
{
    public float amount;
    public DamageType type;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}

public interface IDamageable
{
    void TakeDamage(DamagePayload payload);
}