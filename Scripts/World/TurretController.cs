using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shotsPerSecond = 3f;

    private float fireTimer;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = 1f / shotsPerSecond;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        bulletController b = bullet.GetComponent<bulletController>();
        b.moveDir = firePoint.right; // shoots in turret direction
    }
}

//////////// IMPORTANT //////////////////////////////////////
///  RIGHT NOW, TURRET IS NOT DOING DAMAGE TO PLAYER ////////
/// /////////////////////////////////////////////////////////