namespace PrismAPI.Tools
{
	public unsafe class EventHandler
	{
		/// <summary>
		/// Creates a new instance of the <see cref="EventHandler"/> class.
		/// </summary>
		public EventHandler(ref bool Condition, Action Method)
		{
			fixed (bool* ConditionRef = &Condition)
			{
				this.Condition = ConditionRef;
			}
			this.Method = Method;

			Events.Add(this);
		}

		/// <summary>
		/// Initializes the event handler when the class is first accesed.
		/// </summary>
		static EventHandler()
		{
			Events = new();

			Timer T = new((_) => HandleAllEvents(), null, 0, 150);
		}

		#region Methods

		/// <summary>
		/// Processes all events in the event handles list.
		/// </summary>
		public static void HandleAllEvents()
		{
			foreach (EventHandler E in Events)
			{
				if (E.Condition == null)
				{
					Events.Remove(E);
					continue;
				}
				if (*E.Condition)
				{
					*E.Condition = false;
					E.Method.Invoke();
				}
			}
		}

		#endregion

		#region Fields

		public static readonly List<EventHandler> Events;

		public bool* Condition;
		public Action Method;

		#endregion
	}
}