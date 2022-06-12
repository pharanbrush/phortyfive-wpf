using System;
using System.Collections.Generic;

namespace PhortyFiveSeconds;

// This class manages being able to cycle through a shuffled order.
// It primarily serves shuffled indices given a size,
// with the ability to go backwards and forwards in the shuffled list.
public class Circulator
{
	readonly List<int> reorderedIndexes = new();
	int CurrentNumber { get; set; } = 0;

	public int CurrentIndex => IsPopulated ? reorderedIndexes[CurrentNumber] : 0;
	public int Count => reorderedIndexes.Count;
	public int MaxNumber => Count - 1;
	public bool IsPopulated => Count > 0;

	public event Action? OnCurrentNumberChanged;

	public void MoveNext () => MoveCurrentNumberBy(1);

	public void MovePrevious () => MoveCurrentNumberBy(-1);

	public void StartNewOrder (int count)
	{
		ResetToDefaultOrder(count);
		GenerateShuffledOrder();
		SetCurrentNumber(0);
	}

	public void Clear ()
	{
		reorderedIndexes.Clear();
		SetCurrentNumber(0);
	}

	void ResetToDefaultOrder (int count)
	{
		reorderedIndexes.Clear();
		for (int i = 0; i < count; i++)
		{
			reorderedIndexes.Add(i);
		}
	}

	void GenerateShuffledOrder ()
	{
		Shuffle(reorderedIndexes);
	}

	void MoveCurrentNumberBy (int increment)
	{
		int newValue = CurrentNumber + increment;

		int max = MaxNumber;
		if (newValue > max) newValue = 0;
		if (newValue < 0) newValue = max;

		SetCurrentNumber(newValue);
	}

	void SetCurrentNumber (int newValue)
	{
		CurrentNumber = newValue;
		OnCurrentNumberChanged?.Invoke();
	}

	// Fisher-Yates Shuffle Algorithm
	// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
	// Adapted by https://www.delftstack.com/howto/csharp/shuffle-a-list-in-csharp/
	static void Shuffle<T> (IList<T> list)
	{
		Random rng = new();
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			(list[n], list[k]) = (list[k], list[n]);
		}
	}
}
