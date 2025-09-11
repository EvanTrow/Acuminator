using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Common
{
	public static class StructCollectionsNoBoxingExtensions
	{
		public delegate bool PredicateWithInputByReadOnlyRef<TItem>(in TItem item);
		public delegate TResult FuncWithInputByReadOnlyRef<TItem, TResult>(in TItem item);
		public delegate void ActionWithInputByReadOnlyRef<TItem>(in TItem item);

		/// <summary>
		/// Prepends struct collection <paramref name="source"/> with an <paramref name="itemToAdd"/>.<br/>
		/// This methods prevents additional boxing on convertation of a <typeparamref name="TStructCollection"/> collection to <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TStructCollection">Type of the structure collection.</typeparam>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The struct collection to act on.</param>
		/// <param name="itemToAdd">The item to add.</param>
		/// <returns>
		/// An <see cref="IEnumerable{TItem}"/> that contains the <paramref name="itemToAdd"/> followed by the items in <paramref name="source"/>.
		/// </returns>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TItem> PrependItem<TStructCollection, TItem>(this TStructCollection source, TItem itemToAdd)
		where TStructCollection : struct, IEnumerable<TItem> =>
			source.PrependOrAppend(itemToAdd, isAppending: false);

		/// <summary>
		/// Appends struct collection <paramref name="source"/> with an <paramref name="item"/>.<br/>
		/// This methods prevents additional boxing on convertation of a <typeparamref name="TStructCollection"/> collection to <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The struct collection to act on.</param>
		/// <param name="itemToAdd">The item to add.</param>
		/// <returns>
		/// An <see cref="IEnumerable{TItem}"/> that contains the items in <paramref name="source"/> followed by the <paramref name="itemToAdd"/>.
		/// </returns>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TItem> AppendItem<TStructCollection, TItem>(this TStructCollection source, TItem itemToAdd)
		where TStructCollection : struct, IReadOnlyCollection<TItem> =>
			source.PrependOrAppend(itemToAdd, isAppending: true);

		[DebuggerStepThrough]
		private static IEnumerable<TItem> PrependOrAppend<TStructCollection, TItem>(this TStructCollection source, TItem itemToAdd, bool isAppending)
		where TStructCollection : struct, IEnumerable<TItem>
		{
			if (!isAppending)
				yield return itemToAdd;

			foreach (var item in source)
				yield return item;

			if (isAppending)
				yield return itemToAdd;
		}

		/// <summary>
		/// Concatenate structure list to this collection. This is an optimization method which allows to avoid boxing for collections implemented as structs.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <typeparam name="TStructList">Type of the structure list.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="structList">List implemented as structure.</param>
		/// <returns>
		/// An enumerator that allows foreach to be used to process concatenate structure list in this collection.
		/// </returns>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TItem> ConcatStructList<TItem, TStructList>(this IEnumerable<TItem> source, TStructList structList)
		where TStructList : struct, IReadOnlyCollection<TItem>
		{
			if (source != null)
			{
				foreach (TItem item in source)
				{
					yield return item;
				}
			}

			if (structList.Count > 0)
			{
				foreach (TItem item in structList)
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Concatenate <see cref="ImmutableArray{TItem}"/>s. This is an optimization method which allows to avoid boxing for <see cref="ImmutableArray{TItem}"/>s.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="sourceList">The source list to act on.</param>
		/// <param name="listToConcat">The list to concat.</param>
		/// <returns/>
		[DebuggerStepThrough]
		public static IEnumerable<TItem> Concat<TItem>(this ImmutableArray<TItem> sourceList, ImmutableArray<TItem> listToConcat)
		{
			for (int i = 0; i < sourceList.Length; i++)
				yield return sourceList[i];

			for (int i = 0; i < listToConcat.Length; i++)
				yield return listToConcat[i];
		}

		/// <summary>
		/// Where method for <see cref="SyntaxTokenList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="source">The <see cref="SyntaxTokenList"/> to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxToken> Where(this in SyntaxTokenList source, PredicateWithInputByReadOnlyRef<SyntaxToken> predicate)
		{
			predicate.ThrowOnNull();
			return WhereForSyntaxTokenListImplementation(source, predicate);

			static IEnumerable<SyntaxToken> WhereForSyntaxTokenListImplementation(SyntaxTokenList source, 
																				  PredicateWithInputByReadOnlyRef<SyntaxToken> predicate)
			{
				for (int i = 0; i < source.Count; i++)
				{
					SyntaxToken token = source[i];

					if (predicate(token))
					{
						yield return token;
					}
				}
			}
		}

		/// <summary>
		/// FirstOrDefault method for <see cref="SyntaxTokenList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="source">The <see cref="SyntaxTokenList"/> to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SyntaxToken FirstOrDefault(this in SyntaxTokenList source, PredicateWithInputByReadOnlyRef<SyntaxToken> predicate)
		{
			predicate.ThrowOnNull();

			for (int i = 0; i < source.Count; i++)
			{
				SyntaxToken token = source[i];

				if (predicate(token))
					 return token;
			}

			return default;
		}

		/// <summary>
		/// Where method for <see cref="SyntaxList{TNode}"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TNode">Type of the syntax node.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TNode> Where<TNode>(this SyntaxList<TNode> source, Func<TNode, bool> predicate)
		where TNode : SyntaxNode
		{
			predicate.ThrowOnNull();
			return WhereForStructListImplementation();

			IEnumerable<TNode> WhereForStructListImplementation()
			{
				for (int i = 0; i < source.Count; i++)
				{
					TNode item = source[i];

					if (predicate(item))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Select method for <see cref="SyntaxTriviaList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TResult">Type of the result.</typeparam>
		/// <param name="triviaList">The trivia list to act on.</param>
		/// <param name="selector">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TResult> Select<TResult>(this in SyntaxTriviaList triviaList, 
														  FuncWithInputByReadOnlyRef<SyntaxTrivia, TResult> selector)
		{
			selector.ThrowOnNull();
			return SelectForStructListImplementation(triviaList, selector);

			static IEnumerable<TResult> SelectForStructListImplementation(SyntaxTriviaList triviaList, 
																		  FuncWithInputByReadOnlyRef<SyntaxTrivia, TResult> selector)
			{
				for (int i = 0; i < triviaList.Count; i++)
				{
					yield return selector(triviaList[i]);
				}
			}
		}

		/// <summary>
		/// Where method for <see cref="SyntaxTriviaList"/>. 
		/// This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="triviaList">The trivia list to act on.</param>
		/// <param name="predicate">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia> Where(this in SyntaxTriviaList triviaList,
													  PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate) =>
			WhereImplementation(triviaList, predicate.CheckIfNull());

		/// <summary>
		/// Where method for <see cref="SyntaxTriviaList.Reversed"/>.
		/// This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="reversedTrivia">The reversedTrivia to act on.</param>
		/// <param name="predicate">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia> Where(this in SyntaxTriviaList.Reversed reversedTrivia,
													  PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate) =>
			WhereImplementation(reversedTrivia, predicate.CheckIfNull());

		private static IEnumerable<SyntaxTrivia> WhereImplementation<TStructCollection>(TStructCollection source,
																						PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate)
		where TStructCollection : struct, IEnumerable<SyntaxTrivia>
		{
			foreach (SyntaxTrivia item in source)
			{
				if (predicate(item))
					yield return item;
			}
		}

		/// <summary>
		/// Take method for <see cref="SyntaxTriviaList"/>. This is an optimization method which allows to avoid boxing and allocations in some cases.
		/// </summary>
		/// <param name="triviaList">The trivia list to act on.</param>
		/// <param name="countToTake">The count of elements to take.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SyntaxTriviaList Take(this in SyntaxTriviaList triviaList, int countToTake)
		{
			if (countToTake >= triviaList.Count)
				return triviaList;

			switch (countToTake)
			{
				case <= 0:
					return SyntaxTriviaList.Empty;
				case 1:
					return new SyntaxTriviaList(triviaList[0]);     // Hot path to avoid some allocations
				default:
					var slice = new SyntaxTrivia[countToTake];

					for (int i = 0; i < countToTake; i++)
						slice[i] = triviaList[i];

					return new SyntaxTriviaList(slice);
			}
		}

		/// <summary>
		/// TakeWhile method for <see cref="SyntaxTriviaList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="triviaList">The trivia list to act on.</param>
		/// <param name="predicate">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia> TakeWhile(this in SyntaxTriviaList triviaList,
														  PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate)
		{
			predicate.ThrowOnNull();
			return TakeWhileImplementation(triviaList, predicate);
		}

		/// <summary>
		/// TakeWhile method for <see cref="SyntaxTriviaList.Reversed"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="reversedTrivia">The reversedTrivia to act on.</param>
		/// <param name="predicate">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxTrivia> TakeWhile(this in SyntaxTriviaList.Reversed reversedTrivia,
														  PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate)
		{
			predicate.ThrowOnNull();
			return TakeWhileImplementation(reversedTrivia, predicate);			
		}

		private static IEnumerable<SyntaxTrivia> TakeWhileImplementation<TStructCollection>(TStructCollection source,
																							PredicateWithInputByReadOnlyRef<SyntaxTrivia> predicate)
		where TStructCollection : struct, IEnumerable<SyntaxTrivia>
		{
			foreach (SyntaxTrivia item in source)
			{
				if (predicate(item))
					yield return item;
				else
					yield break;
			}
		}

		/// <summary>
		/// Concat method that appends <see cref="SyntaxTriviaList"/> collection without boxing.<br/>
		/// This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="source">The source collection to act on.</param>
		/// <param name="triviasToAdd">The <see cref="SyntaxTriviaList"/> trivias to add.</param>
		/// <returns/>
		public static IEnumerable<SyntaxTrivia> Concat(this IEnumerable<SyntaxTrivia>? source, in SyntaxTriviaList triviasToAdd)
		{
			if (source == null)
				return triviasToAdd;
			else if (triviasToAdd.Count == 0)
				return source;

			return ConcatImpl(source, triviasToAdd);

			//------------------------------------Local Function-----------------------------------------
			static IEnumerable<SyntaxTrivia> ConcatImpl(IEnumerable<SyntaxTrivia> source, SyntaxTriviaList triviasToAdd)
			{
				foreach (SyntaxTrivia trivia in source)
					yield return trivia;

				for (int i = 0; i < triviasToAdd.Count; i++)
					yield return triviasToAdd[i];
			}
		}

		/// <summary>
		/// Select many implementation for <see cref="ImmutableArray{T}"/> without boxing.
		/// </summary>
		/// <typeparam name="TCollectionHolder">Type of the item with collection.</typeparam>
		/// <typeparam name="TCollectionItem">Type of the collection item.</typeparam>
		/// <param name="array">The array to act on.</param>
		/// <param name="selector">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TCollectionItem> SelectMany<TCollectionHolder, TCollectionItem>(this ImmutableArray<TCollectionHolder> array, 
																								  Func<TCollectionHolder, IEnumerable<TCollectionItem>> selector)
		{
			selector.ThrowOnNull();
			return GeneratorMethod();


			IEnumerable<TCollectionItem> GeneratorMethod()
			{
				foreach (TCollectionHolder collectionHolder in array)
				{
					foreach (TCollectionItem item in selector(collectionHolder))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Reverses <see cref="ImmutableArray{T}"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <returns/>
		[DebuggerStepThrough]
		public static ImmutableArray<TItem?> Reverse<TItem>(this ImmutableArray<TItem?> source)
		{
			if (source.Length == 0)
				return source;

			ImmutableArray<TItem?>.Builder builder = ImmutableArray.CreateBuilder<TItem?>(source.Length);

			for (int i = source.Length - 1; i >= 0; i--)
			{		
				builder.Add(source[i]);
			}

			return builder.ToImmutable();
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this ImmutableArray<T> source, Func<T, bool> condition) =>
			FindIndex(source, startInclusive: 0, endExclusive: source.Length, condition);

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this ImmutableArray<T> source, int startInclusive, Func<T, bool> condition) =>
			FindIndex(source, startInclusive, endExclusive: source.Length, condition);

		[DebuggerStepThrough]
		public static int FindIndex<T>(this ImmutableArray<T> source, int startInclusive, int endExclusive, Func<T, bool> condition)
		{
			condition.ThrowOnNull();

			if (startInclusive < 0 || startInclusive >= source.Length)
				throw new ArgumentOutOfRangeException(nameof(startInclusive));
			else if (endExclusive <= 0 || endExclusive > source.Length)
				throw new ArgumentOutOfRangeException(nameof(endExclusive));

			for (int i = startInclusive; i < endExclusive; i++)
			{
				if (condition(source[i]))
					return i;
			}

			return -1;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<TNode>(this in SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			return FindIndex(source, startInclusive: 0, endExclusive: source.Count, condition);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<TNode>(this in SeparatedSyntaxList<TNode> source, int startInclusive, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			return FindIndex(source, startInclusive, endExclusive: source.Count, condition);
		}

		[DebuggerStepThrough]
		public static int FindIndex<TNode>(this in SeparatedSyntaxList<TNode> source, int startInclusive, int endExclusive, 
										   Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull();

			if (startInclusive < 0 || startInclusive >= source.Count)
				throw new ArgumentOutOfRangeException(nameof(startInclusive));
			else if (endExclusive <= 0 || endExclusive > source.Count)
				throw new ArgumentOutOfRangeException(nameof(endExclusive));

			for (int i = startInclusive; i < endExclusive; i++)
			{
				if (condition(source[i]))
					return i;
			}

			return -1;
		}

		[DebuggerStepThrough]
		public static bool All<TNode>(this in SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull();

			for (int i = 0; i < source.Count; i++)
			{
				if (!condition(source[i]))
					return false;
			}

			return true;
		}

		[DebuggerStepThrough]
		public static bool Any<TNode>(this in SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull();

			for (int i = 0; i < source.Count; i++)
			{
				if (condition(source[i]))
					return true;
			}

			return false;
		}

		[DebuggerStepThrough]
		public static bool Contains<TNode>(this in SyntaxList<TNode> source, TNode node)
		where TNode : SyntaxNode
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (Equals(node, source[i]))
					return true;
			}

			return false;
		}
	}
}