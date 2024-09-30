﻿using System.Numerics;

namespace PG.Shared.Services
{
	/// <summary>
	/// A service for generating random numbers. It implements all public methods for <see cref="System.Random"/> and keeps track the different possible 
	/// passwords to calculate the entropy.
	/// </summary>
	public class RandomService
	{
		/// <summary>
		/// The seed for the random number generator. It is set to the current tick count of the environment and incremented atomically. It's static to 
		/// ensure that multiple instances of the service don't generate the same random numbers.
		/// </summary>
		private static int _seed = Environment.TickCount;
		private readonly Random _random = new(Interlocked.Increment(ref _seed));

		private BigInteger _possibleCombinations;

		private BigInteger _definitiveCombinations;

		public RandomService()
		{
			ResetEntropy();
		}

		private static int GetPrecission(Type dataType)
		{
			return Type.GetTypeCode(dataType) switch
			{
				TypeCode.Single => 7, // Single (float) has 7 digits of precision
				TypeCode.Double => 15, // Double has 15 digits of precision
				TypeCode.Decimal => (decimal.GetBits(0m)[3] >> 16) & 0xFF, // Decimal precision can vary, so we calculate it dynamically
				_ => throw new ArgumentException($"The data type '{dataType}' is not supported.", nameof(dataType)),
			};
		}

		/// <summary>
		/// Calculates the entropy of the password based on the number of possible passwords.
		/// </summary>
		/// <seealso cref="https://en.wikipedia.org/wiki/Password_strength"/>
		public double GetBitsOfEntropy()
		{
			return BigInteger.Log(_definitiveCombinations, 2);
		}

		private void IncrementEntropy(BigInteger combinations)
		{
			if (combinations <= 0)
				throw new ArgumentOutOfRangeException(nameof(combinations), "The number of combinations must be greater than zero.");

			_possibleCombinations *= combinations;
		}

		public void CommitEntropy()
		{
			_definitiveCombinations *= _possibleCombinations;
			_possibleCombinations = 1;
		}

		public void DiscardEntropy()
		{
			_possibleCombinations = 1;
		}

		public void ResetEntropy()
		{
			_possibleCombinations = 1;
			_definitiveCombinations = 1;
		}

		/// <summary>
		/// Fills the elements of a specified span with items chosen at random from the provided set of choices.
		/// </summary>
		/// <typeparam name="T">The type of span.</typeparam>
		/// <param name="choices">The items to use to populate the span.</param>
		/// <param name="destination">The span to be filled with items.</param>
		/// <exception cref="ArgumentException">Thrown when choices is empty.</exception>
		public void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination)
		{
			int combinations = Math.Max(1, choices.Length);
			IncrementEntropy(combinations);
			_random.GetItems(choices, destination);
		}

		/// <summary>
		/// Creates an array populated with items chosen at random from the provided set of choices.
		/// </summary>
		/// <typeparam name="T">The type of array.</typeparam>
		/// <param name="choices">The items to use to populate the array.</param>
		/// <param name="length">The length of array to return.</param>
		/// <returns>An array populated with random items.</returns>
		/// <exception cref="ArgumentException">Thrown when choices is empty.</exception>
		/// <exception cref="ArgumentNullException">Thrown when choices is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when length is not zero or a positive number.</exception>
		public T[] GetItems<T>(T[] choices, int length)
		{
			int combinations = Math.Max(1, choices.Length * length);
			IncrementEntropy(combinations);
			return _random.GetItems(choices, length);
		}

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than Int32.MaxValue.</returns>
		public virtual int Next()
		{
			IncrementEntropy(int.MaxValue);
			return _random.Next();
		}

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue. However, if maxValue equals 0, 0 is returned.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when maxValue is less than 0.</exception>
		public virtual int Next(int maxValue)
		{
			int combinations = Math.Max(1, maxValue);
			IncrementEntropy(combinations);
			return _random.Next(maxValue);
		}

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
		/// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when minValue is greater than maxValue.</exception>
		public virtual int Next(int minValue, int maxValue)
		{
			int combinations = Math.Max(1, maxValue - minValue);
			IncrementEntropy(combinations);
			return _random.Next(minValue, maxValue);
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">The array to be filled with random numbers.</param>
		/// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
		public virtual void NextBytes(byte[] buffer)
		{
			int combinations = Math.Max(1, buffer.Length);
			IncrementEntropy(combinations);
			_random.NextBytes(buffer);
		}

		/// <summary>
		/// Fills the elements of a specified span of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">The array to be filled with random numbers.</param>
		public virtual void NextBytes(Span<byte> buffer)
		{
			int combinations = Math.Max(1, buffer.Length);
			IncrementEntropy(combinations);
			_random.NextBytes(buffer);
		}

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		/// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
		public virtual double NextDouble()
		{
			IncrementEntropy(BigInteger.Pow(10, GetPrecission(typeof(double))));
			return _random.NextDouble();
		}

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than Int64.MaxValue.</returns>
		public virtual long NextInt64()
		{
			IncrementEntropy(long.MaxValue);
			return _random.NextInt64();
		}

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to 0.</param>
		/// <returns>A 64-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily includes 0 but not maxValue. However, if maxValue equals 0, maxValue is returned.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">maxValue is less than 0.</exception>
		public virtual long NextInt64(long maxValue)
		{
			long combinations = Math.Max(1, maxValue);
			IncrementEntropy(combinations);
			return _random.NextInt64(maxValue);
		}

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="minValue">The inclusive lower bound of the random number returned.</param>
		/// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
		/// <returns>A 64-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">minValue is greater than maxValue.</exception>
		public virtual long NextInt64(long minValue, long maxValue)
		{
			long combinations = Math.Max(1, maxValue - minValue);
			IncrementEntropy(combinations);
			return _random.NextInt64(minValue, maxValue);
		}

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		/// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0./returns>
		public virtual float NextSingle()
		{
			IncrementEntropy(BigInteger.Pow(10, GetPrecission(typeof(float))));
			return _random.NextSingle();
		}

		/// <summary>
		/// Performs an in-place shuffle of an array.
		/// </summary>
		/// <typeparam name="T">The type of array.</typeparam>
		/// <param name="values">The array to shuffle.</param>
		/// <exception cref="ArgumentNullException">Thrown when values is null.</exception>"
		public void Shuffle<T>(T[] values)
		{
			int combinations = Math.Max(1, values.Length);
			IncrementEntropy(combinations);
			_random.Shuffle(values);
		}

		/// <summary>
		/// Performs an in-place shuffle of a span. 
		/// </summary>
		/// <typeparam name="T">The type of span.</typeparam>
		/// <param name="values">The span to shuffle.</param>
		public void Shuffle<T>(Span<T> values)
		{
			int combinations = Math.Max(1, values.Length);
			IncrementEntropy(combinations);
			_random.Shuffle(values);
		}
	}
}