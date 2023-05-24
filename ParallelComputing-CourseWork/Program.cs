using ParallelComputing_CourseWork;
using System;
using System.Collections.Concurrent;

class Program
{
    const int size = 1000000;
    static void Main(string[] args)
    {
        Entity[] entities = GenerateRandomEntities(size);

        //Console.WriteLine("Before Sorting:");
        //PrintEntities(entities.Take(50));

        var watch = System.Diagnostics.Stopwatch.StartNew();
        //code
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Time spent: {elapsedMs}");

        //Console.WriteLine("After Sorting:");
        //PrintEntities(entities.Take(20000));

        Console.WriteLine($"Correct: {IsSorted(entities).ToString()}");

        Console.WriteLine(Test_SmallArray() ? "SmallArray Passed" : "SmallArray Failed");
        Console.WriteLine(Test_BigArray() ? "BigArray Passed" : "SmallArray Failed");
        Console.WriteLine(Test_SortedArray() ? "SortedArray Passed" : "SmallArray Failed");
        Console.WriteLine(Test_EmptyArray() ? "EmptyArray Passed" : "SmallArray Failed");
        Console.WriteLine(Test_OneElementArray() ? "OneElementArray Passed" : "SmallArray Failed");

        //int processorCount = Environment.ProcessorCount;
        //Console.WriteLine("Number of processor cores: " + processorCount);
    }

    static bool Test_SmallArray()
    {
        Random random = new Random();
        int arraySize = random.Next(1000,100000);

        Entity[] entities = GenerateRandomEntities(arraySize);

        entities = ModifiedParallelPyramidSort(entities);

        return IsSorted(entities) ? true : false;
    }

    static bool Test_BigArray()
    {
        Random random = new Random();
        int arraySize = random.Next(100000, 10000000);

        Entity[] entities = GenerateRandomEntities(arraySize);

        entities = ModifiedParallelPyramidSort(entities);

        return IsSorted(entities) ? true : false;
    }

    static bool Test_SortedArray()
    {
        Random random = new Random();
        int arraySize = random.Next(1000, 10000000);

        Entity[] entities = GenerateRandomEntities(arraySize);

        Array.Sort(entities, delegate (Entity x, Entity y) { return x.Number.CompareTo(y.Number); });

        entities = ModifiedParallelPyramidSort(entities);

        return IsSorted(entities) ? true : false;
    }

    static bool Test_EmptyArray()
    {
        Entity[] entities = new Entity[0];

        entities = ModifiedParallelPyramidSort(entities);

        return entities.Length == 0 ? true : false;
    }

    static bool Test_OneElementArray()
    {
        Random random = new Random();
        int value = random.Next(1, 1000000);

        Entity[] entities = new Entity[1];
        entities[0] = new Entity(value);

        entities = ModifiedParallelPyramidSort(entities);

        return entities.Length == 1 && entities[0].Number == value ? true : false;
    }

    static bool IsSorted(Entity[] entities)
    {
        for (int i = 1; i < entities.Length; i++)
        {
            if (entities[i].Number < entities[i - 1].Number)
            {
                return false;
            }
        }

        return true;
    }

    static Entity[] ModifiedPyramidSort(Entity[] entities)
    {
        int n = entities.Length;

        List<Entity[]> smallerArrays = DivideIntoSmallerArrays(entities);


        foreach (var smallerArray in smallerArrays)
        {
            PyramidSort(smallerArray);
        }

        while (smallerArrays.Count > 1)
        {
            List<Entity[]> mergedArrays = new List<Entity[]>();

            for (int i = 0; i < smallerArrays.Count - 1; i += 2)
            {
                Entity[] mergedArray = Merge(smallerArrays[i], smallerArrays[i + 1]);
                mergedArrays.Add(mergedArray);
            }

            if (smallerArrays.Count % 2 == 1)
            {
                mergedArrays.Add(smallerArrays[smallerArrays.Count - 1]);
            }

            smallerArrays = mergedArrays;
        }

        return smallerArrays[0];
    }


