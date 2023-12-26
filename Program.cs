using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        Console.Write("Enter your Queens Number: ");
        int x = int.Parse(Console.ReadLine());
        int boardSize = x;
        int populationSize = 1000;
        int generations = 10000;
        double crossoverProbability = 0.7;
        double mutationProbability = 0.1;

        try
        {
            GeneticAlgorithm geneticAlgorithm = new GeneticAlgorithm(boardSize, populationSize, crossoverProbability, mutationProbability);
            Board solution = geneticAlgorithm.Run(generations);

            Console.WriteLine("Solution found:");
            Console.WriteLine(solution.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
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
        this.crossoverProbability = crossoverProbability;
        this.mutationProbability = mutationProbability;
    }

    public Board Run(int generations)
    {
        List<Board> population = InitializePopulation();

        for (int generation = 0; generation < generations; generation++)
        {
            population = EvolvePopulation(population);
            Board bestIndividual = population.OrderBy(b => Fitness(b)).First();

            if (Fitness(bestIndividual) == 0)
            {
                return bestIndividual; // Solution found
            }

            Console.WriteLine($"Generation {generation + 1}, Best Fitness: {Fitness(bestIndividual)} ,\n{bestIndividual}");
        }

        return new Board(0); // No solution found
    }

    private List<Board> InitializePopulation()
    {
        List<Board> population = new List<Board>();

        for (int i = 0; i < populationSize; i++)
        {
            Board board = new Board(boardSize);
            population.Add(board);
        }

        return population;
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
        // Tournament selection
        Random random = new Random();
        int tournamentSize = 5;

        List<Board> tournament = new List<Board>();
        for (int i = 0; i < tournamentSize; i++)
        {
            int index = random.Next(population.Count);
            tournament.Add(population[index]);
        }

        return tournament.OrderBy(b => Fitness(b)).First();
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
                if (i < boardSize)
                {
                    childQueens[i] = parent1.Queens[i];
                }
            }

            int index = 0;
            for (int i = 0; i < boardSize; i++)
            {
                if (!childQueens.Contains(parent2.Queens[i]))
                {
                    try
                    {
                        while (index < boardSize && childQueens[index] != 0)
                        {
                            index++;
                        }

                        if (index < boardSize)
                        {
                            childQueens[index++] = parent2.Queens[i];
                        }
                    }
                    catch (Exception)
                    {
                        // Handle the exception, for example, by skipping this iteration
                        continue;
                    }
                }
            }

            return new Board(childQueens);
        }
        else
        {
            return parent1; // No crossover
        }
    }

    private Board Mutate(Board board)
    {
        if (random.NextDouble() < mutationProbability)
        {
            // Swap mutation
            int pos1 = random.Next(boardSize);
            int pos2 = random.Next(boardSize);

            if (pos1 < boardSize && pos2 < boardSize)
            {
                int temp = board.Queens[pos1];
                board.Queens[pos1] = board.Queens[pos2];
                board.Queens[pos2] = temp;
            }
        }

        return board;
    }

    private int Fitness(Board board)
    {
        int conflicts = 0;

        for (int i = 0; i < board.Size; i++)
        {
            for (int j = i + 1; j < board.Size; j++)
            {
                if (i < board.Size && j < board.Size &&
                    board.Queens[i] == board.Queens[j] ||
                    Math.Abs(i - j) == Math.Abs(board.Queens[i] - board.Queens[j]))
                {
                    conflicts++;
                }
            }
        }

        return conflicts;
    }

    private Random random = new Random();
}
