using System;
using Android.Hardware.Camera2;
using Media.Plugin.Custom.Android.Abstractions;

namespace Media.Plugin.Custom.Android.Handlers
{
	internal class CameraCaptureSessionPhotoCaptureHandler : CameraCaptureSession.CaptureCallback
	{
		private readonly Camera _camera;
		private readonly Action _unlockFocus;

		public CameraCaptureSessionPhotoCaptureHandler(Camera camera, Action unlockFocus)
		{
			_camera = camera;
			_unlockFocus = unlockFocus;
		}

		#region Overrides of CaptureCallback

		/// <inheritdoc />
		public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request,
			TotalCaptureResult result) => _unlockFocus();

		#endregion
	}
}