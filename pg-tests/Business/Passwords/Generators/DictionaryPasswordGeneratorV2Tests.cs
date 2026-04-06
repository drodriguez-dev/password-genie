using PG.Logic.Passwords.Generators;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class DictionaryPasswordGeneratorV2Tests : DictionaryPasswordGeneratorTestsBase<DictionaryPasswordGeneratorV2>
	{

		[ClassInitialize]
		public static new void ClassInitialize(TestContext testContext)
		{
			DictionaryPasswordGeneratorTestsBase<DictionaryPasswordGeneratorV2>.ClassInitialize(testContext);
		}
	}
}