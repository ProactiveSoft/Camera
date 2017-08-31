using System;
using System.Threading.Tasks;

namespace Plugin.Media.Abstractions.Extras
{
	public interface IVisitor
	{
		void Visit(IVisitable visitable);
	}

	/// <summary>
	/// Visitor for passing camera options.
	/// </summary>
	/// <seealso cref="Plugin.Media.Abstractions.Extras.IVisitor" />
	public interface ICameraOptionsVisitor : IVisitor
	{
		/// <summary>
		/// Passes options to classes.
		/// </summary>
		/// <param name="options">Camera options.</param>
		void Visit(StoreMediaOptions options);
	}

	public interface IVisitor<T> : IVisitor
	{
		T Visit(IVisitableReturns visitable);
	}

	public interface ICameraVisitor<T> : IVisitor<T>
	{
		T Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data);
	}

	public abstract class BaseVisitor<T> : IVisitor<T>
	{
		protected readonly StoreMediaOptions Options;

		protected BaseVisitor()
		{
		}

		protected BaseVisitor(StoreMediaOptions options) => Options = options;

		public abstract T Visit(IVisitableReturns visitable);
		public abstract void Visit(IVisitable visitable);
	}
}