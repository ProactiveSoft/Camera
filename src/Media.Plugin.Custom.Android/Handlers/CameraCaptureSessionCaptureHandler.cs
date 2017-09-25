// ***********************************************************************
// Assembly         : Media.Plugin.Custom.Android
// Author           : anila
// Created          : 09-16-2017
//
// Last Modified By : anila
// Last Modified On : 09-19-2017
// ***********************************************************************
// <copyright file="CameraCaptureSessionCaptureHandler.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Android.Hardware.Camera2;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Handlers
{
	/// <inheritdoc cref="IVisitableRef{TRequiredData}" />
	/// <summary>
	/// Class CameraCaptureSessionCaptureHandler.
	/// </summary>
	/// <typeparam name="TRequiredData">The type of required data.</typeparam>
	/// <seealso cref="!:Android.Hardware.Camera2.CameraCaptureSession.CaptureCallback" />
	/// <seealso cref="!:Plugin.Media.Abstractions.Custom.IVisitableRef{TRequiredData}" />
	internal class CameraCaptureSessionCaptureHandler<TRequiredData> : CameraCaptureSession.CaptureCallback, IVisitableRef<TRequiredData>
	{
		/// <summary>
		/// The capture state
		/// </summary>
		private TRequiredData _captureState;

		/// <summary>
		/// Called when [capture progressed].
		/// </summary>
		/// <param name="session">The session.</param>
		/// <param name="request">The request.</param>
		/// <param name="partialResult">The partial result.</param>
		public override void OnCaptureProgressed(CameraCaptureSession session, CaptureRequest request, CaptureResult partialResult)
		{

		}

		/// <summary>
		/// Called when [capture completed].
		/// </summary>
		/// <param name="session">The session.</param>
		/// <param name="request">The request.</param>
		/// <param name="result">The result.</param>
		public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
		{

		}


		#region Visitables

		/// <inheritdoc />
		/// <summary>
		/// Transfers required data to <see cref="T:Plugin.Media.Abstractions.Custom.IVisitorRef" />.
		/// </summary>
		/// <param name="visitor">
		///   <see cref="T:Plugin.Media.Abstractions.Custom.IVisitorRef" /> requiring private data.</param>
		/// <returns>
		/// Private data required by <see cref="T:Plugin.Media.Abstractions.Custom.IVisitorRef" />.
		/// </returns>
		public ref TRequiredData Accept(IVisitorRef visitor) => ref visitor.Visit(ref _captureState);

		public void Accept(IVisitor visitor) => visitor.Visit(this);

		#endregion
	}
}