using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacterGalaxy : MonoBehaviour
{
	private Vector3 _upAxis, _rightAxis, _forwardAxis;
	[SerializeField] private Rigidbody _rb;
	[SerializeField] private float _moveSpeed = 5.0f;
	[SerializeField] private float _jumpPower = 25.0f;
	private Vector3 _sideDirections = Vector3.zero;
	private Vector3 _forwardDirection = Vector3.zero;
	public Vector3 CombinedForceAdded = Vector3.zero;
	public bool Grounded = false;


    [SerializeField] private float _pullStrength = 9.81f;
    public Transform PlanetPuller;
    public int PlanetPrio;

	
	// Start is called once before the first execution of Update after the MonoBehaviour is created

	private void Start()
	{
		_upAxis = Vector3.up;
		_rightAxis = Vector3.right;
		_forwardAxis = Vector3.forward;
	}

	private void FixedUpdate()
	{
		GetMovementInput();
		UpdateGravity();
		MoveCharacter();
		resetAddedForce();
	}
	/// <summary>
	/// Adds force to the total force to be added to the movement of the character.
	/// </summary>
	/// <param name="force"></param>
	private void AddedForceUpdate(Vector3 force)
	{
		CombinedForceAdded += force;
	}
	/// <summary>
	/// Reset the force that was just used during this physics frame. Use at the end of the fixed update so that the added force can be recalculated next physics frame.
	/// </summary>
	private void resetAddedForce()
	{
		CombinedForceAdded = Vector3.zero;
	}
	/// <summary>
	/// Gets the direction of the movemnt in current physics frame and adds it to combined force to be used this physics frame.
	/// </summary>
	private void GetMovementInput()
	{
		_sideDirections = Input.GetAxisRaw("Horizontal")*transform.right;
		_forwardDirection = Input.GetAxisRaw("Vertical")*transform.forward;
		AddedForceUpdate((_sideDirections + _forwardAxis).normalized * _moveSpeed);
		
	}
	/// <summary>
	/// Used Combined force to move the character.
	/// </summary>
	private void MoveCharacter()
	{
		_rb.AddForce(CombinedForceAdded, ForceMode.Force);
		JumpForce();
	}

	private void JumpForce()
	{
		if (Input.GetKey(KeyCode.Space) && Grounded)
		{
			Grounded = false;
			_rb.AddForce(_rb.transform.up * _jumpPower, ForceMode.Impulse);
		}
	}

	/// <summary>
	/// Updates the gravity based on the planet that has priority on the character. If there is no such planet, then the gravity just pulls the character straight down.
	/// </summary>
	private void UpdateGravity()
	{
		if (PlanetPuller)
		{
			Vector3 gravityDirection = (PlanetPuller.position - transform.position).normalized;
			Physics.gravity = gravityDirection * _pullStrength;
			UpdateUpAxis(gravityDirection);
		}
		else
		{
			Physics.gravity = new Vector3(0.0f, -_pullStrength, 0.0f);
			UpdateUpAxis(Vector3.down);
		}
	}
	/// <summary>
	/// Updates the up axis on the player relative to the gravity pull of the currently pulling planet.
	/// </summary>
	/// <param name="gravityDirection"></param>
	private void UpdateUpAxis(Vector3 gravityDirection)
	{
		// The up axis should be opposite the gravity direction
		_upAxis = -gravityDirection;

		// Rotate the character to align its up with the calculated up axis
		Quaternion targetRotation = Quaternion.FromToRotation(transform.up, _upAxis) * transform.rotation;
		_rb.MoveRotation(targetRotation);
	}
}
