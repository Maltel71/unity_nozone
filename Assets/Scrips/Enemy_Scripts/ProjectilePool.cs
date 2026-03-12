using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 8;

    [Header("Hold Point")]
    [Tooltip("Empty Transform inside the enemy where projectiles sit while idle. Assign the ProjectileHoldPoint here.")]
    [SerializeField] private Transform holdPoint;

    private readonly List<HomingProjectile> _pool = new();

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[ProjectilePool] Projectile prefab is not assigned.", this);
            return;
        }

        Transform spawnParent = holdPoint != null ? holdPoint : transform;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, spawnParent);
            obj.name = $"Projectile_{i}";
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
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

    /// <summary>Disables the projectile, re-parents it to the hold point, and resets its local transform.</summary>
    public void ReturnProjectile(HomingProjectile proj)
    {
        proj.gameObject.SetActive(false);

        Transform returnParent = holdPoint != null ? holdPoint : transform;
        proj.transform.SetParent(returnParent, false);
        proj.transform.localPosition = Vector3.zero;
        proj.transform.localRotation = Quaternion.identity;
    }
}