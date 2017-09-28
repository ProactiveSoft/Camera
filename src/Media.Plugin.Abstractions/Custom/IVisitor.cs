// ***********************************************************************
// Assembly         : Plugin.Media.Abstractions
// Author           : anila
// Created          : 07-29-2017
//
// Last Modified By : anila
// Last Modified On : 09-28-2017
// ***********************************************************************
// <copyright file="IVisitor.cs" company="Plugin.Media.Abstractions">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace Plugin.Media.Abstractions.Custom
{
	/// <summary>
	/// Simple Visitor interface.
	/// </summary>
	public interface IVisitor
	{
		/// <summary>
		/// Visits the specified visitable.
		/// </summary>
		/// <param name="visitable">The visitable.</param>
		void Visit(IVisitable visitable);
	}

	/// <inheritdoc />
	/// <summary>
	/// Visitor interface with method returning value.
	/// </summary>
	/// <typeparam name="T">Data type to be returned.</typeparam>
	/// <seealso cref="T:Plugin.Media.Abstractions.Custom.IVisitor" />
	public interface IVisitor<out T> : IVisitor
	{
		/// <summary>
		/// Visits the specified visitable.
		/// </summary>
		/// <param name="visitable">The visitable.</param>
		/// <returns>Data type to be returned.</returns>
		T Visit(IVisitableReturns visitable);
	}

	/// <inheritdoc />
	/// <summary>
	/// Base Visitor for different <see cref="T:Plugin.Media.Abstractions.IMedia" /> implementations.
	/// Used for common Visitor members.
	/// </summary>
	/// <typeparam name="T">Data type of returned media file.</typeparam>
	/// <seealso cref="T:Plugin.Media.Abstractions.Custom.IVisitor`1" />
	public abstract class BaseVisitor<T> : IVisitor<T>
	{
		/// <summary>
		/// Camera options.
		/// </summary>
		protected readonly StoreMediaOptions Options;
		/// <summary>
		/// The camera operation type (photo or video).
		/// </summary>
		protected readonly OperationType CameraOperationType;

		/// <summary>
		/// Initializes Common Camera members..
		/// </summary>
		/// <param name="options">Camera options.</param>
		/// <param name="cameraOperationType">The camera operation type (photo or video).</param>
		protected BaseVisitor(StoreMediaOptions options, OperationType cameraOperationType)
		{
			Options = options;
			CameraOperationType = cameraOperationType;
		}


		/// <inheritdoc />
		public abstract T Visit(IVisitableReturns visitable);

		/// <inheritdoc />
		public virtual void Visit(IVisitable visitable) { }
	}

	/// <inheritdoc />
	/// <summary>
	/// Visitor for <see cref="T:Plugin.Media.Abstractions.IMedia" /> imaplementations.
	/// </summary>
	/// <typeparam name="T">Returned file type.</typeparam>
	/// <seealso cref="T:Plugin.Media.Abstractions.Custom.IVisitor`1" />
	public interface IMediaVisitor<out T> : IVisitor<T>
	{
		/// <summary>
		/// Visit's <see cref="IMedia" /> implementations to collect their private data.
		/// </summary>
		/// <param name="data">Private data of <see cref="IMedia" /> implementations to be used by visitor.</param>
		/// <returns>Required file type.</returns>
		T Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data);
	}
}