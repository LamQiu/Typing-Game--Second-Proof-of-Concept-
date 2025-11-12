using System;
using UnityEngine;
using System.Collections;

public class BackgroundScroll : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 1;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //myRenderer.material.SetTextureOffset ("_MainTex", new Vector2 (Time.time/4, 0));
        _meshRenderer.material.mainTextureOffset = new Vector2(Time.time * speedMultiplier, Time.time * speedMultiplier);
    }
}