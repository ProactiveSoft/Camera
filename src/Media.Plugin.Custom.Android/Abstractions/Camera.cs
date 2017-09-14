// ***********************************************************************
// Assembly         : Media.Plugin.Custom.Android
// Author           : anila
// Created          : 09-06-2017
//
// Last Modified By : anila
// Last Modified On : 09-06-2017
// ***********************************************************************
// <copyright file="Camera.cs" company="Proso">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Widget;
using Java.Lang;
using Media.Plugin.Custom.Android.Handlers;
using Media.Plugin.Custom.Android.Helpers.EventArgs;
using Media.Plugin.Custom.Android.Helpers.Parameters;
using Plugin.CurrentActivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;
using Boolean = Java.Lang.Boolean;
using CameraDevice = Android.Hardware.Camera2.CameraDevice;
using CameraChoice = Plugin.Media.Abstractions.CameraDevice;

namespace Media.Plugin.Custom.Android.Abstractions
{
	/// <summary>
	/// Facade for Android's Camera2 Api.
	/// </summary>
	internal abstract class Camera : IVisitable
	{
		#region Fields & properties

		protected readonly IVisitable Visitable;

		// Undone: Make fields private & expose operations
		//++ Camera2's camera 
		/// <summary>
		/// The Camera's manager
		/// </summary>
		protected readonly CameraManager Manager;
		/// <summary>
		/// The camera device
		/// </summary>
		protected CameraDevice CameraDevice;
		protected CameraCaptureSession CameraCaptureSession;

		//++ Camera handlers
		/// <summary>
		/// The camera device state handler
		/// </summary>
		protected readonly CameraDeviceStateHandler CameraDeviceStateHandler;
		protected readonly CameraCaptureSessionHandler CameraCaptureSessionHandler;

		//++ Camera properties
		/// <summary>
		/// The store options
		/// </summary>
		/// <value>The store options.</value>
		protected StoreCameraMediaOptions StoreOptions { get; }
		/// <summary>
		/// The camera identifier
		/// </summary>
		protected string CameraId;
		/// <summary>
		/// The camera characteristics
		/// </summary>
		protected CameraCharacteristics CameraCharacteristics;
		/// <summary>
		/// The flash supported
		/// </summary>
		protected bool FlashSupported;
		protected CameraParameters CameraParameters;


		protected OperationType CameraOperationType;


		protected readonly MediaPickerActivity MediaPickerActivity = new MediaPickerActivity();

		#endregion


		#region Camera threading

		private HandlerThread _cameraThread;
		protected Handler CameraBackgroundHandler;

		protected readonly SemaphoreSlim CameraOpenCloseLock = new SemaphoreSlim(0, 1);

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


		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		/// <param name="storeOptions">The store options.</param>
		/// <param name="visitable">The visitable.</param>
		protected Camera(StoreMediaOptions storeOptions, IVisitable visitable)
		{
			Visitable = visitable;
			StoreOptions = storeOptions as StoreCameraMediaOptions;

			Manager = (CameraManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.CameraService);

			CameraDeviceStateHandler = new CameraDeviceStateHandler(this);
			CameraCaptureSessionHandler = new CameraCaptureSessionHandler();
		}

		#region Camera properties 

		/// <summary>
		/// Finds the camera properties.
		/// </summary>
		/// <param name="defaultCamera">The default camera.</param>
		/// <exception cref="ArgumentOutOfRangeException">defaultCamera</exception>
		private void FindCameraProperties(CameraChoice defaultCamera)
		{
			switch (defaultCamera)
			{
				case CameraChoice.Rear:
					Helper(LensFacing.Back);
					break;
				case CameraChoice.Front:
					Helper(LensFacing.Front);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(defaultCamera), defaultCamera,
						$"Only {CameraChoice.Front} & {CameraChoice.Rear} camera is supported.");
			}

			FindLargestResolution();


			void Helper(LensFacing lensFacing)
			{
				foreach (string camId in Manager.GetCameraIdList())
				{
					CameraCharacteristics cameraCharacteristics = Manager.GetCameraCharacteristics(camId);
					Integer facing = (Integer)cameraCharacteristics.Get(CameraCharacteristics.LensFacing);
					if (facing != null && facing == Integer.ValueOf((int)lensFacing))
					{
						CameraCharacteristics = cameraCharacteristics;
						CameraId = camId;

						// Check if flash is supported
						Boolean flashAvailable = (Boolean)CameraCharacteristics.Get(CameraCharacteristics.FlashInfoAvailable);
						FlashSupported = flashAvailable != null && (bool)flashAvailable;
					}
				}
			}
		}

		/// <summary>
		/// Finds the largest resolution.
		/// </summary>
		protected abstract void FindLargestResolution();

