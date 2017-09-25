using System;
using System.Collections.Generic;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.States.Capture;

namespace Media.Plugin.Custom.Android.Factories
{
	internal class CaptureStateFactory
	{
		private static CaptureStateFactory _instance;

		private readonly Action _runPrecaptureSequence, _captureStillPhoto;

		private static readonly Dictionary<CaptureStates, ICaptureState> States =
			new Dictionary<CaptureStates, ICaptureState>(4);

		public CaptureStateFactory(Action runPrecaptureSequence, Action captureStillPhoto)
		{
			_instance = this;

			_captureStillPhoto = captureStillPhoto;
			_runPrecaptureSequence = runPrecaptureSequence;
		}

		internal static ICaptureState GetCaptureState(CaptureStates captureState)
		{
			if (States.TryGetValue(captureState, out ICaptureState state)) return state;

			switch (captureState)
			{
				case CaptureStates.WaitingLock:
					state = new WaitingLockState(_instance._runPrecaptureSequence, _instance._captureStillPhoto);
					States[CaptureStates.WaitingLock] = state;
					break;
				case CaptureStates.WaitingPrecapture:
					state = new WaitingPrecaptureState();
					States[CaptureStates.WaitingPrecapture] = state;
					break;
				case CaptureStates.WaitingNonPrecapture:
					state = new WaitingNonPrecaptureState(_instance._captureStillPhoto);
					States[CaptureStates.WaitingNonPrecapture] = state;
					break;
				case CaptureStates.PictureTaken:
					state = new PictureTakenState();
					States[CaptureStates.PictureTaken] = state;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(captureState), captureState, null);
			}

			return state;
		}
	}
}