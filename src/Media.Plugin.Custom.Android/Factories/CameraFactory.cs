using System;
using System.Collections.Generic;
using Media.Plugin.Custom.Android.Abstractions;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Factories
{
	internal static class CameraFactory
	{
		private static readonly Dictionary<OperationType, Camera> Cameras = new Dictionary<OperationType, Camera>(2);

		internal static Camera CreateCamera(OperationType operationType, StoreMediaOptions storeOptions, IVisitable visitable)
		{
			if (Cameras.TryGetValue(operationType, out Camera camera)) return camera;

			switch (operationType)
			{
				case OperationType.Photo:
					camera = new PhotoCamera(storeOptions, visitable);
					Cameras[OperationType.Photo] = camera;
					break;
				case OperationType.Video:
					camera = new VideoCamera(storeOptions, visitable);
					Cameras[OperationType.Video] = camera;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
			}

			return camera;
		}
	}
}