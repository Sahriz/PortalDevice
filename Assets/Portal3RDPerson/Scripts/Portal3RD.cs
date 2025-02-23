using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.TextCore.Text;

public class Portal3RD : MonoBehaviour
{
	private Transform _parent;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Camera _portalCamera;
	[SerializeField] private Camera _otherPortalCamera;
	[SerializeField] private CinemachineCamera _mainCamera;
	[SerializeField] private Transform phonieCharcter;
    [SerializeField] private RenderTexture _portalTexture;
	[SerializeField] private Transform _otherPortalPOS;
	[SerializeField] private Transform _otherPortalChildPos;
	[SerializeField] private string _layerName;
	[SerializeField] private PlayerCharacter3RD _character;
	[SerializeField] private List<PortalTraveller3RD> _traveller = new List<PortalTraveller3RD>();
	[SerializeField] private float offset = 6.0f;

	[SerializeField] private bool _cameraCrossedProtal = false;

	private List<Vector3> _travellersOffset = new List<Vector3>();

	


	private void Awake()
	{
		// Assign the portal's camera target texture.
		if (_portalTexture != null)
		{
			_portalCamera.targetTexture = _portalTexture;
		}
		int portalLayer = LayerMask.NameToLayer(_layerName);
		_portalCamera.cullingMask &= ~(1<< portalLayer);
		_parent = transform.parent;
	}
	private void Start()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector2[] uvs = new Vector2[mesh.vertices.Length];

		uvs[1] = new Vector2(0, 0);
		uvs[0] = new Vector2(0, 1);
		uvs[3] = new Vector2(1, 0);
		uvs[2] = new Vector2(1, 1);

		uvs[5] = new Vector2(0, 0);
		uvs[4] = new Vector2(0, 1);
		uvs[7] = new Vector2(1, 0);
		uvs[6] = new Vector2(1, 1);

		uvs[9] = new Vector2(0, 0);
		uvs[8] = new Vector2(0, 1);
		uvs[11] = new Vector2(1, 0);
		uvs[10] = new Vector2(1, 1);

		uvs[13] = new Vector2(0, 0);
		uvs[12] = new Vector2(0, 1);
		uvs[15] = new Vector2(1, 0);
		uvs[14] = new Vector2(1, 1);

		uvs[19] = new Vector2(0, 0);
		uvs[18] = new Vector2(0, 1);
		uvs[16] = new Vector2(1, 0);
		uvs[17] = new Vector2(1, 1);

