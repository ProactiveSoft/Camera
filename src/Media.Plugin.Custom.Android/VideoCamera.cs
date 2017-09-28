using System;
using System.Threading.Tasks;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android
{
	/// <inheritdoc />
	/// <summary>
	/// Camera with video related features.
	/// </summary>
	internal class VideoCamera : Camera
	{
		internal VideoCamera(StoreMediaOptions storeOptions) : base(storeOptions)
		{
			CameraOperationType = OperationType.Video;

			CameraParameters = ComputerParametersFactory.CreateCameraParameters(CameraOperationType);
		}

		#region Camera preparations

		protected override void FindLargestResolution()
		{
			throw new NotImplementedException();
		}

		protected override void CreateCameraCaptureSession()
		{
			throw new NotImplementedException();
		}

		protected override void SetupMediaReader()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Camera operations

		/// <inheritdoc />
		protected override Task<MediaFile> GetSavedMediaFile()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}