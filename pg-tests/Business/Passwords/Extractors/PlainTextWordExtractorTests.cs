using Microsoft.Extensions.DependencyInjection;
using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Extractors;
using PG.Logic.Passwords.Extractors.Entities;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Loaders;
using PG.Shared.Services;
using System.Text;

namespace PG.Tests.Business.Passwords.Extractors
{
	[TestClass()]
	public class PlainTextWordExtractorTests
	{
		private readonly ServiceProvider _provider;
		private readonly WordExtractorFactory _factory;

		public PlainTextWordExtractorTests()
		{
			_provider = new ServiceCollection()
				.AddSingleton<PasswordGeneratorFactory>()
				.AddSingleton<WordExtractorFactory>()
				.AddSingleton<IDictionaryLoaderFactory, DictionaryLoaderFactory>()
				.AddSingleton<IDictionariesDataFactory, DictionariesDataFactory>()
				.AddTransient<RandomService>()
				.BuildServiceProvider();

			_factory = new WordExtractorFactory(_provider);
		}

		[TestMethod()]
		public void ExtractWordTreeTest()
		{
			// Load some words into a Memory stream, each word separated by a new line.
			var text = string.Join(Environment.NewLine, ["aone", "atwo", "athree"]);
			MemoryStream input = new(Encoding.UTF8.GetBytes(text));
			MemoryStream output = new();

			_factory.Create(DictionaryFormat.Plain).ExtractWordTree(input, output);

			MemoryStream outputStreamCopy = new(output.ToArray());
			WordDictionaryTree tree = new BinaryWordTreeFile().FetchTree(outputStreamCopy);

			ITreeNode<string> node = tree.Root;
			Assert.AreEqual(1, node.Children.Count, "Unexpected letter count for first level");
			Assert.AreEqual("a", node.Children.First().Key, "Unexpected letter for first level");

			node = node.Children.First().Value;
			Assert.AreEqual(2, node.Children.Count, "Unexpected letter count for first level");
			Assert.AreEqual("o", node.Children.First().Key, "Unexpected letter for first level");
		}
	}
}