		uvs[20] = new Vector2(0, 0);
		uvs[21] = new Vector2(0, 1);
		uvs[23] = new Vector2(1, 0);
		uvs[22] = new Vector2(1, 1);

		
		mesh.uv = uvs;
		foreach(PortalTraveller3RD traveller in _traveller)
		{
			_travellersOffset.Add(traveller.transform.position);
			traveller._rb.interpolation = RigidbodyInterpolation.Interpolate;
		}
		
	}
	private void Update()
	{
		
		HandlePortalCamera();
	}
	private void LateUpdate()
	{
		MovePhonies();
	}

	private void FixedUpdate()
	{
		// Delay teleportation for a frame to allow the movement to resolve.
		PortalOffset();
		HandleTeleportation();
		
		CameraCrossPortalUpdate();
	}
	/// <summary>
	/// Checks if the player camera can see the other portal, enables the other portal camera if that is true and moves the other portal camerato get the correct perspective.
	/// </summary>
	private void HandlePortalCamera() {
		_otherPortalCamera.enabled = isPortalVisible();
		moveCam();
		
	}
	/// <summary>
	/// Moves the portal back a bit to remove clipping with the portal surface, assuming the player is anywhere near the surface.
	/// </summary>
	private void PortalOffset()
	{
		float direction = Vector3.Dot(_playerCamera.transform.position - _parent.position, transform.right);
		float distanceFromSurface = (transform.right * direction).magnitude;
		float distanceThreshold = 0.5f;
		if(distanceFromSurface < distanceThreshold)
		{
			//Debug.Log("N�ra!");
			float Offset = Mathf.Cos((distanceFromSurface*Mathf.PI)/(2*distanceThreshold))/ offset;
			transform.localPosition = -1 *Mathf.Sign(direction) * Offset * transform.right;
		}
		else
		{
			transform.localPosition = Vector3.zero;
		}
	}
	/// <summary>
	/// Moves the phonie characters to take the position of the player character, so that the camera can function with teleportation in a smooth manor. 
	/// </summary>
	private void MovePhonies()
	{
		Vector3 relativePosition = _character.transform.position - _otherPortalPOS.position;
		phonieCharcter.rotation = _character.CameraTarget.rotation;
		phonieCharcter.localPosition = relativePosition;
		if(!_cameraCrossedProtal)
		{
			_mainCamera.Follow = phonieCharcter;
		}
	}

	/// <summary>
	/// Teleports the player to the other portal assuming that the player is within bounds and moved accros the portal.
	/// </summary>
	private void HandleTeleportation()
	{
	
		for (int i = 0; i < _traveller.Count; i++)
		{
			Vector3 prevPosDirection = (new Vector3(_travellersOffset[i].x, 0, _travellersOffset[i].z) - new Vector3(_parent.position.x, 0, _parent.position.z)).normalized;
			Vector3 currentPosDirection = (new Vector3(_traveller[i].transform.position.x, 0, _traveller[i].transform.position.z) - new Vector3(_parent.position.x, 0, _parent.position.z)).normalized;
			bool dotProduct = Mathf.Sign(Vector3.Dot(prevPosDirection, transform.right)) != Mathf.Sign(Vector3.Dot(currentPosDirection, transform.right));

			float bounds = Vector3.Magnitude(Vector3.Dot(_character.transform.position - _parent.position, transform.forward) * transform.forward);
			bool boundBool = bounds < 2.5f && bounds > -2.5;

			if (dotProduct && boundBool)
			{
				Vector3 playerRelativePosition = _otherPortalPOS.TransformPoint(_parent.InverseTransformPoint(_character.transform.position));

				Quaternion playerRelativeRotation = _otherPortalPOS.rotation * Quaternion.Inverse(transform.rotation) * _character.transform.rotation;
				

				_traveller[i].Teleport(_portalCamera.transform, _otherPortalCamera.transform, playerRelativePosition, playerRelativeRotation);
				_cameraCrossedProtal = false;
			}
			
			_travellersOffset[i] = _traveller[i].transform.position;
		}
	}

	/// <summary>
	/// Handles the player camera to move only when it makes sense (a certain threshold has been met).
	/// </summary>
	private void CameraCrossPortalUpdate()
	{
		if(!_cameraCrossedProtal)
		{
			Vector3 portalNormal = transform.right;

			// Calculate vector from portal screen to camera
			Vector3 toCamera = Vector3.Dot(_mainCamera.transform.position - transform.position, portalNormal) * portalNormal;

			Vector3 toPlayer = Vector3.Dot(_character.transform.position - transform.position, portalNormal) * portalNormal;	

			// Check if the camera has crossed the portal plane
			if (Mathf.Sign(Vector3.Dot(toCamera, toPlayer)) > 0)
			{
				Debug.Log("Test");
				// Camera has crossed the portal threshold
				_cameraCrossedProtal = true;

				// Switch back to following the player
				_mainCamera.Follow = _character.transform;
			}
		}
	}

	/// <summary>
	/// Function that moves the portal camera.
	/// </summary>
	private void moveCam()
{
		Vector3 playerRelativePosition = _otherPortalChildPos.InverseTransformPoint(_playerCamera.transform.position);
		_portalCamera.transform.position = transform.TransformPoint(playerRelativePosition);

		Quaternion playerRelativeRotation = Quaternion.Inverse(_otherPortalChildPos.rotation) * _playerCamera.transform.rotation;
		_portalCamera.transform.rotation = transform.rotation * playerRelativeRotation;
	}
	/// <summary>
	/// Function that looks to see if the player camera can see the portal sufrace.
	/// </summary>
	/// <returns>bool representing the camera being able to see the portal surface.</returns>
	private bool isPortalVisible()
	{
		Plane[] frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(_playerCamera);
		Renderer screen = transform.GetComponent<Renderer>();
		return GeometryUtility.TestPlanesAABB(frustrumPlanes, screen.bounds);
	}

	

	
}
