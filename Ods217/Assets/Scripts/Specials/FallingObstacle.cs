using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingObstacle : MonoBehaviour {

    public GameObject ExplosionPart;
    public GameObject DropShadow;
    public int Damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
            return;

        // Ignore the fall plane
        if (other.name.Contains("Safe Spawn"))
            return;

        ExplosionPart.SetActive(true);
        ExplosionPart.transform.parent = null;
        ExplosionPart.transform.position = transform.position;
        gameObject.SetActive(false);


        IDamageable dmg = other.GetComponent<IDamageable>();
        // Look for something to deal damage to
        if(dmg != null)
        {
            dmg.OnHit(Damage);
        } 
    }

    private void OnEnable()
    {
        ExplosionPart.SetActive(false);
        ExplosionPart.transform.parent = this.gameObject.transform;
        ExplosionPart.transform.localPosition = Vector3.zero;

        RaycastHit hit;
        Ray r = new Ray(DropShadow.transform.parent.position + (Vector3.up), Vector3.down);
        Vector3 pos = transform.position;
        if (Physics.Raycast(r, out hit, 50, LayerMask.GetMask("Ground")))
        {
            pos = Vector3.Lerp(transform.position, hit.point + Vector3.up / 5, .98f);
        }
        DropShadow.transform.position = pos;
    }
}
