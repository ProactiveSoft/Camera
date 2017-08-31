using Plugin.Media.Abstractions.Extras;

namespace Plugin.Media.Extras.Abstractions
{
	public interface ICameraActionVisitor : IVisitor
	{
		void Visit(ICameraActionVisitable visitable);
	}
}