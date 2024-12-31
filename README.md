Large Merge Sort Algorithm

The Large Merge Sort Algorithm is designed to efficiently sort massive datasets that cannot fit into memory. It combines the divide-and-conquer strategy with external sorting techniques to handle large files by splitting, 
sorting, and merging data incrementally. Below is a high-level description of the algorithm.

Algorithm Workflow

1. Chunk Creation
Input: A large file containing unsorted data.
Process:
Divide the file into smaller chunks based on memory constraints.
Each chunk is read into memory, sorted, and written back to disk as a separate temporary file.
These sorted chunks serve as the building blocks for the merge process.
Output: A set of sorted temporary files (e.g., chunk_1.cnk, chunk_2.cnk, etc.).
Strategies:
 1.1. SingleThreadReaderStrategy - read file in a single thread and create chunks consequentially.
 1.2. ParallelFileReaderStrategy - calculate available system resuorces and read same file in parallel.

2. Merge Strategy
Input: Sorted chunks from the previous step.
Process:
Use merge Sort algorithm to combine sorted chunks into a single file.

Perform merging in parallel by dispatching tasks to background threads.
Use a min-heap (or priority queue) to efficiently merge sorted data from multiple files.
Strategies:
 2.1. MultiChunkMergeStrategy - single thread merge algorithm. Open multiple readers (per each chunk), write the smallest line into target file.
 2.2. PairwiseMergeStrategy - single thread merge algorithm. Merge 2 chunks into a single file. Repeat until single file is left.
 2.3. ParallelPairwiseStrategy - multithreaded version of PairwiseMergeStrategy. Continuously dequeue two sorted files, merge them into a single sorted file, and enqueue the result. Repeat until only one file remains in the queue.


3. Final Output
The last remaining file is the fully sorted result.

Initial requirements:

The input is a large text file, where each line is a Number. String
For example:
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
Both parts can be repeated within the file. You need to get another file as output, where all
the lines are sorted. Sorting criteria: String part is compared first, if it matches then
Number.
Those in the example above, it should be:
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
You need to write two programs:
1. A utility for creating a test file of a given size. The result of the work should be a text file
of the type described above. There must be some number of lines with the same String
part.
2. The actual sorter. An important point, the file can be very large. The size of ~100Gb will
be used for testing.
When evaluating the completed task, we will first look at the result (correctness of
generation / sorting and running time), and secondly, at how the candidate writes the code.
Programming language: C#.
