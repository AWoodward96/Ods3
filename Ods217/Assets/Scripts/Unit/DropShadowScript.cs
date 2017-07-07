using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a script that is placed on almost every unit
/// It's a drop shadow, and it should raycast to the ground
/// </summary>
public class DropShadowScript : MonoBehaviour
{
    public Vector3 Center;

    // Update is called once per frame
    void Update()
    {
        // Set this objects position to the closest position on the ground
        RaycastHit hit;
        Ray r = new Ray(transform.parent.position + (Vector3.up), Vector3.down);
        Vector3 pos = transform.position;
        if (Physics.Raycast(r, out hit, 50, LayerMask.GetMask("Ground")))
        {
            pos = Vector3.Lerp(transform.position, hit.point + Vector3.up / 5, .98f);
        }
        transform.position = new Vector3(transform.parent.position.x, pos.y, transform.parent.position.z) + Center;
    }
}
