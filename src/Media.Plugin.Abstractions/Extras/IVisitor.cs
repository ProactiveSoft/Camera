using System;
using System.Threading.Tasks;

namespace Plugin.Media.Abstractions.Extras
{
	public interface IVisitor
	{
		Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data);
	}

	public abstract class BaseVisitor : IVisitor
	{
		protected readonly StoreMediaOptions Options;

		protected BaseVisitor()
		{
		}

		protected BaseVisitor(StoreMediaOptions options) => Options = options;

		public abstract Task<MediaFile> Visit((bool permission, Action<StoreMediaOptions> verifyOptions, IMedia media) data);
	}
}