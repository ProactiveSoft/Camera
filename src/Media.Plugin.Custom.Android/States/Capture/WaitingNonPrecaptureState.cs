using System;
using Android.Hardware.Camera2;
using Java.Lang;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;

namespace Media.Plugin.Custom.Android.States.Capture
{
	internal class WaitingNonPrecaptureState : ICaptureState
	{
		private readonly Action _captureStillPhoto;

		public WaitingNonPrecaptureState(Action captureStillPhoto)
		{
			_captureStillPhoto = captureStillPhoto;
		}

		public void Work<TState>(CameraCaptureSession session, CaptureRequest request, CaptureResult result,
			ref TState captureState) where TState : class
		{
			Integer aeState = (Integer)result.Get(CaptureResult.ControlAeState);
			if (aeState == null || aeState.IntValue() != (int)ControlAEState.Precapture)
			{
				captureState = (TState)CaptureStateFactory.GetCaptureState(CaptureStates.PictureTaken);
				_captureStillPhoto();
			}
		}
	}
}