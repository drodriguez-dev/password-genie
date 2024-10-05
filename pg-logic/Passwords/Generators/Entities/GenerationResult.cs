namespace PG.Logic.Passwords.Generators.Entities
{
	public class GenerationResult
	{
		public string[] Passwords { get; set; } = [];
		public double AverageEntropy { get; set; }
	}
}
