using System.Collections;
using UnityEngine;

public class PortalTraveller3RD : MonoBehaviour
{
    public Vector3 previousOffsetFromPorrtal { get; set; } = Vector3.zero;
    public Rigidbody _rb;
	public bool _isTeleporting = false;
	private int i = 0;

	private void Start()
	{
		previousOffsetFromPorrtal = transform.localPosition;
	}
	public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
		Debug.Log("TELEPORTING: " + i++);
		StartCoroutine(UnlockPortal());
		_rb.MovePosition(pos + Vector3.up*0.001f);
        //_rb.transform.rotation = rot;
    }
	private IEnumerator UnlockPortal()
	{
		_isTeleporting = true;
		yield return new WaitForEndOfFrame();
		_isTeleporting = false;
	}
    public virtual void EnterPortalThreshold() { }

    public virtual void ExitPortalThreshold() { }
}
