namespace Accessibility
{
	/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
	public interface IAccessible
	{
		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <returns>An integer representing the count.</returns>
		int accChildCount { get; }

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <returns>If successful, returns S_OK. Otherwise, returns another standard COM error code.</returns>
		object accFocus { get; }

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <returns>An object.</returns>
		object accParent { get; }

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <returns>An object.</returns>
		object accSelection { get; }

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <param name="varChild">This parameter is intended for internal use only.</param>
		void accDoDefaultAction(object childID);

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <param name="xLeft">This parameter is intended for internal use only.</param>
		/// <param name="yTop">This parameter is intended for internal use only.</param>
		/// <returns>An object.</returns>
		object accHitTest(int xLeft, int yTop);

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <param name="pxLeft">This parameter is intended for internal use only.</param>
		/// <param name="pyTop">This parameter is intended for internal use only.</param>
		/// <param name="pcxWidth">This parameter is intended for internal use only.</param>
		/// <param name="pcyHeight">This parameter is intended for internal use only.</param>
		/// <param name="varChild">This parameter is intended for internal use only.</param>
		void accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, object childID);

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <param name="navDir">This parameter is intended for internal use only.</param>
		/// <param name="varStart">This parameter is intended for internal use only.</param>
		/// <returns>If successful, returns S_OK. For other possible return values, see the documentation for <see langword="IAccessible::accNavigate" />.</returns>
		object accNavigate(int navDir, object childID);

		/// <summary>The <see cref="T:Accessibility.IAccessible" /> interface and all of its exposed members are part of a managed wrapper for the Component Object Model (COM) <see langword="IAccessible" /> interface.</summary>
		/// <param name="flagsSelect">This parameter is intended for internal use only.</param>
		/// <param name="varChild">This parameter is intended for internal use only.</param>
		void accSelect(int flagsSelect, object childID);

		object get_accChild(object childID);

		string get_accDefaultAction(object childID);

		string get_accDescription(object childID);

		string get_accHelp(object childID);

		int get_accHelpTopic(out string pszHelpFile, object childID);

		string get_accKeyboardShortcut(object childID);

		string get_accName(object childID);

		object get_accRole(object childID);

		object get_accState(object childID);

		string get_accValue(object childID);

		void set_accName(object childID, string newName);

		void set_accValue(object childID, string newValue);
	}
}
