

public class HtmlElement
{
	private string _html;
	public HtmlElement(string Html)
	{
		_html = Html;
	}
}

public abstract class ViewBase<T>
{
	private HtmlElement? _element = null;
	protected T Data;
	public ViewBase(T data)
	{
		Data = data;
	}

	public HtmlElement GetElement()
	{
		if (_element is null) throw new Exception("You must call render() before accessing the element.");

		return _element;
	}

	public abstract HtmlComponent Template();

	public HtmlElement Render()
	{
		var html = Template().ToHtml();
		_element = new HtmlElement(html);
		WriteLine($"OnRender: {html}");
		// Assumption: Render HTML
		OnRender();
		return _element!;
	}

	protected virtual void OnRender() { }
}
