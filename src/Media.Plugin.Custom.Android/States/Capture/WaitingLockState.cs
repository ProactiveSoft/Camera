using System;
using Android.Hardware.Camera2;
using Java.Lang;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;

namespace Media.Plugin.Custom.Android.States.Capture
{
	internal class WaitingLockState : ICaptureState
	{
		private readonly Action _runPrecaptureSequence, _captureStillPhoto;

		public WaitingLockState(Action runPrecaptureSequence, Action captureStillPhoto)
		{
			_runPrecaptureSequence = runPrecaptureSequence;
			_captureStillPhoto = captureStillPhoto;
		}

		public void Work<TState>(CameraCaptureSession session, CaptureRequest request, CaptureResult result,
			ref TState captureState) where TState : class
		{
			Integer afState = (Integer)result.Get(CaptureResult.ControlAfState);
			if (afState == null)
				_captureStillPhoto();
			else if (afState.IntValue() == (int)ControlAFState.FocusedLocked ||
					 afState.IntValue() == (int)ControlAFState.NotFocusedLocked)
			{
				// Auto exposure can be null on some devices
				Integer aeState = (Integer)result.Get(CaptureResult.ControlAeState);
				if (aeState == null || aeState.IntValue() == (int)ControlAEState.Converged)
				{
					captureState = (TState)CaptureStateFactory.GetCaptureState(CaptureStates.PictureTaken);
					_captureStillPhoto();
				}
				else
					_runPrecaptureSequence();
			}
		}
	}
}