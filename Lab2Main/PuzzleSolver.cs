namespace Lab2Main;

public abstract class PuzzleSolver
{
    private static readonly int[] RowMoves = [0, 0, -1, 1];
    private static readonly int[] ColMoves = [-1, 1, 0, 0];
    private static readonly Random Random = new();

    public class Node
    {
        public readonly int[] State;
        public readonly int ZeroIndex;
        public readonly List<int[]> Path;
        public int Cost;

        public Node(int[] state, int zeroIndex, List<int[]> path, int cost = 0)
        {
            State = state;
            ZeroIndex = zeroIndex;
            Path = path;
            Cost = cost;
        }
    }
    
    private class SearchStatistics
    {
        public int IterationsCount;
        public int DeadEndsCount;
        public int TotalNodesCount;
        public int MaxNodesInMemory;
    }
    
    public static bool IsSolvable(int[] board)
    {
        var flatBoard = board.Where(n => n != 0).ToArray();
        int inversions = 0;

        for (int i = 0; i < flatBoard.Length - 1; i++)
        {
            for (int j = i + 1; j < flatBoard.Length; j++)
            {
                if (flatBoard[i] > flatBoard[j])
                {
                    inversions++;
                }
            }
        }
        
        return inversions % 2 == 0;
    }
    
    public static List<Node> GenerateRandomInitialStates(int count)
    {
        List<Node> randomStates = new();

        while (randomStates.Count < count)
        {
            int[] state = GenerateRandomState();
            int zeroIndex = Array.IndexOf(state, 0);
            randomStates.Add(new Node(state, zeroIndex, [state]));
        }

        return randomStates;
    }

    private static int[] GenerateRandomState()
    {
        int[] state = [0, 1, 2, 3, 4, 5, 6, 7, 8];
        for (int i = state.Length - 1; i > 0; i--)
        {
            int j = Random.Next(i + 1);
            (state[i], state[j]) = (state[j], state[i]);
        }
        return state;
    }
    
    public static void Bfs(int[] initialState, int[] goalState, TimeSpan timeLimit, long memoryLimit)
    {
        var startTime = DateTime.Now;
        
        int iterationsCount = 0;
        int deadEndsCount = 0;
        int totalNodesCount = 1;
        int maxNodesInMemory = 1;

        Queue<Node> queue = new();
        HashSet<string> visited = new();

        int zeroIndex = Array.IndexOf(initialState, 0);
        queue.Enqueue(new Node(initialState, zeroIndex, [initialState]));
        visited.Add(StateToString(initialState));

        while (queue.Count > 0)
        {
            iterationsCount++;
            
            if (DateTime.Now - startTime > timeLimit)
            {
                Console.WriteLine("Часове обмеження перевищено для BFS.");
                break;
            }
            
            if (GetCurrentMemoryUsage() > memoryLimit)
            {
                Console.WriteLine("Обмеження по пам'яті перевищено для BFS.");
                break;
            }

            Node currentNode = queue.Dequeue();
            int[] currentState = currentNode.State;

            if (IsGoalState(currentState, goalState))
            {
                Console.WriteLine("Розв'язок знайдено!");
                PrintPath(currentNode.Path);
                
                int solutionSteps = currentNode.Path.Count - 1;
                Console.WriteLine($"Кількість кроків для розв'язання: {solutionSteps}");
                Console.WriteLine($"Кількість ітерацій: {iterationsCount}");
                Console.WriteLine($"Кількість глухих кутів: {deadEndsCount}");
                Console.WriteLine($"Загальна кількість вузлів: {totalNodesCount}");
                Console.WriteLine($"Максимальна кількість вузлів у пам'яті: {maxNodesInMemory}");

                return;
            }

            int currentRow = currentNode.ZeroIndex / 3;
            int currentCol = currentNode.ZeroIndex % 3;

            bool hasSuccessor = false;

            for (int i = 0; i < 4; i++)
            {
                int newRow = currentRow + RowMoves[i];
                int newCol = currentCol + ColMoves[i];

                if (IsValidMove(newRow, newCol))
                {
                    hasSuccessor = true;

                    int[] newState = (int[])currentState.Clone();
                    int newZeroIndex = newRow * 3 + newCol;
                    newState[currentNode.ZeroIndex] = newState[newZeroIndex];
                    newState[newZeroIndex] = 0;

                    if (visited.Add(StateToString(newState)))
                    {
                        totalNodesCount++;
                        List<int[]> newPath = [..currentNode.Path, newState];
                        queue.Enqueue(new Node(newState, newZeroIndex, newPath));
                    }
                }
            }

            if (!hasSuccessor)
            {
                deadEndsCount++;
            }

            int nodesInMemory = queue.Count + visited.Count;
            if (nodesInMemory > maxNodesInMemory)
            {
                maxNodesInMemory = nodesInMemory;
            }
        }

        Console.WriteLine("Розв'язок не знайдено.");
        Console.WriteLine($"Кількість ітерацій: {iterationsCount}");
        Console.WriteLine($"Кількість глухих кутів: {deadEndsCount}");
        Console.WriteLine($"Загальна кількість вузлів: {totalNodesCount}");
        Console.WriteLine($"Максимальна кількість вузлів у пам'яті: {maxNodesInMemory}");
    }
    
