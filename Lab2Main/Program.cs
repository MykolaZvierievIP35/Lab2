namespace Lab2Main;

public static class Program
{
    public static void Main()
    {
        int[] goalState = [1, 2, 3, 4, 5, 6, 7, 8, 0];
        var randomStates = PuzzleSolver.GenerateRandomInitialStates(20);

        foreach (var initialState in randomStates)
        {
            Console.WriteLine("\nПочатковий стан:");
            PuzzleSolver.PrintState(initialState.State);

            if (PuzzleSolver.IsSolvable(initialState.State))
            {
                Console.WriteLine("\nПочатковий стан розв'язний.");
                
                Console.WriteLine("\nBFS:");
                PuzzleSolver.Bfs(initialState.State, goalState, TimeSpan.FromMinutes(30), 1_073_741_824);
                
                Console.WriteLine("\nRBFS:");
                PuzzleSolver.Rbfs(initialState.State, goalState, TimeSpan.FromMinutes(30), 1_073_741_824);
            }
            else
            {
                Console.WriteLine("\nПочатковий стан не розв'язний.");
            }
        }
    }
}