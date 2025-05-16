using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

// [RequireComponent(typeof(MeshFilter))]
public class TwistDeformer : MonoBehaviour
{
    public float value = 0f; // 每单位长度的扭转角度（度）
    private Mesh mesh;
    private Vector3[] originalVertices, twistedVertices;

    public float maxAngle = 10f;
    
    public Color maxColor = Color.red;

    private float maxDistance = 0.0f;
    
    private Material material;

    void Start()
    {
        mesh = GetComponentInChildren<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        twistedVertices = new Vector3[originalVertices.Length];
        originalVertices.CopyTo(twistedVertices, 0);
        
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector2 v = new Vector2(originalVertices[i].x, originalVertices[i].z);
            float distance = v.magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
            
            Debug.Log("最远距离: " + maxDistance);

        }
        
        material = GetComponentInChildren<MeshRenderer>().material;
        material.SetColor("_Color", Color.gray);
        
        
    }

    void Update()
    {
        
        value = Mathf.Clamp(value, -maxAngle, maxAngle);
        for (int i = 0; i < twistedVertices.Length; i++)
        {
            Vector2 v = new Vector2(originalVertices[i].x, originalVertices[i].y);
            float angle =  v.magnitude * value * Mathf.Deg2Rad / maxDistance;
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            float x = v.x * cos - v.y*sin;
            float y = v.x * sin + v.y*cos;

            twistedVertices[i] = new Vector3(x, y, originalVertices[i].z);
        }

        mesh.vertices = twistedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        Color color = Color.Lerp(Color.gray, maxColor, Mathf.Abs(value) / maxAngle);
        material.SetColor("_Color", color);
    }
}
