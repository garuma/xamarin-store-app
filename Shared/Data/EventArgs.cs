using System;

public class EventArgs<T> : EventArgs
{
	public T Data { get; set; }

	public EventArgs (T data)
	{
		Data = data;
	}

	public static EventArgs<T> From (T data)
	{
		return new EventArgs<T> (data);
	}
}

