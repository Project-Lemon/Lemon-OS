using Cosmos.System;

namespace PrismAPI.Tools.Extentions;

/// <summary>
/// Keyboard conversion class.
/// </summary>
public static class KeyboardEx
{
	#region Constructors

    /// <summary>
    /// Initializes the KeyboardEx class uppon access.
    /// </summary>
    static KeyboardEx()
    {
        // Initialize callback list.
        Callbacks = new();

        // Create timer to call every 150 MS.
        Timer T = new((_) =>
        {
            if (TryReadKey(out ConsoleKeyInfo Key))
            {
                for (int I = 0; I < Callbacks.Count; I++)
                {
                    Callbacks[I].Invoke(Key);
                }
            }
        }, null, 0, 150);
    }

	#endregion

    #region Methods

    /// <summary>
    /// Registers a new callback to an event to be fired whenever a key is pressed.
    /// </summary>
    /// <param name="CallBack">Action to call uppon a key press.</param>
    public static void RegisterCallback(Action<ConsoleKeyInfo> CallBack)
    {
        Callbacks.Add(CallBack);
    }

    /// <summary>
    /// Attempts to read a key, returns true if a key is pressed.
    /// </summary>
    /// <param name="Key">Key read, if key is available.</param>
    /// <returns>True when key is read.</returns>
    public static bool TryReadKey(out ConsoleKeyInfo Key)
    {
        if (KeyboardManager.TryReadKey(out var KeyX))
        {
            Key = new(KeyX.KeyChar, KeyX.Key.ToConsoleKey(), KeyX.Modifiers == ConsoleModifiers.Shift, KeyX.Modifiers == ConsoleModifiers.Alt, KeyX.Modifiers == ConsoleModifiers.Control);
            return true;
        }

        Key = default;
        return false;
    }

    /// <summary>
    /// A non-blocking key read method.
    /// </summary>
    /// <returns>The currently pressed key, or null if none is pressed.</returns>
    public static ConsoleKeyInfo? ReadKey()
    {
		if (TryReadKey(out ConsoleKeyInfo Key))
		{
			return Key;
		}

		return null;
    }

    #endregion

    #region Fields

    private static readonly List<Action<ConsoleKeyInfo>> Callbacks;

    #endregion
}