public record User(string Name, int Age);

public class UserView : ViewBase<User>
{
	public UserView(User data) : base(data) { }

	public override HtmlComponent Template()
	{
		return TemplateEngine.Html($"""
		<div>
			{Data.Name} ({Data.Age})
			<button>x</button>
		</div>
		""");
	}

	protected override void OnRender()
	{
	}
}
