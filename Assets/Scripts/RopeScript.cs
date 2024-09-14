using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeScript : MonoBehaviour
{
    private MeshRenderer mr;
    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mat = mr.material;
    }

    // Update is called once per frame
    void Update()
    {
        ScaleRopeTiling();
    }

    void ScaleRopeTiling()
    {
        mat.mainTextureScale = new Vector2(mat.mainTextureScale.x, transform.localScale.y);
    }
}
