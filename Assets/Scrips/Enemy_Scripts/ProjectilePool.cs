using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 8;

    private readonly List<HomingProjectile> _pool = new();

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[ProjectilePool] Projectile prefab is not assigned.", this);
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.name = $"Projectile_{i}";
            obj.SetActive(false);

            HomingProjectile proj = obj.GetComponent<HomingProjectile>();
            if (proj == null)
            {
                Debug.LogError("[ProjectilePool] Projectile prefab is missing HomingProjectile component.", this);
                return;
            }

            proj.Initialize(this);
            _pool.Add(proj);
        }
    }

    /// <summary>Returns the first inactive projectile, or null if all are in use.</summary>
    public HomingProjectile GetProjectile()
    {
        foreach (HomingProjectile proj in _pool)
        {
            if (!proj.gameObject.activeSelf)
                return proj;
        }
        return null;
    }

    /// <summary>Disables the projectile and re-parents it back to this pool.</summary>
    public void ReturnProjectile(HomingProjectile proj)
    {
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }
}