		/// <summary>
		/// Opens requested camera and creates <see cref="Android.Hardware.Camera2.CameraCaptureSession"/>.
		/// </summary>
		/// <returns>Task&lt;CameraDevice&gt;: CameraDevice.</returns>
		private async Task<CameraDevice> OpenCamera()
		{
			var tcs = new TaskCompletionSource<CameraDevice>();

			CameraDeviceStateHandler.Opened += CameraStateHandler;
			CameraDeviceStateHandler.Error += CameraStateHandler;
			CameraDeviceStateHandler.Disconnected += CameraStateHandler;

			try
			{
				if (!await CameraOpenCloseLock.WaitAsync(2500))
					tcs.SetException(new RuntimeException("Time out waiting to lock camera opening."));
				else
					Manager.OpenCamera(CameraId, CameraDeviceStateHandler, CameraBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				e.PrintStackTrace();
				tcs.SetException(e);
			}
			catch (InterruptedException e)
			{
				tcs.SetException(new RuntimeException("Interrupted while trying to lock camera opening.", e));
			}

			return await tcs.Task;

			void CameraStateHandler(object sender, CameraDeviceStateEventArgs args)
			{
				try
				{
					tcs.SetResult(args.Camera);
				}
				finally
				{
					CameraDeviceStateHandler.Opened -= CameraStateHandler;
					CameraDeviceStateHandler.Error -= CameraStateHandler;
					CameraDeviceStateHandler.Disconnected -= CameraStateHandler;
				}
			}
		}

		protected abstract void CreateCameraCaptureSession();

		private Task<CameraCaptureSession> GetCameraCaptureSession()
		{
			var tcs = new TaskCompletionSource<CameraCaptureSession>();

			CameraCaptureSessionHandler.Configured += CameraCaptureSession_State;

			return tcs.Task;


			void CameraCaptureSession_State(object sender, CameraCaptureSessionStateEventArgs args)
			{
				try
				{
					tcs.SetResult(args.CameraCaptureSession);
				}
				finally
				{
					CameraCaptureSessionHandler.Configured -= CameraCaptureSession_State;
				}
			}
		}

		#endregion

		#region Camera preparations

		protected abstract void SetupMediaReader();

		#endregion

		#region Camera operations

		/// <summary>
		/// Takes photo or video depending on <see cref="OperationType"/>.
		/// </summary>
		/// <param name="data">Data for setting up camera.</param>
		/// <returns>Captured photo or video.</returns>
		internal virtual async Task<MediaFile> TakeMedia((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data)
		{
			(bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) = data;

			if (CameraCaptureSession == null)
			{
				(CameraDevice camera, CameraCaptureSession session) = await SetupCamera().ConfigureAwait(false);
				CameraDevice = camera;
				CameraCaptureSession = session;
			}

			return default;   // No need to return anything as it only prepares camera to be used by Child

			// Camera setup using Template Method pattern
			async Task<(CameraDevice camera, CameraCaptureSession session)> SetupCamera()
			{
				#region Early camera setup

				if (!media.IsCameraAvailable) throw new NotSupportedException("OS doesn't support camera.");

				if (!permission) return default;

				verifyOptions(StoreOptions);

				try
				{
					FindCameraProperties(StoreOptions.DefaultCamera);

					FindLargestResolution();

					SetupMediaReader();
				}
				catch (CameraAccessException e)
				{
					e.PrintStackTrace();
				}
				catch (NullPointerException e)
				{
					// Currently an NPE is thrown when the Camera2API is used but not supported on the
					// device this code runs.
					Toast.MakeText(CrossCurrentActivity.Current.Activity, "App cannot use Camera because camera's driver is old.",
						ToastLength.Long);
				}

				#endregion

				#region Camera setup

				CameraDevice camera = await OpenCamera().ConfigureAwait(false);
				CameraCaptureSession session = await GetCameraCaptureSession();

				return (camera, session);
				#endregion
			}
		}

		#endregion

		#region Visitables

		/// <summary>
		/// Sends private members to required classes.
		/// </summary>
		/// <param name="visitor">Class which wants private members.</param>
		public virtual void Accept(IVisitor visitor)
		{
			switch (visitor)
			{
				case CameraDeviceStateHandler cameraDeviceStateHandler:
					CameraParameters.CameraOpenCloseLock = CameraOpenCloseLock;
					CameraParameters.CreateCameraCaptureSession = CreateCameraCaptureSession;

					// Passes private members to CameraDeviceStateHandler
					cameraDeviceStateHandler.Visit(CameraParameters);
					break;
				default:
					visitor.Visit(this);
					break;
			}
		}

		#endregion
	}
}