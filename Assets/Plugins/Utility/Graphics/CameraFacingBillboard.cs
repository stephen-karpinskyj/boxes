using UnityEngine;

/// <remarks>Based on: http://wiki.unity3d.com/index.php?title=CameraFacingBillboard</remarks>
public class CameraFacingBillboard : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private string searchTag = "MainCamera";

    private void Awake()
    {
        if (this.cam == null)
        {
            this.cam = GameObjectUtility.FindAllInScene<Camera>(this.gameObject).Find(c => c.tag == this.searchTag);
        }

        Debug.Assert(this.cam != null, this);
    }

    private void Update()
    {
        if (this.cam != null)
        {
            this.transform.LookAt(this.transform.position + this.cam.transform.rotation * Vector3.forward,
                this.cam.transform.rotation * Vector3.up);
        }
    }
}