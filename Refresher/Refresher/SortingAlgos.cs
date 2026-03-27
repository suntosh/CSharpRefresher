
 /// <summary>
 /// Sorting Algorithms Reference — C# Implementation
 /// ─────────────────────────────────────────────────
 /// Complexity Summary:
 ///
 /// Algorithm        Best        Average     Worst       Space   Stable
 /// ─────────────────────────────────────────────────────────────────────
 /// Bubble Sort      O(n)        O(n²)       O(n²)       O(1)    Yes
 /// Selection Sort   O(n²)       O(n²)       O(n²)       O(1)    No
 /// Insertion Sort   O(n)        O(n²)       O(n²)       O(1)    Yes
 /// Merge Sort       O(n log n)  O(n log n)  O(n log n)  O(n)    Yes
 /// Quick Sort       O(n log n)  O(n log n)  O(n²)       O(log n) No
 /// Heap Sort        O(n log n)  O(n log n)  O(n log n)  O(1)    No
 /// Counting Sort    O(n+k)      O(n+k)      O(n+k)      O(k)    Yes
 /// Radix Sort       O(nk)       O(nk)       O(nk)       O(n+k)  Yes
 ///
 /// n = number of elements
 /// k = range of input values (Counting Sort) / number of digits (Radix Sort)
 /// ─────────────────────────────────────────────────────────────────────
 /// </summary>

    using System;
    using System.Collections.Generic;

    namespace SortingAlgorithms
    {
        public static class Sorters
        {
            // ═══════════════════════════════════════════════════════════════
            // BUBBLE SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Repeatedly compare adjacent elements and swap if
            //              out of order. Each pass bubbles the largest unsorted
            //              element to its correct position.
            //
            // Best Case:   O(n)    — Array already sorted; early exit on zero swaps
            // Average:     O(n²)   — Random data, ~n²/2 comparisons
            // Worst Case:  O(n²)   — Reverse sorted, maximum swaps
            // Space:       O(1)    — In-place, single temp variable for swap
            // Stable:      Yes     — Equal elements never swap, relative order preserved
            //
            // When to use: Teaching purposes only. Never in production.
            //              One legitimate use: nearly-sorted tiny arrays where
            //              early exit makes it approach O(n).
            //
            public static void BubbleSort(int[] arr)
            {
                int n = arr.Length;

                for (int i = 0; i < n - 1; i++)
                {
                    bool swapped = false; // Early exit optimization — O(n) best case

                    for (int j = 0; j < n - i - 1; j++)
                    {
                        if (arr[j] > arr[j + 1])
                        {
                            // Tuple swap — no temp variable needed
                            (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                            swapped = true;
                        }
                    }

                    // If no swaps occurred in this pass, array is already sorted
                    if (!swapped) break;
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // SELECTION SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Divide array into sorted and unsorted regions.
            //              Each pass finds the minimum of the unsorted region
            //              and swaps it to the end of the sorted region.
            //
            // Best Case:   O(n²)   — No early exit; always scans full unsorted region
            // Average:     O(n²)   — Always n(n-1)/2 comparisons regardless of input
            // Worst Case:  O(n²)   — Same as best; no adaptive behavior
            // Space:       O(1)    — In-place, only stores index of minimum
            // Stable:      No      — Swap can move equal elements past each other
            //
            // Key property: Makes at most O(n) swaps — best choice when write
            //               operations are extremely expensive (e.g., flash memory).
            //
            // When to use: When minimizing swaps matters more than comparisons.
            //              Otherwise, insertion sort is strictly better.
            //
            public static void SelectionSort(int[] arr)
            {
                int n = arr.Length;

                for (int i = 0; i < n - 1; i++)
                {
                    int minIndex = i; // Assume current position is minimum

                    // Scan unsorted region for true minimum
                    for (int j = i + 1; j < n; j++)
                    {
                        if (arr[j] < arr[minIndex])
                            minIndex = j;
                    }

                    // Only swap if minimum is not already in position
                    if (minIndex != i)
                        (arr[i], arr[minIndex]) = (arr[minIndex], arr[i]);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // INSERTION SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Build sorted array one element at a time by inserting
            //              each new element into its correct position in the
            //              already-sorted prefix.
            //
            // Best Case:   O(n)    — Already sorted; inner while never executes
            // Average:     O(n²)   — Random data, ~n²/4 comparisons and shifts
            // Worst Case:  O(n²)   — Reverse sorted; maximum shifts per insertion
            // Space:       O(1)    — In-place, stores current key only
            // Stable:      Yes     — Only shifts past strictly greater elements
            //
            // Key property: Online algorithm — can sort as elements arrive.
            //               Adaptive — faster on nearly sorted data.
            //               Preferred over bubble/selection for small n.
            //
            // When to use: Small arrays (n < 20), nearly sorted data, or as
            //              the base case in hybrid sorts (Timsort uses n < 64).
            //              Also used to finish Shellsort's final pass.
            //
            public static void InsertionSort(int[] arr)
            {
                int n = arr.Length;

                for (int i = 1; i < n; i++)
                {
                    int key = arr[i]; // Element to be inserted into sorted prefix
                    int j = i - 1;

                    // Shift elements greater than key one position to the right
                    while (j >= 0 && arr[j] > key)
                    {
                        arr[j + 1] = arr[j];
                        j--;
                    }

                    arr[j + 1] = key; // Insert key into its correct position
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // MERGE SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Divide and conquer. Recursively split array into halves,
            //              sort each half, then merge the two sorted halves.
            //
            // Best Case:   O(n log n)  — Always divides evenly, always merges
            // Average:     O(n log n)  — Consistent regardless of input distribution
            // Worst Case:  O(n log n)  — Guaranteed; no bad pivot problem
            // Space:       O(n)        — Requires auxiliary array for merging
            // Stable:      Yes         — Merge preserves relative order of equals
            //
            // Key properties:
            //   - Only comparison sort with guaranteed O(n log n) worst case
            //   - Preferred for linked lists (no random access needed for merge)
            //   - Foundation of Timsort (.NET's Array.Sort uses a variant)
            //   - External sort — can sort data larger than RAM
            //
            // When to use: When stability required + guaranteed performance needed.
            //              Linked list sorting. External/disk-based sorting.
            //              When worst-case matters more than average performance.
            //
            public static void MergeSort(int[] arr, int left, int right)
            {
                if (left >= right) return; // Base case: single element is sorted

                int mid = left + (right - left) / 2; // Avoids integer overflow vs (left+right)/2

                MergeSort(arr, left, mid);        // Sort left half
                MergeSort(arr, mid + 1, right);   // Sort right half
                Merge(arr, left, mid, right);     // Merge sorted halves
            }

            /// <summary>
            /// Merges two adjacent sorted subarrays arr[left..mid] and arr[mid+1..right]
            /// Time: O(n) where n = right - left + 1
            /// Space: O(n) for the temporary array
            /// </summary>
            private static void Merge(int[] arr, int left, int mid, int right)
            {
                // Copy subarray to temp — O(n) space allocation
                int[] temp = arr[left..(right + 1)];

                int i = 0;                    // Index into left half of temp
                int j = mid - left + 1;       // Index into right half of temp
                int k = left;                 // Index into original array

                // Merge by comparing smallest remaining elements from each half
                while (i <= mid - left && j <= right - left)
                {
                    // Use <= for stability: prefer left element on ties
                    if (temp[i] <= temp[j])
                        arr[k++] = temp[i++];
                    else
                        arr[k++] = temp[j++];
                }

                // Copy remaining elements from left half (right half already in place)
                while (i <= mid - left) arr[k++] = temp[i++];
                while (j <= right - left) arr[k++] = temp[j++];
            }

            // ═══════════════════════════════════════════════════════════════
            // QUICKSORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Divide and conquer. Select a pivot, partition array
            //              so elements < pivot are left and > pivot are right,
            //              then recursively sort both partitions.
            //
            // Best Case:   O(n log n)  — Pivot always splits array in half
            // Average:     O(n log n)  — Expected with random or shuffled data
            // Worst Case:  O(n²)       — Pivot always min/max (sorted input, naive pivot)
            // Space:       O(log n)    — Recursive call stack depth (average)
            //              O(n)        — Worst case call stack on degenerate input
            // Stable:      No          — Partition swaps can reorder equal elements
            //
            // Key properties:
            //   - Fastest in practice despite same O notation as Merge Sort
            //   - Cache friendly — sequential memory access pattern
            //   - In-place — no auxiliary array needed
            //   - Worst case avoided with: random pivot, median-of-three, or
            //     switching to Heapsort when recursion depth exceeds 2*log(n)
            //     (Introsort — what .NET actually uses under Array.Sort)
            //
            // When to use: General purpose default sort when stability not needed.
            //              Consistently fastest in practice for random data.
            //
            public static void QuickSort(int[] arr, int low, int high)
            {
                if (low >= high) return; // Base case: zero or one element

                int pivotIndex = Partition(arr, low, high);

                QuickSort(arr, low, pivotIndex - 1);  // Sort left of pivot
                QuickSort(arr, pivotIndex + 1, high); // Sort right of pivot
            }

            /// <summary>
            /// Lomuto partition scheme.
            /// Places pivot (last element) in its correct sorted position.
            /// Elements left of pivot are <= pivot, right are > pivot.
            /// Time: O(n), Space: O(1)
            ///
            /// Note: Hoare partition (two-pointer) is faster in practice
            /// but harder to implement correctly. Lomuto shown here for clarity.
            /// </summary>
            private static int Partition(int[] arr, int low, int high)
            {
                int pivot = arr[high]; // Naive pivot — last element
                                       // Production: use median-of-three to avoid O(n²) on sorted input

                int i = low - 1; // i tracks boundary of elements <= pivot

                for (int j = low; j < high; j++)
                {
                    if (arr[j] <= pivot)
                    {
                        i++;
                        (arr[i], arr[j]) = (arr[j], arr[i]);
                    }
                }

                // Place pivot in its final sorted position
                (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
                return i + 1;
            }

            // ═══════════════════════════════════════════════════════════════
            // HEAP SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Build a max-heap from the array. Repeatedly extract
            //              the maximum (root) and place it at the end, reducing
            //              heap size by one each iteration.
            //
            // Best Case:   O(n log n)  — No adaptive behavior, always heapifies
            // Average:     O(n log n)  — Consistent regardless of input
            // Worst Case:  O(n log n)  — Guaranteed; unlike Quicksort, no bad input
            // Space:       O(1)        — In-place heap in the original array
            // Stable:      No          — Heapify swaps disrupt relative order
            //
            // Key properties:
            //   - Guaranteed O(n log n) with O(1) space — best of Merge+Quick
            //   - Slower in practice than Quicksort due to poor cache behavior
            //     (heap access pattern jumps around memory non-sequentially)
            //   - Used in Introsort as fallback when Quicksort depth exceeds limit
            //
            // When to use: When O(1) space AND guaranteed O(n log n) required.
            //              Rarely standalone in practice — usually part of Introsort.
            //
            public static void HeapSort(int[] arr)
            {
                int n = arr.Length;

                // Phase 1: Build max-heap — O(n)
                // Start from last non-leaf node and heapify down to root
                for (int i = n / 2 - 1; i >= 0; i--)
                    Heapify(arr, n, i);

                // Phase 2: Extract elements from heap one by one — O(n log n)
                for (int i = n - 1; i > 0; i--)
                {
                    // Move current root (maximum) to end
                    (arr[0], arr[i]) = (arr[i], arr[0]);

                    // Restore heap property on reduced heap
                    Heapify(arr, i, 0);
                }
            }

            /// <summary>
            /// Maintains max-heap property for subtree rooted at index i.
            /// Assumes subtrees below i are already valid max-heaps.
            /// Time: O(log n), Space: O(1) iterative implementation
            /// </summary>
            private static void Heapify(int[] arr, int n, int i)
            {
                while (true)
                {
                    int largest = i;
                    int left = 2 * i + 1; // Left child index
                    int right = 2 * i + 2; // Right child index

                    // Find largest among root, left child, right child
                    if (left < n && arr[left] > arr[largest])
                        largest = left;

                    if (right < n && arr[right] > arr[largest])
                        largest = right;

                    // If root is already largest, heap property satisfied
                    if (largest == i) break;

                    // Swap and continue down the tree
                    (arr[i], arr[largest]) = (arr[largest], arr[i]);
                    i = largest;
                }
            }

            // ═══════════════════════════════════════════════════════════════
            // COUNTING SORT
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Non-comparison sort. Count occurrences of each value,
            //              compute cumulative counts (positions), then place each
            //              element directly into its correct output position.
            //
            // Best Case:   O(n + k)    — k = range of values (max - min + 1)
            // Average:     O(n + k)    — Same regardless of distribution
            // Worst Case:  O(n + k)    — Same regardless of distribution
            // Space:       O(k)        — Count array of size k; O(n) for output
            // Stable:      Yes         — Backward iteration preserves relative order
            //
            // Key properties:
            //   - Breaks O(n log n) comparison sort lower bound
            //   - Efficient when k = O(n) — range not much larger than count
            //   - Impractical when k >> n (e.g., sorting 100 numbers in range 0..1M)
            //   - Foundation of Radix Sort
            //
            // When to use: Integer keys with small known range.
            //              Classic: sorting exam scores (0-100), character frequencies,
            //              age distributions, anything with bounded integer domain.
            //
            public static int[] CountingSort(int[] arr)
            {
                if (arr.Length == 0) return arr;

                int min = arr[0], max = arr[0];
                foreach (int x in arr)
                {
                    if (x < min) min = x;
                    if (x > max) max = x;
                }

                int k = max - min + 1;          // Range of values
                int[] count = new int[k];       // O(k) space

                // Phase 1: Count occurrences — O(n)
                foreach (int x in arr)
                    count[x - min]++;

                // Phase 2: Cumulative counts = final positions — O(k)
                for (int i = 1; i < k; i++)
                    count[i] += count[i - 1];

                // Phase 3: Build output array — O(n)
                // Iterate backward for stability
                int[] output = new int[arr.Length];
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    output[--count[arr[i] - min]] = arr[i];
                }

                return output;
            }

            // ═══════════════════════════════════════════════════════════════
            // RADIX SORT (LSD — Least Significant Digit)
            // ═══════════════════════════════════════════════════════════════
            //
            // Strategy:    Non-comparison sort. Sort by each digit position from
            //              least significant to most significant using stable
            //              counting sort as subroutine.
            //
            // Best Case:   O(nk)       — k = number of digits in maximum value
            // Average:     O(nk)       — log10(max) passes of O(n) counting sort
            // Worst Case:  O(nk)       — Same; no adaptive behavior
            // Space:       O(n + b)    — b = base (10 for decimal), n for output
            // Stable:      Yes         — Each pass uses stable counting sort
            //
            // Key properties:
            //   - Beats O(n log n) when k = O(log n) — e.g., 32-bit integers, k=10
            //   - Works on strings, fixed-width keys, dates — anything with positional digits
            //   - MSD (Most Significant Digit) variant enables partial sort / range queries
            //   - Used in suffix array construction, DNA sequence sorting
            //
            // When to use: Large arrays of integers or fixed-length strings.
            //              When key length is bounded and small relative to n.
            //              Network packet sorting, database join algorithms.
            //
            public static void RadixSort(int[] arr)
            {
                if (arr.Length == 0) return;

                int max = arr[0];
                foreach (int x in arr)
                    if (x > max) max = x;

                // Process each digit position using stable counting sort — O(nk)
                // exp = 1 (ones), 10 (tens), 100 (hundreds), ...
                for (int exp = 1; max / exp > 0; exp *= 10)
                    CountingSortByDigit(arr, exp);
            }

            /// <summary>
            /// Stable counting sort on the digit at position exp (1, 10, 100...).
            /// Only handles non-negative integers.
            /// Time: O(n + 10) = O(n), Space: O(n + 10) = O(n)
            /// </summary>
            private static void CountingSortByDigit(int[] arr, int exp)
            {
                int n = arr.Length;
                int[] output = new int[n];
                int[] count = new int[10]; // Digits 0-9

                // Count occurrences of each digit at current position
                foreach (int x in arr)
                    count[(x / exp) % 10]++;

                // Cumulative count — gives final positions
                for (int i = 1; i < 10; i++)
                    count[i] += count[i - 1];

                // Build output — iterate backward for stability
                for (int i = n - 1; i >= 0; i--)
                {
                    int digit = (arr[i] / exp) % 10;
                    output[--count[digit]] = arr[i];
                }

                // Copy back to original array
                Array.Copy(output, arr, n);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // DEMO / TEST HARNESS
        // ═══════════════════════════════════════════════════════════════════
        public class SorterShell
        {
            public static void Exec()
            {
                Console.WriteLine("Sorting Algorithms — Complexity Reference");
                Console.WriteLine("==========================================\n");

                // Test arrays
                int[] original = { 64, 34, 25, 12, 22, 11, 90, 1, 55, 42 };
                int[] sorted = { 1, 2, 3, 4, 5 };      // Best case for adaptive sorts
                int[] reversed = { 9, 8, 7, 6, 5, 4, 3, 2, 1 }; // Worst case for naive sorts

                RunSort("Bubble Sort", (int[] a) => Sorters.BubbleSort(a), (int[])original.Clone());
                RunSort("Selection Sort", (int[] a) => Sorters.SelectionSort(a), (int[])original.Clone());
                RunSort("Insertion Sort", (int[] a) => Sorters.InsertionSort(a), (int[])original.Clone());
                RunSort("Merge Sort", (int[] a) => Sorters.MergeSort(a, 0, a.Length - 1), (int[])original.Clone());
                RunSort("Quick Sort", (int[] a) => Sorters.QuickSort(a, 0, a.Length - 1), (int[])original.Clone());
                RunSort("Heap Sort", (int[] a) => Sorters.HeapSort(a), (int[])original.Clone());

                // Non-comparison sorts
                int[] forCounting = (int[])original.Clone();
                int[] counted = Sorters.CountingSort(forCounting);
                Console.WriteLine($"Counting Sort:  [{string.Join(", ", counted)}]");

                int[] forRadix = (int[])original.Clone();
                Sorters.RadixSort(forRadix);
                Console.WriteLine($"Radix Sort:     [{string.Join(", ", forRadix)}]");

                Console.WriteLine("\n── Complexity Summary ──────────────────────────────────────────");
                Console.WriteLine($"{"Algorithm",-16} {"Best",-12} {"Average",-12} {"Worst",-12} {"Space",-8} {"Stable"}");
                Console.WriteLine(new string('─', 72));
                Console.WriteLine($"{"Bubble Sort",-16} {"O(n)",-12} {"O(n²)",-12} {"O(n²)",-12} {"O(1)",-8} {"Yes"}");
                Console.WriteLine($"{"Selection Sort",-16} {"O(n²)",-12} {"O(n²)",-12} {"O(n²)",-12} {"O(1)",-8} {"No"}");
                Console.WriteLine($"{"Insertion Sort",-16} {"O(n)",-12} {"O(n²)",-12} {"O(n²)",-12} {"O(1)",-8} {"Yes"}");
                Console.WriteLine($"{"Merge Sort",-16} {"O(n log n)",-12} {"O(n log n)",-12} {"O(n log n)",-12} {"O(n)",-8} {"Yes"}");
                Console.WriteLine($"{"Quick Sort",-16} {"O(n log n)",-12} {"O(n log n)",-12} {"O(n²)",-12} {"O(log n)",-8} {"No"}");
                Console.WriteLine($"{"Heap Sort",-16} {"O(n log n)",-12} {"O(n log n)",-12} {"O(n log n)",-12} {"O(1)",-8} {"No"}");
                Console.WriteLine($"{"Counting Sort",-16} {"O(n+k)",-12} {"O(n+k)",-12} {"O(n+k)",-12} {"O(k)",-8} {"Yes"}");
                Console.WriteLine($"{"Radix Sort",-16} {"O(nk)",-12} {"O(nk)",-12} {"O(nk)",-12} {"O(n+k)",-8} {"Yes"}");
            }

            static void RunSort(string name, Action<int[]> sortFn, int[] arr)
            {
                sortFn(arr);
                Console.WriteLine($"{name,-16}: [{string.Join(", ", arr)}]");
            }
        }
    }

