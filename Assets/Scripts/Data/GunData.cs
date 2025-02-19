using UnityEngine;

[CreateAssetMenu(fileName = "NewGunData", menuName = "Guns/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;
    public GameObject bulletPrefab;
    public int magazineSize;
    public int currentAmmo;
    public float bulletSpeed;
}
