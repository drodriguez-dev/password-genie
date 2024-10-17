using PG.Entities.WordTrees;
using System.IO.Compression;
using System.Text;

namespace PG.Data.Files.DataFiles.WordTrees
{
	/// <summary>
	/// Saves and loads a word tree to and from a binary file. The structure of the file is:
	///   (4 bytes) Magic number
	///   (1 byte) Version
	///   Repetable structure of:
	///     (2 bytes) Number of children
	///     (N bytes depending on the encoding) Character
	///     (Recursively) Children
	/// </summary>
	/// <param name="fileStream"></param>
	public class BinaryWordTreeFile() : IWordTreeData
	{
		private const int MAGIC_NUMBER = 0x10bfaa38; // Reversed in file
		private const byte CURRENT_VERSION = 1;

		private struct FileHeader
		{
			public int MagicNumber;
			public byte Version;

			public FileHeader()
			{
				MagicNumber = MAGIC_NUMBER;
				Version = CURRENT_VERSION;
			}
		}

		public void SaveTree(Stream fileStream, WordDictionaryTree tree)
		{
			fileStream.SetLength(0);
			using GZipStream zipStream = new(fileStream, CompressionLevel.SmallestSize);
			WriteTree(zipStream, new FileHeader(), tree);
		}

		private static void WriteTree(Stream fileStream, FileHeader header, WordDictionaryTree tree)
		{
			using BinaryWriter writer = new(fileStream, Encoding.UTF8);
			writer.Write(header.MagicNumber);
			writer.Write(header.Version);

			// Serialize the tree
			WriteNode(writer, tree.Root);
		}

		/// <summary>
		/// Recursively writes the tree to the stream. The key is stored as a single char, which can be a surrogate pair (2 bytes).
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="node"></param>
		private static void WriteNode(BinaryWriter writer, ITreeNode<string> node)
		{
			writer.Write((short)node.Children.Count);
			foreach (var child in node.Children)
			{
				writer.Write(child.Key);

				WriteNode(writer, child.Value);
			}
		}

		public WordDictionaryTree FetchTree(Stream fileStream)
		{
			using GZipStream zipStream = new(fileStream, CompressionMode.Decompress);
			using BinaryReader reader = new(zipStream, Encoding.UTF8);

			// Read and validate the header
			FileHeader header = new()
			{
				MagicNumber = reader.ReadInt32(),
				Version = reader.ReadByte()
			};

			if (header.MagicNumber != MAGIC_NUMBER)
				throw new InvalidDataException("Invalid file format.");

			if (header.Version != CURRENT_VERSION)
				throw new InvalidDataException("Unsupported file version, expected version " + CURRENT_VERSION);

			// Deserialize the tree
			WordDictionaryTree tree = new();
			ReadNode(reader, tree.Root);

			return tree;
		}

		private static void ReadNode(BinaryReader reader, ITreeNode<string> node)
		{
			int childrenCount = reader.ReadInt16();
			for (int i = 0; i < childrenCount; i++)
			{
				string key = reader.ReadString();
				node.Children[key] = new TreeNode<string>(key);
				ReadNode(reader, node.Children[key]);
			}
		}
	}
}
