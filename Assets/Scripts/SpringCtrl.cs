using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringCtrl : MonoBehaviour
{
    
    public TwistDeformer twistDeformer;
    
    private Quaternion initRot;

    // Start is called before the first frame update
    void Start()
    {
        initRot = transform.localRotation; // 初始变换
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = initRot * Quaternion.Euler(0, 0, twistDeformer.value);
    }
}
