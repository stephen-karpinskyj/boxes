using UnityEngine;

public class RealtimeReflection : MonoBehaviour
{
    [SerializeField]
    private ReflectionProbe probe;

    private void Update()
    {
        this.probe.transform.position = new Vector3(
            Camera.main.transform.position.x,
            Camera.main.transform.position.y * -1,
            Camera.main.transform.position.z
        );

        this.probe.RenderProbe();
    }
}
