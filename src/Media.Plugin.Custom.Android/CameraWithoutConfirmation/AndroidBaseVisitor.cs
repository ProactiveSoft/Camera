using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Widget;
using Java.Lang;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation.Handlers;
using Media.Plugin.Custom.Android.Factories;
using Plugin.CurrentActivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using AndroidSize = Android.Util.Size;
using Camera = Media.Plugin.Custom.Android.Abstractions.Camera;
using CameraChoice = Plugin.Media.Abstractions.CameraDevice;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;

namespace Media.Plugin.Custom.Android.CameraWithoutConfirmation
{
	public abstract class AndroidBaseVisitor : BaseVisitor<Task<MediaFile>>, ICameraVisitor<Task<MediaFile>>,
		IVisitable
	{
		private readonly Activity _currentActivity = CrossCurrentActivity.Current.Activity;

		//+ Camera
		internal readonly Camera Camera;
		protected CameraDevice CameraDevice;


		protected readonly SemaphoreSlim CameraOpenCloseLock = new SemaphoreSlim(0, 1);


		//+ Camera background thread
		private HandlerThread _cameraThread;
		protected internal Handler CameraBackgroundHandler;


		internal static readonly MediaPickerActivity MediaPickerActivity = new MediaPickerActivity();


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
		public virtual async Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			try
			{
				if (!media.IsCameraAvailable) throw new NotSupportedException("OS doesn't support Camera.");

				if (!permission) return null;

				verifyOptions(Options);

				Camera.FindCameraProperties(Camera.StoreOptions.DefaultCamera);

				CameraDevice = await Camera.OpenCamera(CameraOpenCloseLock, CameraBackgroundHandler);
			}
			catch (CameraAccessException cameraAccessException)
			{
				cameraAccessException.PrintStackTrace();
			}
			catch (NullPointerException nullPointerException)
			{
				Toast.MakeText(_currentActivity, "App cannot use Camera because camera driver is old.", ToastLength.Long);
			}

			// No need to return anything as it only prepares camera to be used by Child
			return default(MediaFile);
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
					pickerActivityVisitor.Visit(CameraBackgroundHandler, MediaPickerActivity);

					MediaPickerActivity.Accept(pickerActivityVisitor); // Passes MediaPickerActivity's private members
					break;
				case CameraDeviceStateHandler cameraDeviceStateHandler:
					// Passes private members to CameraDeviceStateHandler
					cameraDeviceStateHandler.Visit(CameraOpenCloseLock);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Starts the back ground thread.
		/// </summary>
		private void StartBackGroundThread()
		{
			_cameraThread = new HandlerThread("CameraBackgroundThread");
			_cameraThread.Start();
			CameraBackgroundHandler = new Handler(_cameraThread.Looper);
		}

		/// <summary>
		/// Stops the background thread.
		/// </summary>
		private void StopBackgroundThread()
		{
			_cameraThread.QuitSafely();
			try
			{
				_cameraThread.Join();
				_cameraThread = null;
				CameraBackgroundHandler = null;
			}
			catch (InterruptedException e)
			{
				e.PrintStackTrace();
			}
		}

		#endregion
	}
}