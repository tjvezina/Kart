using UnityEngine;
using System.Collections;

namespace Modules.CameraSystems
{
	/// <summary>
	/// Allows for axis control of a third person camera
	/// </summary>
	/// Author: JHuffman </remarks>
	[RequireComponent(typeof(Camera))]
	public class AxisOrbitCamera : OrbitCamera
	{
		[SerializeField] private float _deadValue = 0.001f;

		protected override void Awake()
		{
			base.Awake();

			//We lower the camera smooth time to increase response time on camera movement
			_smoothTime = 0.1f;
		}

		/// <summary> Calculate the position, pitch and yaw changes in the camera and let the base class calculate the update</summary>
		protected override void RecaluclatePosition()
		{
			//Get the input and update the Pitch and Yaw
			_camYaw += Input.GetAxis(CameraInputStatics.CAMERA_HORIZONTAL) * sensitivity;
			_camPitch -= Input.GetAxis(CameraInputStatics.CAMERA_VERTICAL) * sensitivity;

			base.RecaluclatePosition();
		}

		/// <summary>
		/// Updates the camera's distance from the target
		/// </summary>
		protected override void UpdateZoom()
		{
			var scrollValue = Input.GetAxis(CameraInputStatics.CAMERA_ZOOM);

			if (scrollValue != 0)
			{
				//If scrolling up, then zooming in, otherwise zooming out
				bool newZoomDirection = scrollValue > 0 ? true : false;

				if (newZoomDirection != _zoomDirection)
				{
					_distance = _zoomSmooth;
					_zoomDirection = newZoomDirection;
				}

				_distance -= _zoomDirection ? zoomFactor : -zoomFactor;
			}

			_zoomSmooth = Mathf.SmoothDamp(_zoomSmooth, _distance, ref _zoomRate, _smoothTime);
			_zoomSmooth = Mathf.Clamp(_zoomSmooth, minZoom, maxZoom);

			transform.position = target.position + ((transform.position - target.position).normalized * _zoomSmooth);
		}

		/// <summary>
		/// Determines whether the camera's rotation should be updated based on the joystick information.
		/// </summary>
		/// <returns>Returns true if the camera should update its rotation</returns>
		protected override bool IsCameraRotating()
		{
			float horizontalInput = Input.GetAxis(CameraInputStatics.CAMERA_HORIZONTAL);
			float verticalInput = Input.GetAxis(CameraInputStatics.CAMERA_VERTICAL);

			//Check to see if the joystiq is dead
			if (horizontalInput <= _deadValue && horizontalInput >= (_deadValue * -1) && verticalInput <= _deadValue && verticalInput >= (_deadValue * -1))
			{
				//Even if the joystick is dead we want to give the camera a chance to smoothly stop instead of stopping it immediately
				//This is because if we stop the camera immediately we will get an odd jerk when changing direction
				if (_xVelocity <= 0.001 && _yVelocity <= 0.001)
					return false;
			}

			return true;
		}
	}
}