    static Entity[] ModifiedParallelPyramidSort(Entity[] entities)
    {
        int n = entities.Length;

        List<Entity[]> smallerArrays = DivideIntoSmallerArrays(entities);


        Parallel.ForEach(smallerArrays, smallerArray =>
        {
            PyramidSort(smallerArray);
        });

        while (smallerArrays.Count > 1)
        {
            List<Entity[]> mergedArrays = new List<Entity[]>();

            for (int i = 0; i < smallerArrays.Count - 1; i += 2)
            {
                Entity[] mergedArray = Merge(smallerArrays[i], smallerArrays[i + 1]);
                mergedArrays.Add(mergedArray);
            }

            if (smallerArrays.Count % 2 == 1)
            {
                mergedArrays.Add(smallerArrays[smallerArrays.Count - 1]);
            }

            smallerArrays = mergedArrays;
        }

        return smallerArrays[0];
    }

    static List<Entity[]> DivideIntoSmallerArrays(Entity[] entities)
    {
        int numSmallerArrays = 4;
        int smallerArraySize = (int)Math.Ceiling((double)entities.Length / numSmallerArrays);

        List<Entity[]> smallerArrays = new List<Entity[]>(numSmallerArrays);

        for (int i = 0; i < numSmallerArrays; i++)
        {
            int startIndex = i * smallerArraySize;
            int endIndex = Math.Min(startIndex + smallerArraySize, entities.Length);
            int smallerArrayLength = endIndex - startIndex;

            Entity[] smallerArray = new Entity[smallerArrayLength];
            Array.Copy(entities, startIndex, smallerArray, 0, smallerArrayLength);

            smallerArrays.Add(smallerArray);
        }

        return smallerArrays;
    }

    static Entity[] Merge(Entity[] leftArray, Entity[] rightArray)
    {
        int leftLength = leftArray.Length;
        int rightLength = rightArray.Length;
        int mergedLength = leftLength + rightLength;
        Entity[] mergedArray = new Entity[mergedLength];

        int leftIndex = 0;
        int rightIndex = 0;
        int mergedIndex = 0;

        while (leftIndex < leftLength && rightIndex < rightLength)
        {
            if (leftArray[leftIndex].Number < rightArray[rightIndex].Number)
            {
                mergedArray[mergedIndex++] = leftArray[leftIndex++];
            }
            else
            {
                mergedArray[mergedIndex++] = rightArray[rightIndex++];
            }
        }

        while (leftIndex < leftLength)
        {
            mergedArray[mergedIndex++] = leftArray[leftIndex++];
        }

        while (rightIndex < rightLength)
        {
            mergedArray[mergedIndex++] = rightArray[rightIndex++];
        }

        return mergedArray;
    }

    static void PyramidSort(Entity[] entities)
    {
        int n = entities.Length;

        for (int i = n / 2 - 1; i >= 0; i--)
        {
            Heapify(entities, n, i);
        }

        for (int i = n - 1; i > 0; i--)
        {
            Entity temp = entities[0];
            entities[0] = entities[i];
            entities[i] = temp;

            Heapify(entities, i, 0);
        }
    }

    static void Heapify(Entity[] entities, int n, int i)
    {
        int largest = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < n && entities[left].Number > entities[largest].Number)
        {
            largest = left;
        }

        if (right < n && entities[right].Number > entities[largest].Number)
        {
            largest = right;
        }

