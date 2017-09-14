using System;

namespace Plugin.Media.Abstractions.Custom
{
	public interface IVisitor
	{
		void Visit(IVisitable visitable);
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
		protected readonly OperationType CameraOperationType;

		protected BaseVisitor()
		{
		}

		protected BaseVisitor(StoreMediaOptions options, OperationType cameraOperationType)
		{
			Options = options;
			CameraOperationType = cameraOperationType;
		}

		public abstract T Visit(IVisitableReturns visitable);
		public abstract void Visit(IVisitable visitable);
	}
}