using Android.Hardware.Camera2;
using Media.Plugin.Custom.Android.Abstractions;

namespace Media.Plugin.Custom.Android.States.Capture
{
	internal class PictureTakenState : ICaptureState
	{
		public void Work<TState>(CameraCaptureSession session, CaptureRequest request, CaptureResult result,
			ref TState captureState) where TState : class
		{

		}
	}
}