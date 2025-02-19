using Oculus.Interaction;
using TMPro;
using UnityEngine;

public abstract class GunBase : MonoBehaviour
{
    [SerializeField] public GunData gunData;
    [SerializeField] protected Transform spawnPoint;
    [SerializeField] protected AudioTrigger gunSound;
    [SerializeField] protected AudioTrigger emptySound;
    [SerializeField] protected TMP_Text magText;
    [SerializeField] protected TMP_Text bulletText;
    [SerializeField] protected float bulletLifeSpan;

    protected virtual void Start()
    {
        gunData.currentAmmo = gunData.magazineSize;
        UpdateUI();
    }

    public virtual void Shoot()
    {
        if (gunData.currentAmmo <= 0)
        {
            emptySound.PlayAudio();
            Debug.LogWarning($"{gunData.gunName} is out of ammo!");
            return;
        }

        if (gunData.bulletPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Bullet prefab or spawn point is not assigned!");
            return;
        }

        gunData.currentAmmo--;
        UpdateUI();

        GameObject bullet = Instantiate(gunData.bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = spawnPoint.forward * gunData.bulletSpeed;
        }

        gunSound.PlayAudio();

        if (gunData.currentAmmo <= 0)
        {
            Reload();
        }
        Destroy(bullet , bulletLifeSpan);
    }

    public virtual void Reload()
    {
        int ammoToReload = Mathf.Min(gunData.magazineSize, 6);
        gunData.currentAmmo = ammoToReload;
        gunData.magazineSize -= ammoToReload;
        Debug.Log($"{gunData.gunName} reloaded!");

        UpdateUI();
    }

    protected void UpdateUI()
    {
        bulletText.text = $"Bullet: {gunData.currentAmmo}";
        magText.text = $"Mag: {gunData.magazineSize}";
    }
}
