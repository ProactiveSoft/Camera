using Android.Hardware.Camera2;

namespace Media.Plugin.Custom.Android.Abstractions
{
	internal interface ICaptureState
	{
		void Work<TState>(CameraCaptureSession session, CaptureRequest request, CaptureResult result, ref TState captureState)
			where TState : class;
	}
}