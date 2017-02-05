using UnityEngine;
using System.Collections;

namespace Modules.CameraSystems
{
	/// <summary> Allowsa for mouse control of a third person camera. This component does not require any axis setup. </summary>
	/// Author: JHuffman </remarks>
	[RequireComponent(typeof(Camera))]
	public class MouseOrbitCamera : OrbitCamera
	{
		/// <summary> Mouse position falls into positive X and Y values with (0, 0) being the bottom left of the game screen. Z is always 0
		/// DEAD_MOUSE is used as a safe value for when the previous mouse position needs to be reset </summary>
		private static Vector3 DEAD_MOUSE = new Vector3(-1, -1, -1);
		private enum MouseOptions
		{
			FREE = -1,
			LEFT_HOLD = 0,
			RIGHT_HOLD = 1,
			MIDDLE_HOLD = 2,
			ANY_HOLD = 3
		}

		[SerializeField] private MouseOptions _rotationControl;

		private Vector3 _prevMousePosition = DEAD_MOUSE;

		protected override void Update()
		{
			base.Update();

			if (IsCameraRotating() == false)
				_prevMousePosition = DEAD_MOUSE;
		}

		/// <summary> Calculate the position, pitch and yaw changes in the camera and let the base class calculate the update</summary>
		protected override void RecaluclatePosition()
		{
			if (_prevMousePosition == DEAD_MOUSE)
				_prevMousePosition = Input.mousePosition;

			//Get the input and update the Pitch and Yaw
			Vector3 deltaPosition = Input.mousePosition - _prevMousePosition;
			_camYaw += deltaPosition.x * sensitivity;
			_camPitch -= deltaPosition.y * sensitivity;

			_prevMousePosition = Input.mousePosition;

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
		/// Determines whether the camera's rotation should be updated based on the mouse control options
		/// </summary>
		/// <returns>Returns true if the camera should update its rotation</returns>
		protected override bool IsCameraRotating()
		{
			if (_rotationControl == MouseOptions.FREE)
				return true;

			if (_rotationControl == MouseOptions.LEFT_HOLD || _rotationControl == MouseOptions.RIGHT_HOLD || _rotationControl == MouseOptions.MIDDLE_HOLD)
				return Input.GetMouseButton((int)_rotationControl);

			if (_rotationControl == MouseOptions.ANY_HOLD)
				return Input.GetMouseButton((int)MouseOptions.LEFT_HOLD) | Input.GetMouseButton((int)MouseOptions.RIGHT_HOLD) | Input.GetMouseButton((int)MouseOptions.MIDDLE_HOLD);

			return false;
		}
	}
}