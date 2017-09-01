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
using Plugin.Media.Abstractions.Extras;
using Plugin.Media.Extras.CameraWithoutConfirmation;

namespace Plugin.Media.Extras.Abstractions
{
	/// <summary>
	/// Visitor to collect <see cref="AndroidBaseVisitor"/>'s private members
	/// </summary>
	/// <seealso cref="Plugin.Media.Abstractions.Extras.IVisitor" />
	public interface IAndroidBaseVisitor : IVisitor
	{
		/// <summary>
		/// Gets <see cref="AndroidBaseVisitor"/>'s private members.
		/// </summary>
		/// <param name="cameraOpenCloseLock">The camera open close lock.</param>
		void Visit(Semaphore cameraOpenCloseLock);
	}
}