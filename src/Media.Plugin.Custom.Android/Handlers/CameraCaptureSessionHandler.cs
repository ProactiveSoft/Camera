using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.Handlers
{
	internal class CameraCaptureSessionHandler : CameraCaptureSession.StateCallback
	{
		public override void OnConfigured(CameraCaptureSession session)
		{
			
		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			// ToDo: Handle CameraCaptureSession creation failed
		}
	}
}