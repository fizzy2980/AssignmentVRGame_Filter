using UnityEngine;

public class Pistol : GunBase
{
   
    public int CurrentAmmo;
    public int Mag;
    protected override void Start()
    {
        base.Start();
        InitilizeBullet();
    }

    private void InitilizeBullet(){
        gunData.currentAmmo = CurrentAmmo;
        gunData.magazineSize = Mag;
    }
    public override void Shoot()
    {
        Debug.Log("Pistol shot fired!");
        base.Shoot();
    }
}
