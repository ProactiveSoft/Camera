// ***********************************************************************
// Assembly         : Media.Plugin.Custom.Android
// Author           : anila
// Created          : 09-28-2017
//
// Last Modified By : anila
// Last Modified On : 09-28-2017
// ***********************************************************************
// <copyright file="AndroidVisitor.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading.Tasks;
using Media.Plugin.Custom.Android.Abstractions;
using Media.Plugin.Custom.Android.Factories;
using Plugin.Media.Abstractions;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android
{
	/// <inheritdoc cref="BaseVisitor{T}" />
	/// <summary>
	/// Class for taking picture or video without confirmation.
	/// </summary>
	/// <seealso cref="!:Media.Plugin.Abstractions.Custom.BaseVisitor{Task{Plugin.Media.Abstractions.MediaFile}}" />
	/// <seealso cref="!:Media.Plugin.Abstractions.Custom.IMediaVisitor{Task{Plugin.Media.Abstractions.MediaFile}}" />
	public class AndroidVisitor : BaseVisitor<Task<MediaFile>>, IMediaVisitor<Task<MediaFile>>
	{
		/// <summary>
		/// Gets the <see cref="Abstractions.Camera"/> facade for Camera2 api.
		/// </summary>
		/// <value>The camera.</value>
		internal Camera Camera { get; }

		/// <inheritdoc />
		/// <summary>
		/// Initializes Common Camera members..
		/// </summary>
		/// <param name="options">Camera options.</param>
		/// <param name="cameraOperationType">The camera operation type (photo or video).</param>
		protected AndroidVisitor(StoreMediaOptions options, OperationType cameraOperationType) :
			base(options, cameraOperationType) => Camera = CameraFactory.CreateCamera(CameraOperationType, options);

		#region Visitors

		/// <inheritdoc />
		/// <summary>
		/// Visit's <see cref="T:Plugin.Media.Abstractions.IMedia" /> implementations to collect their private data.
		/// </summary>
		/// <param name="data">Private data of <see cref="T:Plugin.Media.Abstractions.IMedia" /> implementations to be used by visitor.</param>
		/// <returns>Required file type.</returns>
		public virtual Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data) =>
			Camera.TakeMedia(data);

		/// <summary>
		/// Visits the specified visitable.
		/// </summary>
		/// <param name="visitable">The visitable.</param>
		/// <returns>Task&lt;MediaFile&gt;.</returns>
		/// <exception cref="NotImplementedException"></exception>
		/// <inheritdoc cref="IVisitor{T}.Visit(IVisitableReturns)" />
		public override Task<MediaFile> Visit(IVisitableReturns visitable)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}