        if (largest != i)
        {
            Entity swap = entities[i];
            entities[i] = entities[largest];
            entities[largest] = swap;

            Heapify(entities, n, largest);
        }
    }

    static Entity[] GenerateRandomEntities(int size)
    {
        Random random = new Random();
        Entity[] entities = new Entity[size];

        for (int i = 0; i < size; i++)
        {
            int randomNumber = random.Next(1000000);
            entities[i] = new Entity(randomNumber);
        }

        return entities;
    }

    static List<Entity> GenerateRandomListOfEntities(int size)
    {
        Random random = new Random();
        List<Entity> entities = new List<Entity>();

        for (int i = 0; i < size; i++)
        {
            int randomNumber = random.Next(1000000);
            entities.Add(new Entity(randomNumber));
        }

        return entities;
    }

    static void PrintEntities(IEnumerable<Entity> entities)
    {
        Console.WriteLine();
        foreach (Entity entity in entities)
        {
            Console.Write(entity.Number + ", ");
        }
        Console.WriteLine();
    }

    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    Parallel.For(n / 2 - 1, 0, i =>
    //    {
    //        ParallelHeapify(entities, n, i);
    //    });

    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        Entity temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        Heapify(entities, i, 0);
    //    }
    //}

    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity swap = entities[i];
    //        entities[i] = entities[largest];
    //        entities[largest] = swap;

    //        Task leftChildTask = Task.Factory.StartNew(() =>
    //        {
    //            ParallelHeapify(entities, n, largest);
    //        });

    //        leftChildTask.Wait();
    //    }
    //}
    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    // Build max heap in parallel
    //    //for (int i = n / 2 - 1; i >= 0; i--)
    //    //{
    //    //    ParallelHeapify(entities, n, i);
    //    //}

    //    Parallel.For(0, n / 2, i =>
    //    {
    //        ParallelHeapify(entities, n, i);
    //    });

    //    // Perform sorting in parallel with synchronization
    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        // Swap the root (maximum element) with the current element
    //        Entity temp;
    //        temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;
    //        // Rebuild the heap in parallel with reduced size
    //        ParallelHeapify(entities, i, 0);
    //    }
    //}

    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity swap;
    //        lock (entities)
    //        {
    //            swap = entities[i];
    //            entities[i] = entities[largest];
    //            entities[largest] = swap;
    //        }

    //        ParallelHeapify(entities, n, largest);

    //        //Parallel.Invoke(
    //        //    () => ParallelHeapify(entities, n, largest)
    //        //);
    //    }
    //}

    //int n = entities.Length;

    //Parallel.For(n / 2 - 1, 0, i =>
    //{
    //    ParallelHeapify(entities, n, i);
    //});

    //for (int i = n / 2 - 1; i >= 0; i--)
    //{
    //    Heapify(entities, n, i);
    //}

    //for (int i = n - 1; i > 0; i--)
    //{
    //    Entity temp = entities[0];
    //    entities[0] = entities[i];
    //    entities[i] = temp;

    //    Heapify(entities, i, 0);
    //}



    //static void ParallelPyramidSort(Entity[] entities)
    //{

    //    int n = entities.Length;

    //    Parallel.Invoke(() =>
    //    {
    //        for (int i = n / 2 - 1; i >= 0; i--)
    //        {
    //            ParallelHeapify(entities, n, i);
    //        }
    //    });

    //    Parallel.Invoke(() =>
    //    {
    //        for (int i = n - 1; i > 0; i--)
    //        {
    //            Entity temp;
    //            temp = entities[0];
    //            entities[0] = entities[i];
    //            entities[i] = temp;

    //            ParallelHeapify(entities, i, 0);
    //        }
    //    });
    //}

    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity temp;
    //        do
    //        {
    //            temp = entities[i];
    //            entities[i] = Interlocked.Exchange(ref entities[largest], temp);
    //            i = largest;
    //            left = 2 * i + 1;
    //            right = 2 * i + 2;

    //            if (left < n && entities[left].Number > entities[largest].Number)
    //            {
    //                largest = left;
    //            }

    //            if (right < n && entities[right].Number > entities[largest].Number)
    //            {
    //                largest = right;
    //            }

    //        } while (largest != i);
    //    }
    //}

    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    Parallel.Invoke(() =>
    //    {
    //        for (int i = n / 2 - 1; i >= 0; i--)
    //        {
    //            ParallelHeapify(entities, n, i);
    //        }
    //    });


    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        Entity temp;
    //        temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        Heapify(entities, i, 0);
    //    }
    //}



    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity temp;
    //        do
    //        {
    //            temp = entities[i];
    //            entities[i] = Interlocked.Exchange(ref entities[largest], temp);
    //            i = largest;
    //            left = 2 * i + 1;
    //            right = 2 * i + 2;

    //            if (left < n && entities[left].Number > entities[largest].Number)
    //            {
    //                largest = left;
    //            }

    //            if (right < n && entities[right].Number > entities[largest].Number)
    //            {
    //                largest = right;
    //            }

    //        } while (largest != i);
    //    }
    //}

    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    Task[] heapifyTasks = new Task[n / 2];

    //    for (int i = n / 2 - 1; i >= 0; i--)
    //    {
    //        int index = i; // Capture the current value of i for the lambda expression
    //        heapifyTasks[index] = Task.Run(() => ParallelHeapify(entities, n, index));
    //    }

    //    Task.WaitAll(heapifyTasks);

    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        Entity temp;
    //        temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        Heapify(entities, i, 0);
    //    }
    //}

    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity temp;
    //        do
    //        {
    //            temp = entities[i];
    //            entities[i] = Interlocked.Exchange(ref entities[largest], temp);
    //            i = largest;
    //            left = 2 * i + 1;
    //            right = 2 * i + 2;

    //            if (left < n && entities[left].Number > entities[largest].Number)
    //            {
    //                largest = left;
    //            }

    //            if (right < n && entities[right].Number > entities[largest].Number)
    //            {
    //                largest = right;
    //            }

    //        } while (largest != i);
    //    }
    //}

    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    Parallel.Invoke(() =>
    //    {
    //        for (int i = n / 2 - 1; i >= 0; i--)
    //        {
    //            ParallelHeapify(entities, n, i);
    //        }
    //    });

    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        Entity temp;
    //        temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        ParallelHeapify(entities, i, 0);
    //    }
    //}

    //static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    //using (var countdownEvent = new CountdownEvent(n / 2))
    //    //{
    //    //    for (int i = n / 2 - 1; i >= 0; i--)
    //    //    {
    //    //        ThreadPool.QueueUserWorkItem(state =>
    //    //        {
    //    //            int index = (int)state;
    //    //            ParallelHeapify(entities, n, index);
    //    //            countdownEvent.Signal();
    //    //        }, i);
    //    //    }

    //    //    countdownEvent.Wait();
    //    //}

    //    Parallel.Invoke(() =>
    //    {
    //        for (int i = n / 2 - 1; i >= 0; i--)
    //        {
    //            ParallelHeapify(entities, n, i);
    //        }
    //    });

    //    for (int i = n - 1; i > 0; i--)
    //    {
    //        Entity temp;
    //        temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        ParallelHeapify(entities, i, 0);
    //    }
    //}

    //static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity temp;
    //        do
    //        {
    //            temp = entities[i];
    //            entities[i] = Interlocked.Exchange(ref entities[largest], temp);
    //            i = largest;
    //            left = 2 * i + 1;
    //            right = 2 * i + 2;

    //            if (left < n && entities[left].Number > entities[largest].Number)
    //            {
    //                largest = left;
    //            }

    //            if (right < n && entities[right].Number > entities[largest].Number)
    //            {
    //                largest = right;
    //            }

    //        } while (largest != i);
    //    }
    //}



    //lock (entities)
    //{
    //    swap = entities[i];
    //    entities[i] = entities[largest];
    //    entities[largest] = swap;
    //}
    //Interlocked.Exchange(ref entities[i], entities[largest]);
    //ParallelHeapify(entities, n, largest);


    //public static void ParallelPyramidSort(Entity[] entities)
    //{
    //    int n = entities.Length;

    //    // Build max heap in parallel
    //    Parallel.For(n / 2 - 1, 0, i =>
    //    {
    //        ParallelHeapify(entities, n, i);
    //    });

    //    // Use a concurrent stack to store intermediate results
    //    ConcurrentStack<int> intermediateResults = new ConcurrentStack<int>();

    //    // Perform sorting in parallel
    //    Parallel.For(0, n, i =>
    //    {
    //        // Swap the root (maximum element) with the current element
    //        Entity temp = entities[0];
    //        entities[0] = entities[i];
    //        entities[i] = temp;

    //        // Push the swapped element into the intermediate results stack
    //        intermediateResults.Push(entities[i].Number);

    //        // Rebuild the heap in parallel with reduced size
    //        ParallelHeapify(entities, i, 0);
    //    });

    //    // Retrieve the intermediate results from the stack and update the sorted array
    //    for (int i = n - 1; i >= 0; i--)
    //    {
    //        intermediateResults.TryPop(out int number);
    //        entities[i].Number = number;
    //    }
    //}

    //private static void ParallelHeapify(Entity[] entities, int n, int i)
    //{
    //    int largest = i;
    //    int left = 2 * i + 1;
    //    int right = 2 * i + 2;

    //    if (left < n && entities[left].Number > entities[largest].Number)
    //    {
    //        largest = left;
    //    }

    //    if (right < n && entities[right].Number > entities[largest].Number)
    //    {
    //        largest = right;
    //    }

    //    if (largest != i)
    //    {
    //        Entity swap = entities[i];
    //        entities[i] = entities[largest];
    //        entities[largest] = swap;

    //        // Recursively heapify the affected subtree in parallel
    //        ParallelHeapify(entities, n, largest);
    //    }
    //}
}