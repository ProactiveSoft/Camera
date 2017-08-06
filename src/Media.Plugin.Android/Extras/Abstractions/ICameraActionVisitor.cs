using System.Threading;

namespace Plugin.Media.Extras.Abstractions
{
	public interface ICameraActionVisitor
	{
		void Visit(ICameraActionVisitable visitable);
	}
}