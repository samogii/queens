using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;

class Program
{
    static void Main()
    {
        Console.Write("Enter your Queens Number : ");
        int x = int.Parse(Console.ReadLine());
        int boardSize = x;
        int populationSize = 1000;
        int generations = 1000;
        int count = 0;
        double crossoverProbability = 0.6;
        double mutationProbability = 0.1;
        for (int i = 0; i < 10; i++)
        {
            crossoverProbability = 0.6;
            mutationProbability = 0.1;
            count = 0;
            GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(boardSize, populationSize, crossoverProbability, mutationProbability);
            Board solution = geneticAlgorithm.Run(generations, ref count);

            Console.WriteLine("Solution found:");
            Console.WriteLine(solution.ToString() + $"\nWith Errors : {count}");
        }
        Console.ReadLine();
    }
}

class Board
{
    private int[] queens;

    public Board(int size)
    {
        queens = new int[size];
        InitializeRandomBoard();
    }

    public Board(int[] queens)
    {
        this.queens = queens.ToArray();
    }

    public int Size => queens.Length;

    public int[] Queens => queens;

    public void InitializeRandomBoard()
    {
        Random random = new Random();
        for (int i = 0; i < Size; i++)
        {
            queens[i] = random.Next(Size);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                sb.Append(queens[i] == j ? "Q" : "#");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

class GeneticAlgorithm
{
    private int boardSize;
    private int populationSize;
    private double crossoverProbability;
    private double mutationProbability;

    public GeneticAlgorithm(int boardSize, int populationSize, double crossoverProbability, double mutationProbability)
    {
        this.boardSize = boardSize;
        this.populationSize = populationSize;
        this.mutationProbability = mutationProbability;
        this.crossoverProbability = crossoverProbability;
    }
    
    private void AdjustCrossoverProbability(int generation, int maxGenerations)
    {
        // Example: Linearly decrease crossoverProbability over generations
        double minCrossoverProbability = 0.1;
        double slope = (minCrossoverProbability - crossoverProbability) / maxGenerations;
        crossoverProbability += slope;

        // Ensure crossoverProbability doesn't go below the minimum value
        crossoverProbability = Math.Max(crossoverProbability, minCrossoverProbability);
    }

    public Board Run(int generations , ref int count )
    {
        List<Board> population = InitializePopulation();

        for (int generation = 0; generation < generations; generation++)
        {
            population = EvolvePopulation(population);
            Board bestIndividual = population.OrderBy(b => CalculateConflicts(b)).First();
            AdjustCrossoverProbability(generation, generations);
            AdjustMutationProbability(generation, generations);
            if (CalculateConflicts(bestIndividual) == 0)
            {
                return bestIndividual;
            }
            count++;
            //Console.WriteLine($"Try Number : {generation + 1}, Conflict: \n{bestIndividual}\nNumber of Conflicts {CalculateConflicts(bestIndividual)}\n");
        }

        return new Board(0); // No solution found
    }

    private void AdjustMutationProbability(int generation, int maxGenerations)
    {
        // Example: Linearly decrease mutationProbability over generations
        double minMutationProbability = 0.01;
        double slope = (minMutationProbability - mutationProbability) / maxGenerations;
        mutationProbability += slope;

        // Ensure mutationProbability doesn't go below the minimum value
        mutationProbability = Math.Max(mutationProbability, minMutationProbability);
    }

    private List<Board> InitializePopulation()
    {
        return Enumerable.Range(0, populationSize).Select(_ => new Board(boardSize)).ToList();
    }


    private List<Board> EvolvePopulation(List<Board> population)
    {
        List<Board> newPopulation = new List<Board>();

        while (newPopulation.Count < populationSize)
        {
            Board parent1 = SelectParent(population);
            Board parent2 = SelectParent(population);

            Board child = Crossover(parent1, parent2);
            child = Mutate(child);

            newPopulation.Add(child);
        }

        return newPopulation;
    }

    private Board SelectParent(List<Board> population)
    {
        Random random = new Random();
        int tournamentSize = 500;

        List<Board> tournament = new List<Board>();
        for (int i = 0; i < tournamentSize; i++)
        {
            int index = random.Next(population.Count);
            tournament.Add(population[index]);
        }

        return tournament.OrderBy(b => CalculateConflicts(b)).First();
    }

    private Board Crossover(Board parent1, Board parent2)
    {
        if (random.NextDouble() < crossoverProbability)
        {
            // Order crossover (OX)
            int[] childQueens = new int[boardSize];
            int startPos = random.Next(boardSize);
            int endPos = random.Next(startPos, boardSize);

            for (int i = startPos; i <= endPos; i++)
            {
                childQueens[i] = parent1.Queens[i];
            }

            int index = 0;
            for (int i = 0; i < boardSize; i++)
            {
                if (!childQueens.Contains(parent2.Queens[i]))
                {
                    try
                    {
                        while (childQueens[index] != 0)
                        {
                            index++;
                        }

                        childQueens[index++] = parent2.Queens[i];
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                    break;
                }
            }

            return new Board(childQueens);
        }
        else
        {
            return parent1;
        }
    }

    private Board Mutate(Board board)
    {
        if (random.NextDouble() < mutationProbability)
        {
            // Swap mutation
            int pos1 = random.Next(boardSize);
            int pos2 = random.Next(boardSize);

            int temp = board.Queens[pos1];
            board.Queens[pos1] = board.Queens[pos2];
            board.Queens[pos2] = temp;
        }
        return board;
    }

    private int CalculateConflicts(Board board)
    {
        int conflicts = 0;

        for (int i = 0; i < board.Size; i++)
        {
            for (int j = i + 1; j < board.Size; j++)
            {
                if (HasConflict(board, i, j))
                {
                    conflicts++;
                    break;
                }
            }
        }
        return conflicts;
    }

    private bool HasConflict(Board board, int i, int j)
    {
        return board.Queens[i] == board.Queens[j] ||
               Math.Abs(i - j) == Math.Abs(board.Queens[i] - board.Queens[j]);
    }


    private Random random = new Random();
}
