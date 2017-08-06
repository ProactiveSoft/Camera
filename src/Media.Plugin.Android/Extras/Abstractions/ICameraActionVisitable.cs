namespace Plugin.Media.Extras.Abstractions
{
	public interface ICameraActionVisitable
	{
		void Accept(ICameraActionVisitor visitor);
	}
}