// ***********************************************************************
// Assembly         : Plugin.Media
// Author           : anila
// Created          : 09-01-2017
//
// Last Modified By : anila
// Last Modified On : 09-01-2017
// ***********************************************************************
// <copyright file="IAndroidBaseVisitor.cs" company="">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading;
using Media.Plugin.Custom.Android.CameraWithoutConfirmation;
using Plugin.Media.Abstractions.Custom;

namespace Media.Plugin.Custom.Android.Abstractions
{
	/// <summary>
	/// Visitor to collect <see cref="AndroidBaseVisitor"/>'s private members
	/// </summary>
	/// <seealso cref="IVisitor" />
	public interface IAndroidBaseVisitor : IVisitor
	{
		/// <summary>
		/// Gets <see cref="AndroidBaseVisitor"/>'s private members.
		/// </summary>
		/// <param name="cameraOpenCloseLock">The camera open close lock.</param>
		void Visit(SemaphoreSlim cameraOpenCloseLock);
	}
}