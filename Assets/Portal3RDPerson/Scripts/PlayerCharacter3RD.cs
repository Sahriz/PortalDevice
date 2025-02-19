using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;


public class PlayerCharacter3RD : MonoBehaviour
{
	[SerializeField] private float _targetSpeed = 5.0f;
	[SerializeField] private float _velPower = 1.0f; //Maybe remove
    [SerializeField] private float _acceleration = 15f;
	[SerializeField] private float _deceleration = 15f;
	[SerializeField] private float _velocityThreshold = 2.0f; //Maybe remove
	[SerializeField] private float _jumpForce = 25f;


	public Rigidbody _rb;
	[SerializeField] private float _sensX;
	[SerializeField] private float _sensY;
	[SerializeField] private Camera _cam;
	[SerializeField] private CinemachineCamera _cinemachineCamera;

	private float _xRotation;
	private float _yRotation;

	[SerializeField] private Transform _orientation;
	
    public Vector3 PlayerPosition { get; private set; }
    public Quaternion PlayerRotation { get; private set; }

    private Vector3 _direction = Vector3.zero; //For movement directions during a physics frame.

	private Vector3 _addedForce = Vector3.zero; //For all the forces enacting on the player during a single physics frame.

	private bool _grounded = true;

	private void Awake()
	{
		PlayerPosition = _rb.position;
        PlayerRotation = _rb.rotation;
	}
	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		_rb.linearDamping = 2.0f;
	}
	private void Update()
	{

		updateRotation();
		UpdatePlayerMovement();
	}
	private void FixedUpdate()
	{
		
		GetMovementDirection();
		CheckJump();
		MoveCharacter();
		ResetAddedForce();
	}
	
	/// <summary>
	/// Updates the character rotation based on the mouse movements.
	/// </summary>
	private void updateRotation()
	{
		float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _sensX;
		float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _sensY;

		_yRotation += mouseX;
		_xRotation -= mouseY;
		///_xRotation = Mathf.Clamp(_xRotation, -90, 90); // Clamp vertical rotation to prevent flipping
		// Rotate the character (and Rigidbody) on the Y-axis
		Quaternion targetOrientationRotation = Quaternion.Euler(0, _yRotation, 0);
		_orientation.rotation = Quaternion.Slerp(_orientation.rotation, targetOrientationRotation, Time.deltaTime * 10f);

		// Rotate the camera on the X-axis only (local rotation)
		Quaternion targetCameraRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
		_cinemachineCamera.transform.localRotation = Quaternion.Slerp(
			_cinemachineCamera.transform.localRotation,
			targetCameraRotation,
			Time.deltaTime * 10f
		);


	}
	/// <summary>
	/// Updates the <c>_addedForce</c> to include a jump force. Currently the space key activates it, as long as the player is <c>_grounded</c>.
	/// </summary>
	private void CheckJump()
	{
		if (Input.GetKey(KeyCode.Space) && _grounded)
		{			
			_addedForce += _jumpForce * Vector3.up;
			_grounded = false;
		}
	}
	/// <summary>
	/// Reset the <c>_addedForce</c>. Usefull at the end of <c>FixedUpdate</c> since the <c>_addedForce</c> should not carry over into the next physics frame.
	/// </summary>
	private void ResetAddedForce()
	{
		_addedForce = Vector3.zero;
	}
	/// <summary>
	/// Calculates the movement <c>_direction</c> and force that is then added onto the <c>_addedForce</c>. 
	/// </summary>
	private void GetMovementDirection()
	{
		_direction = _rb.transform.right * Input.GetAxisRaw("Horizontal") + _rb.transform.forward* Input.GetAxisRaw("Vertical"); //Gets the direction of the inputs relative to the rotation of the character.
		Vector3 current_direction = new Vector3(_rb.linearVelocity.x, 0.0f, _rb.linearVelocity.z); //Direction before physics is applied.
		float currentDir = current_direction.magnitude * Mathf.Sign(Vector3.Dot(_direction, current_direction)); //The direction and magnitude difference between this frames direction and last frames dircetion.
		float input = _direction.magnitude > 0 ? _targetSpeed : 0; //If we input anything then the value is _targetSpeed, else the value is zero.

		float speedDif = input - currentDir; //Get difference in target speed and current speed. If we are above the target speed the speedDif will be negative.
		float accelRate = (input > 0.01f) ? _acceleration : _deceleration; //If input is larger than 0 (any input at all) then we use the _acceleration multiplier, else we use the _deceleration multiplier.

		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, _velPower) * Mathf.Sign(speedDif); //Takes the speedDif, multiplies it with the accelRate (pow is maybe not necessary) and multiply with the sign of speedDif (pos if we need more speed to reach target vel, negative if we are moving in opposite direction or want to stop or going above target speed).
		_addedForce += _grounded ? _direction.normalized * movement : _direction.normalized * movement * 0.2f; //Add the movment speed multiplied with the normalized _direction onto the _addedForce.
	}
	/// <summary>
	/// Adds up the forces of <c>_addedForce</c> onto the character.
	/// </summary>
	private void MoveCharacter()
	{	
		_rb.AddForce(new Vector3(_addedForce.x, 0.0f, _addedForce.z), ForceMode.Force); //To move the character.
		_rb.AddForce(new Vector3(0.0f,_addedForce.y, 0.0f), ForceMode.Impulse); //To make the character jump. Distinction is important because of impulse force vs just force.
	}
	/// <summary>
	/// Updates public variables denoting the player position and rotation.
	/// </summary>
	private void UpdatePlayerMovement()
	{
		PlayerPosition = _rb.position;
		PlayerRotation = _rb.rotation;
	}
	/* 
	 Currently checks if player is grounded to allow for the player to jump. Might add an onCollisionExit as well
	 to check when player is not grounded, since doing that only when the player has jumped would allow the player
	 to jump if they first fell of a ledge.
	*/
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			_grounded = true;
		}
	}
	
}
