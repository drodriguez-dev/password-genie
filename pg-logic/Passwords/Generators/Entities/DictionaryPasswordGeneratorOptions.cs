namespace PG.Logic.Passwords.Generators.Entities
{
	public class DictionaryPasswordGeneratorOptions : CommonPasswordGeneratorOptions
	{
		public required string File { get; set; }
		public int NumberOfWords { get; set; }
		public int AverageWordLength { get; set; }
		public int DepthLevel { get; set; }
		public KeystrokeOrder KeystrokeOrder { get; set; }

		public override string ToString() => $"Q: {NumberOfPasswords}, min: {MinimumLength} (W: {NumberOfWords}, N:{NumberOfNumbers}, S:{NumberOfSpecialCharacters})";
	}

	/// <summary>
	/// The order in which keystrokes are generated.
	/// <list type="table">
	///   <item>
	///     <term>Random</term>
	///     <description>Keystrokes are generated in a random order.</description>
	///   </item>
	///   <item>
	///     <term>Alternating</term>
	///     <description>Keystrokes are generated in an alternating pattern between left and right sides of the keyboard.</description>
	///   </item>
	///   <item>
	///     <term>AlternatingWord</term>
	///     <description>Keystrokes are generated in an alternating pattern between left and right sides of the keyboard after each word.</description>
	///   </item>
	///   <item>
	///     <term>OnlyLeft</term>
	///     <description>Keystrokes are generated only from the left side of the keyboard.</description>
	///   </item>
	///   <item>
	///     <term>OnlyRight</term>
	///     <description>Keystrokes are generated only from the right side of the keyboard.</description>
	///   </item>
	/// </list>
	/// </summary>
	/// <remarks>
	/// When the same hand is used for a word, the keystrokes will avoid using the same finger for consecutive characters.
	/// </remarks>
	public enum KeystrokeOrder { Random, AlternatingStroke, AlternatingWord, OnlyLeft, OnlyRight }
}
