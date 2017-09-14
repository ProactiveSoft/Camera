using System;
using System.Collections.Generic;
using Media.Plugin.Custom.Android.Helpers.Parameters;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Factories
{
	internal static class ComputerParametersFactory
	{
		private static readonly Dictionary<OperationType, CameraParameters> CameraParameterses =
			new Dictionary<OperationType, CameraParameters>(2);

		internal static CameraParameters CreateCameraParameters(OperationType cameraOperationType)
		{
			if (CameraParameterses.TryGetValue(cameraOperationType, out CameraParameters parameters)) return parameters;

			switch (cameraOperationType)
			{
				case OperationType.Photo:
					parameters = new PhotoCameraParameters();
					CameraParameterses[OperationType.Photo] = parameters;
					break;
				case OperationType.Video:
					parameters = new VideoCameraParameters();
					CameraParameterses[OperationType.Video] = parameters;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(cameraOperationType), cameraOperationType, null);
			}

			return parameters;
		}
	}
}