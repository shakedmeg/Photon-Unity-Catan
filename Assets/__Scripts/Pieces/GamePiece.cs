using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class GamePiece : MonoBehaviourPun
{
    [Header("Set in Inspector")]
    public float fallTime = 0.5f;

    [Header("Set Dynamically")]
    private Vector3 p0, p1;
    private bool falling = false;
    private float fallStart;
    private MeshRenderer mRend;
    private Collider coll;


    private Vector3 s0, s1;
    private bool scaling = false;
    private float scaleStart;
    private bool loopScale;
    private Vector3 scaleToFinishAt;


    public virtual void Awake()
    {
        object[] data = photonView.InstantiationData;
        mRend = GetComponent<MeshRenderer>();
        SetColor(Utils.Name_To_Color((string)data[0]));
        coll = GetComponent<Collider>();

    }
    void Start()
    {
    }

    public void InitDrop(Vector3 p0, Vector3 p1)
    {
        this.p0 = p0;
        this.p1 = p1;
        fallStart = Time.time;
        falling = true;
    }

    public void Drop()
    {
        if (falling)
        {
            float u = (Time.time - fallStart) / fallTime;
            if (u >= 1)
            {
                u = 1;
                falling = false;
            }
            transform.position = (1 - u) * p0 + u * p1;

        }
    }

    void SetColor(Color color)
    {
        foreach (Renderer mr in GetComponentsInChildren<Renderer>())
        {
            mr.material.color = color;
        }
        mRend.material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        Drop();
    }


    public void InitScaleUpDown(Vector3 s0, Vector3 s1)
    {
        coll.enabled = true;
        this.s0 = s0;
        this.s1 = s1;
        scaleStart = Time.time;
        scaling = true;
        loopScale = true;
        scaleToFinishAt = s0;
    }

    protected void ScaleUpDown()
    {
        bool assign = true;
        if (scaling)
        {
            float u = (Time.time - scaleStart) / Consts.ScaleTime;
            if (u >= 1)
            {
                u = 1;
                if (loopScale)
                {
                    transform.localScale = (1 - u) * s0 + u * s1;
                    assign = false;
                    Vector3 temp = s0;
                    s0 = s1;
                    s1 = temp;
                    scaleStart = Time.time;
                }
                else
                {
                    scaling = false;
                }
            }
            if (assign)
                transform.localScale = (1 - u) * s0 + u * s1;
        }
    }

    public virtual void StopScaling()
    {
        coll.enabled = false;
        scaling = false;
        transform.localScale = scaleToFinishAt;
    }

}
