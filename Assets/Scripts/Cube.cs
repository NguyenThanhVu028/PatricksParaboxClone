using UnityEngine;

public class Cube : MonoBehaviour
{
    [Header("Detect parent cube")]
    [SerializeField] protected float detectParentCubeRate = 0.1f; // How often this cube checks if it's inside another cube
    [SerializeField] protected LayerMask mainCubeLayerMask; // The layer mask for detecting main cubes
    // The cube which this cube is currently inside of, if any 
    [SerializeField] protected EnterableCube parentCube = null;
    private void Start()
    {
        CompulsoryStart();
        AdditionalStart();
    }

    protected virtual void CompulsoryStart()
    {
        InvokeRepeating("ScanForParentCube", 0f, detectParentCubeRate);
    }
    protected virtual void AdditionalStart()
    {

    }
    protected virtual void ScanForParentCube()
    {
        parentCube = null;
        var collider2Ds = Physics2D.OverlapBoxAll(transform.position, transform.lossyScale, transform.eulerAngles.z, mainCubeLayerMask);
        foreach(var collider2D in collider2Ds)
        {
            if (collider2D == null || collider2D.gameObject == null) continue;
            MainCube detectedMainCube = collider2D.gameObject.GetComponent<MainCube>();
            if (detectedMainCube != null)
            {
                parentCube = detectedMainCube.MiniCube;
                return;
            }
        }
    }
}