    public static void Rbfs(int[] initialState, int[] goalState, TimeSpan timeLimit, long memoryLimit)
    {
        var startTime = DateTime.Now;
        int zeroIndex = Array.IndexOf(initialState, 0);
        Node initialNode = new(initialState, zeroIndex, [initialState]);
        
        SearchStatistics stats = new()
        {
            TotalNodesCount = 1,
            MaxNodesInMemory = 1
        };

        var (found, _) = RbfsHelper(initialNode, goalState, int.MaxValue, startTime, timeLimit, memoryLimit, stats);

        if (!found)
        {
            Console.WriteLine("Розв'язок не знайдено.");
        }
        
        Console.WriteLine($"Кількість ітерацій: {stats.IterationsCount}");
        Console.WriteLine($"Кількість глухих кутів: {stats.DeadEndsCount}");
        Console.WriteLine($"Загальна кількість вузлів: {stats.TotalNodesCount}");
        Console.WriteLine($"Максимальна кількість вузлів у пам'яті: {stats.MaxNodesInMemory}");
    }

    private static (bool, int) RbfsHelper(Node currentNode, int[] goalState, int fLimit, DateTime startTime, TimeSpan timeLimit, long memoryLimit, SearchStatistics stats)
    {
        if (DateTime.Now - startTime > timeLimit)
        {
            Console.WriteLine("Часове обмеження перевищено для RBFS.");
            return (false, int.MaxValue);
        }
        
        if (GetCurrentMemoryUsage() > memoryLimit)
        {
            Console.WriteLine("Обмеження по пам'яті перевищено для RBFS.");
            return (false, int.MaxValue);
        }

        stats.IterationsCount++;

        if (IsGoalState(currentNode.State, goalState))
        {
            Console.WriteLine("Розв'язок знайдено!");
            PrintPath(currentNode.Path);
            int solutionSteps = currentNode.Path.Count - 1;
            Console.WriteLine($"Кількість кроків для розв'язання: {solutionSteps}");
            return (true, 0);
        }

        List<Node> successors = GenerateSuccessors(currentNode);
        if (successors.Count == 0)
        {
            stats.DeadEndsCount++;
            return (false, int.MaxValue);
        }

        foreach (var node in successors)
        {
            stats.TotalNodesCount++;
            node.Cost = Math.Max(currentNode.Cost, ManhattanDistance(node.State, goalState) + node.Path.Count);
        }

        int nodesInMemory = successors.Count + currentNode.Path.Count;
        if (nodesInMemory > stats.MaxNodesInMemory)
        {
            stats.MaxNodesInMemory = nodesInMemory;
        }

        while (successors.Count > 0)
        {
            if (DateTime.Now - startTime > timeLimit)
            {
                Console.WriteLine("Часове обмеження перевищено для RBFS.");
                return (false, int.MaxValue);
            }
            
            if (GetCurrentMemoryUsage() > memoryLimit)
            {
                Console.WriteLine("Обмеження по пам'яті перевищено для RBFS.");
                return (false, int.MaxValue);
            }

            successors.Sort((a, b) => a.Cost.CompareTo(b.Cost));
            Node best = successors[0];

            if (best.Cost > fLimit)
                return (false, best.Cost);

            int alternative = successors.Count > 1 ? successors[1].Cost : int.MaxValue;
            var (found, bestCost) = RbfsHelper(best, goalState, Math.Min(fLimit, alternative), startTime, timeLimit, memoryLimit, stats);

            if (found)
                return (true, bestCost);

            best.Cost = bestCost;
            successors[0] = best;
        }

        return (false, int.MaxValue);
    }

    private static List<Node> GenerateSuccessors(Node currentNode)
    {
        List<Node> successors = new();
        int currentRow = currentNode.ZeroIndex / 3;
        int currentCol = currentNode.ZeroIndex % 3;

        for (int i = 0; i < 4; i++)
        {
            int newRow = currentRow + RowMoves[i];
            int newCol = currentCol + ColMoves[i];

            if (IsValidMove(newRow, newCol))
            {
                int[] newState = (int[])currentNode.State.Clone();
                int newZeroIndex = newRow * 3 + newCol;
                newState[currentNode.ZeroIndex] = newState[newZeroIndex];
                newState[newZeroIndex] = 0;
                
                if (!currentNode.Path.Any(p => p.SequenceEqual(newState)))
                {
                    List<int[]> newPath = [..currentNode.Path, newState];
                    successors.Add(new Node(newState, newZeroIndex, newPath));
                }
            }
        }

        return successors;
    }
    
    private static int ManhattanDistance(int[] state, int[] goalState)
    {
        int distance = 0;
        for (int i = 0; i < 9; i++)
        {
            if (state[i] != 0)
            {
                int targetIndex = Array.IndexOf(goalState, state[i]);
                int currentRow = i / 3, currentCol = i % 3;
                int targetRow = targetIndex / 3, targetCol = targetIndex % 3;
                distance += Math.Abs(currentRow - targetRow) + Math.Abs(currentCol - targetCol);
            }
        }
        return distance;
    }

    private static bool IsGoalState(int[] state, int[] goalState)
    {
        return state.SequenceEqual(goalState);
    }

    private static bool IsValidMove(int row, int col)
    {
        return row is >= 0 and < 3 && col is >= 0 and < 3;
    }

    private static string StateToString(int[] state)
    {
        return string.Join(",", state);
    }

    private static void PrintPath(List<int[]> path)
    {
        foreach (var state in path)
        {
            PrintState(state);
            Console.WriteLine();
        }
    }

    public static void PrintState(int[] state)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i % 3 == 0) Console.WriteLine();
            Console.Write(state[i] + " ");
        }
    }
    
    private static long GetCurrentMemoryUsage()
    {
        return GC.GetTotalMemory(false);
    }
}
