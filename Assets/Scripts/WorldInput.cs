using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInput : MonoBehaviour
{
    [Serializable]
    public class DragData
    {
        /// <summary>
        /// Object being dragged.
        /// </summary>
        public GameObject DraggedGO;

        /// <summary>
        /// World distance between dragged point and dragged object on drag start. 
        /// </summary>
        public Vector3 WorldOffset;

        /// <summary>
		/// World position of dragged object on drag start.
        /// </summary>
        public Vector3 StartWorldPosition;

        /// <summary>
        /// Last ray from camera to dragged point.
        /// </summary>
        public Ray CurrentRay;

        /// <summary>
        /// Last screen position of dragged point;
        /// </summary>
        public Vector2 CurrentScreenPosition;

        /// <summary>
        /// Last world position of dragged point;
        /// </summary>
        public Vector3 CurrentWorldPosition;
    }

    private const float HoldDurationThreshold = 0.5f;
    private const float DragStartDeltaThreshold = 0.1f;

    [SerializeField]
    private Camera worldCamera;

    private Vector2 lastPointerPos;
    private RaycastHit lastPressHit;
    private float timePressStarted;

    private DragData drag;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            this.MouseDown();
        }

        if (Input.GetMouseButton(0))
        {
            var needsUpdate = this.drag == null || GetPointerPos() != this.lastPointerPos;

			if (needsUpdate)
            {
                this.MouseUpdate();
                this.lastPointerPos = GetPointerPos();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            this.MouseUp();
        }
    }
    
    private bool PointerRaycast(out RaycastHit hit, float maxDistance = Mathf.Infinity)
    {
        Debug.Assert(this.worldCamera != null, this);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            hit = default(RaycastHit);
            return false;
        }

        var ray = this.worldCamera.ScreenPointToRay(GetPointerPos());
        return Physics.Raycast(ray, out hit, maxDistance);
    }

    private static Vector2 GetPointerPos()
    {
        return Input.mousePosition;
    }

    private void MouseDown()
	{
        var didHit = this.PointerRaycast(out this.lastPressHit);

        if (didHit)
        {
            this.timePressStarted = Time.realtimeSinceStartup;
        }
	}

	private void MouseUp()
    {
        var pressedGO = this.lastPressHit.GetGameObject();

        if (pressedGO != null)
        {
            if (this.drag != null)
            {
                Debug.Log("[World.Input] Drag ended on object=" + pressedGO.name);
                this.BroadcastMessage("OnDragEndWorldObject", this.drag, SendMessageOptions.DontRequireReceiver);
                this.drag = null;
            }
            else
            {
                RaycastHit hit;
                this.PointerRaycast(out hit);

                if (pressedGO == hit.GetGameObject())
                {
                    Debug.Log("[World.Input] Tapped on object=" + pressedGO.name);
                    this.BroadcastMessage("OnTapWorldObject", pressedGO, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
	}

    private void MouseUpdate()
    {
        var pressedGO = this.lastPressHit.GetGameObject();
        if (pressedGO != null)
        {
			this.CheckDrag();

            if (this.drag == null)
            {
                this.CheckHold();
            }
        }
    }

    private void CheckDrag()
    {
        var pressedGO = this.lastPressHit.GetGameObject();
        var pointerPos = GetPointerPos();
        var ray = this.worldCamera.ScreenPointToRay(pointerPos);
        var draggedPoint = ray.GetPoint(this.lastPressHit.distance);
        var shouldDrag = this.drag != null || Vector3.Distance(this.lastPressHit.point, draggedPoint) >= DragStartDeltaThreshold;

        if (shouldDrag)
        {
            var isDragStarting = this.drag == null;

            if (isDragStarting)
            {
                this.drag = new DragData
                {
                    DraggedGO = pressedGO,
                    WorldOffset = this.lastPressHit.transform.position - this.lastPressHit.point,
                    StartWorldPosition = this.lastPressHit.point,
                };
            }

            this.drag.CurrentRay = ray;
            this.drag.CurrentScreenPosition = pointerPos;
            this.drag.CurrentWorldPosition = draggedPoint;

            if (isDragStarting)
            {
                Debug.Log("[World.Input] Drag started on object=" + pressedGO.name);
                this.BroadcastMessage("OnDragStartWorldObject", this.drag, SendMessageOptions.DontRequireReceiver);
            }

            this.BroadcastMessage("OnDragUpdateWorldObject", this.drag, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void CheckHold()
    {
		var pressedGO = this.lastPressHit.GetGameObject();
		var shouldHold = Time.realtimeSinceStartup - this.timePressStarted >= HoldDurationThreshold;

        if (shouldHold)
	    {
	        RaycastHit hit;
	        var didHit = this.PointerRaycast(out hit);

	        if (didHit && pressedGO == hit.GetGameObject())
            {
                Debug.Log("[World.Input] Held object=" + pressedGO.name);
                this.BroadcastMessage("OnHoldWorldObject", pressedGO, SendMessageOptions.DontRequireReceiver);
                this.lastPressHit = default(RaycastHit);
	        }  
	    }
    }
}
