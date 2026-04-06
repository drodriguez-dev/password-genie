using PG.Logic.Passwords.Generators;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class DictionaryPasswordGeneratorV1Tests : DictionaryPasswordGeneratorTestsBase<DictionaryPasswordGeneratorV1>
	{
		[ClassInitialize]
		public static new void ClassInitialize(TestContext testContext)
		{
			DictionaryPasswordGeneratorTestsBase<DictionaryPasswordGeneratorV1>.ClassInitialize(testContext);
		}
	}
}