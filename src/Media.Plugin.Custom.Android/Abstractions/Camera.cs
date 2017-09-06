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
using Java.Lang;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation.Handlers;
using Plugin.CurrentActivity;
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
	internal abstract class Camera
	{
		// Undone: Make fields private & expose operations
		//+ Camera 
		/// <summary>
		/// The Camera's manager
		/// </summary>
		protected readonly CameraManager Manager;
		/// <summary>
		/// The camera device
		/// </summary>
		protected CameraDevice CameraDevice;

		//+ Camera handlers
		/// <summary>
		/// The camera device state handler
		/// </summary>
		protected readonly CameraDeviceStateHandler CameraDeviceStateHandler;

		//+ Camera properties
		/// <summary>
		/// The store options
		/// </summary>
		/// <value>The store options.</value>
		internal StoreCameraMediaOptions StoreOptions { get; }
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
		

		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		/// <param name="storeOptions">The store options.</param>
		/// <param name="visitable">The visitable.</param>
		protected Camera(StoreMediaOptions storeOptions, IVisitable visitable)
		{
			StoreOptions = storeOptions as StoreCameraMediaOptions;

			Manager = (CameraManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.CameraService);

			CameraDeviceStateHandler = new CameraDeviceStateHandler(visitable);
		}

		#region Camera properties 

		/// <summary>
		/// Finds the camera properties.
		/// </summary>
		/// <param name="defaultCamera">The default camera.</param>
		/// <exception cref="ArgumentOutOfRangeException">defaultCamera</exception>
		internal void FindCameraProperties(CameraChoice defaultCamera)
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

		#endregion

		#region Camera operations

		/// <summary>
		/// Opens requested camera.
		/// </summary>
		/// <param name="cameraOpenLock">Lock for opening camera to handle contention.</param>
		/// <param name="cameraBackgroundHandler"></param>
		/// <returns>Task&lt;CameraDevice&gt;: CameraDevice.</returns>
		internal async Task<CameraDevice> OpenCamera(SemaphoreSlim cameraOpenLock, Handler cameraBackgroundHandler)
		{
			var tcs = new TaskCompletionSource<CameraDevice>();

			CameraDeviceStateHandler.Opened += CameraStateHandler;
			CameraDeviceStateHandler.Error += CameraStateHandler;
			CameraDeviceStateHandler.Disconnected += CameraStateHandler;

			try
			{
				if (!await cameraOpenLock.WaitAsync(2500))
					tcs.SetException(new RuntimeException("Time out waiting to lock camera opening."));
				else
					Manager.OpenCamera(CameraId, CameraDeviceStateHandler, cameraBackgroundHandler);
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

		#endregion
	}
}