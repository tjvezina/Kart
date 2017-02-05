using UnityEngine;
using System.Collections;

namespace Modules.CameraSystems
{
	/// <summary> A C# version of the MouseOrbit script provided by Unity.</summary>
	/// <remarks> This class has been modified to support a more robust control set than Unity's native script. To be effective, the camera should be nested within it's target
	/// Author: JHuffman </remarks>
	[RequireComponent(typeof(Camera))]
	public abstract class OrbitCamera : MonoBehaviour
	{
		[SerializeField] protected Transform target;
		[SerializeField] protected Vector3 lookAtOffset;
		[SerializeField] protected float minY = -20;
		[SerializeField] protected float maxY = 80;
		[SerializeField] protected float sensitivity = 1f;
		[SerializeField] protected bool zoomEnabled = true;
		[SerializeField] protected float zoomFactor = 3;
		[SerializeField] protected float minZoom = 1;
		[SerializeField] protected float maxZoom = 10;

		protected float _distance;
		protected float _smoothTime = 0.3f;

		//Used in camera rotation
		protected float _camYaw;
		protected float _camPitch;

		//Used in camera zooming
		protected float _zoomSmooth = 0;
		protected float _zoomRate = 0;

		/// <summary> True = Zooming in; False = Zooming out </summary>
		protected bool _zoomDirection = false;

		protected float _xVelocity = 0;
		protected float _yVelocity = 0;

		private float _xSmooth = 0;
		private float _ySmooth = 0;
		private float _prevYSmooth = 0;
		private bool _collidingWithGround = false;

		#region Abstract Methods
		/// <summary>
		/// Base function for updating the camera's zoom, don;t forget to update the _relativeCameraOffset as well
		/// </summary>
		protected abstract void UpdateZoom();
		#endregion

		#region Virtual Methods
		protected virtual void Awake()
		{
			if (target == null)
				Debug.LogError("Target in OrbitCamera component is null!");

			_distance = Vector3.Distance(transform.position, target.position);
			_zoomSmooth = _distance;

			Vector3 tempAngles = transform.eulerAngles;
			_camYaw = tempAngles.y;
			_camPitch = tempAngles.x;
			_xSmooth = _camYaw;
			_ySmooth = _camPitch;
			_prevYSmooth = _ySmooth;
		}

		protected virtual void Update()
		{
			if (zoomEnabled)
				UpdateZoom();

			//Rotate the camera if rotation is active, otherwise simply maintain its position relative to the target
			if (IsCameraRotating())
				RecaluclatePosition();

			UpdatePosition();

			//Refocus the camera on target
			transform.LookAt(target.position + lookAtOffset);
		}

		/// <summary> Updates the transform information of the camera relative to the target</summary>
		protected virtual void RecaluclatePosition()
		{
			_prevYSmooth = _ySmooth;

			//Smooth the transition
			_xSmooth = Mathf.SmoothDamp(_xSmooth, _camYaw, ref _xVelocity, _smoothTime);
			_ySmooth = Mathf.SmoothDamp(_ySmooth, _camPitch, ref _yVelocity, _smoothTime);
			_ySmooth = ClampAngle(_ySmooth, minY, maxY);

			//Reset the yVelocity if y is already at it's extent
			if (_ySmooth == minY || _ySmooth == maxY)
			{
				_yVelocity = 0;
			}

			if (_collidingWithGround && _ySmooth - _prevYSmooth < 0)
				_ySmooth = _prevYSmooth;
		}

		/// <summary>
		/// Determines whether the camera's rotation should be updated. Base function always returns true
		/// </summary>
		/// <returns>Returns true if the camera should update its rotation</returns>
		protected virtual bool IsCameraRotating()
		{
			return true;
		}
		#endregion

		#region Private Methods
		private void OnCollisionEnter()
		{
			_collidingWithGround = true;
		}

		private void OnCollisionExit()
		{
			_collidingWithGround = false;
		}

		private void UpdatePosition()
		{
			transform.rotation = Quaternion.Euler(_ySmooth, _xSmooth, 0);
			transform.position = transform.rotation * new Vector3(0, 0, -_zoomSmooth) + (target.position + lookAtOffset);
		}

		/// <summary>
		/// Clamps the given angle between a min and max value
		/// </summary>
		/// <param name="angle">Angle to clamp</param>
		/// <param name="min">Minimum angle value</param>
		/// <param name="max">Maximum angle value</param>
		/// <returns>The angle value clamped within the given range.</returns>
		private float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360)
				angle += 360;
			if (angle > 360)
				angle -= 360;
			return Mathf.Clamp(angle, min, max);
		}
		#endregion

		#region Editor
		void OnDrawGizmos()
		{
			if (target != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(transform.position, (target.position + lookAtOffset));
				Gizmos.DrawWireSphere((target.position + lookAtOffset), 0.1f);
			}
		}

		void OnValidate()
		{
			if(target != null)
				transform.LookAt(target.position + lookAtOffset);
		}
		#endregion
	}
}