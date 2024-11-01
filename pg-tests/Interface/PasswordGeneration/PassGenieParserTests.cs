using Microsoft.Extensions.DependencyInjection;
using PG.Data.Files.DataFiles;
using PG.Interface.Command.PasswordGeneration;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Loader;
using PG.Shared.Services;

namespace PG.Tests.Interface.PasswordGeneration
{
	[TestClass()]
	public class PassGenieParserTests
	{
		private readonly ServiceProvider _provider;

		public PassGenieParserTests()
		{
			_provider = new ServiceCollection()
				.AddSingleton<PasswordGeneratorFactory>()
				.AddSingleton<IDictionaryLoaderFactory, DictionaryLoaderFactory>()
				.AddSingleton<IDictionariesDataFactory, DictionariesDataFactory>()
				.AddTransient<RandomService>()
				.BuildServiceProvider();
		}

		[TestMethod()]
		public void ParseForRandomTest()
		{
			string[] arguments = [
				"generate", "random",
				"-p", "10",
				"-l", "8",
				"-c", "4",
				"-n", "4",
				"-s", "1",
				"-r",
				"--Verbose",
			];

			int result = new PassGenieParser(_provider).ParseAndExecute(arguments).Result;

			Assert.AreEqual(0, result, "Unexpected result");
		}

		[TestMethod]
		public void ParseForDictionaryTest()
		{
			string[] arguments = [
				"generate", "dictionary",
				"-d", @".\Resources\Dictionaries\words_alpha_esES.txt",
				"-p", "10",
				"-l", "12",
				"-w", "2",
				"-n", "1",
				"-s", "1",
				"-sc", " ",
				"-wl", "6",
				"-wd", "4",
				"-r",
				"--Verbose",
			];

			int result = new PassGenieParser(_provider).ParseAndExecute(arguments).Result;

			Assert.AreEqual(0, result, "Unexpected result");
		}

		[TestMethod]
		public void ParseForWordTreeTest()
		{
			string[] arguments = [
				"generate", "dictionary",
				"-wt", @".\Resources\Dictionaries\word_tree_esES.dat.gz",
				"-p", "10",
				"-l", "12",
				"-w", "2",
				"-n", "1",
				"-s", "1",
				"-sc", " ",
				"-wl", "6",
				"-wd", "4",
				"-r",
				"--Verbose",
			];

			int result = new PassGenieParser(_provider).ParseAndExecute(arguments).Result;

			Assert.AreEqual(0, result, "Unexpected result");
		}
	}
}