﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeIfInZone : MonoBehaviour {

    public Vector2 Size;
    GameObject Player;

    SpriteRenderer myRenderer;
    Color baseColor;
    Color emptyColor;
    bool Inside;

    Vector3 topLeft;
    Vector3 bottomRight;

    // Use this for initialization
	void Start () {
        Player = GameObject.FindGameObjectWithTag("Player");
        myRenderer = GetComponent<SpriteRenderer>();

        // set up the colors
        baseColor = myRenderer.color;
        Color b = baseColor;
        b.a = 0;
        emptyColor = b; // The empty color should be the base color without an alpha


        topLeft = transform.position + new Vector3(-Size.x / 2, 0, Size.y / 2);
        bottomRight = transform.position + new Vector3(Size.x / 2, 0, -Size.y / 2);
    }

    // Update is called once per frame
    void Update() {
        myRenderer.color = Color.Lerp(myRenderer.color, (Inside) ? emptyColor : baseColor, 3f * Time.deltaTime);


        Vector3 playerPos = Player.transform.position;
        Inside = (playerPos.x > topLeft.x && playerPos.x < bottomRight.x && playerPos.z < topLeft.z && playerPos.z > bottomRight.z);
    }

    private void OnDrawGizmosSelected()
    {
        Color c = Color.blue;
        c.a = .3f;
        Gizmos.color = c;

        Vector3 zoneSize = new Vector3(Size.x, .5f, Size.y);

        // Draw a cube to represent the area of the zone
        Vector3 pos = transform.position;
        pos.y = transform.position.y;


        Gizmos.DrawWireCube(pos, zoneSize);
    }
}
