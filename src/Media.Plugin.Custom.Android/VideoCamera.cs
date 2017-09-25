using System;
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
		internal VideoCamera(StoreMediaOptions storeOptions, IVisitable visitable) : base(storeOptions, visitable)
		{
			CameraOperationType = OperationType.Video;

			CameraParameters = ComputerParametersFactory.CreateCameraParameters(CameraOperationType);
		}

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
		
		public override void Accept(IVisitor visitor)
		{
			base.Accept(visitor);
		}
	}
}