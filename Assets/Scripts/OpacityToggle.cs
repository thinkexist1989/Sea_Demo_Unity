using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityToggle : MonoBehaviour
{
    
    public Material tranparentMaterial;
    public Material OpaqueMaterial;

    // Start is called before the first frame update
    void Start()
    {
    }

    
    public void ToggleOpacity(bool isOpaque)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (isOpaque)
            {
                renderer.material = OpaqueMaterial; // 设置为不透明材质
            }
            else
            {
                renderer.material = tranparentMaterial; // 设置为透明材质
            }
        }
        else
        {
            Debug.LogWarning("No MeshRenderer found on this GameObject.", this);
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
