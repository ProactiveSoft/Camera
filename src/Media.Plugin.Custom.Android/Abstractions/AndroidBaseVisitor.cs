using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Widget;
using Java.Lang;
using Media.Plugin.Custom.Android.Factories;
using Media.Plugin.Custom.Android.Handlers;
using Plugin.CurrentActivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Media.Plugin.Custom.Android.Abstractions
{
	public abstract class AndroidBaseVisitor : BaseVisitor<Task<MediaFile>>, ICameraVisitor<Task<MediaFile>>,
		IVisitable
	{
		//+ Camera
		internal Camera Camera { get; }


		protected AndroidBaseVisitor()
		{
		}

		protected AndroidBaseVisitor(StoreMediaOptions options, OperationType cameraOperationType) :
			base(options, cameraOperationType) => Camera = CameraFactory.CreateCamera(CameraOperationType, options, this);

		/// <summary>
		/// Setup the camera.
		/// </summary>
		/// <param name="data">Data containing camera permission & camera options.</param>
		/// <returns>Doesn't return anything. Task&lt;MediaFile&gt; will be returned by child.</returns>
		/// <exception cref="System.NotSupportedException">Not supported exception when platform doesn't support camera.</exception>
		public virtual Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			try
			{
				if (!media.IsCameraAvailable) throw new NotSupportedException("OS doesn't support Camera.");

				if (!permission) return null;

				verifyOptions(Options);

				Camera.FindCameraProperties(Camera.StoreOptions.DefaultCamera);

				// Undone: Define Facade for calling OpenCamera()  
				//CameraDevice = await Camera.OpenCamera(CameraOpenCloseLock, CameraBackgroundHandler);
			}
			catch (CameraAccessException cameraAccessException)
			{
				cameraAccessException.PrintStackTrace();
			}
			catch (NullPointerException nullPointerException)
			{
				Toast.MakeText(CrossCurrentActivity.Current.Activity, "App cannot use Camera because camera driver is old.",
					ToastLength.Long);
			}

			// No need to return anything as it only prepares camera to be used by Child
			return Task.FromResult<MediaFile>(null);
		}

		#region Visitable

		/// <summary>
		/// Passes private members to handlers.
		/// </summary>
		/// <param name="visitor">Handler.</param>
		public void Accept(IVisitor visitor)
		{
			switch (visitor)
			{
				case IPickerActivityVisitor pickerActivityVisitor:
					// Passes AndroidBaseVisitor's private members to ImageAvailableHandler
					// ToDo: Pass AndroidBaseVisitor's private members to ImageAvailableHandler from Camera
					//pickerActivityVisitor.Visit(CameraBackgroundHandler, MediaPickerActivity);

					// Already called from ImageSaver.Run()
					// May cause bug as new URI will not be created for new image after getting URI for 1st image
					// MediaPickerActivity.Accept(pickerActivityVisitor); // Passes MediaPickerActivity's private members
					break;
				case CameraDeviceStateHandler cameraDeviceStateHandler:
					// ToDo: Pass lock to CameraDeviceStateHandler from Camera
					// Passes private members to CameraDeviceStateHandler
					// cameraDeviceStateHandler.Visit(CameraOpenCloseLock);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion
	}
}