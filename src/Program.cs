using System;
using System.Collections.Generic;
using System.Data;

public class Connect4
{
    private const int noPce = 0;
    private const int red = 1;
    private const int yellow = 2;

    public class StdBoard
    {
        public int[,] board;
        public int turn;
        public int last_row;
        public int last_col;

        public StdBoard()
        {
            board = new int[6, 7];
            turn = red;
            last_row = 0;
            last_col = 0;
        }
    }

    private static StdBoard AddPiece(StdBoard boardClass, int col)
    {
        int pce = boardClass.turn;
        int last_row = 0;
        int last_col = col;

        for (int row = 5; row >= 0; row--)
        {
            if (boardClass.board[row, col] == noPce)
            {
                boardClass.board[row, col] = pce;
                last_row = row; // Update last_row to the current row
                break;
            }
        }

        boardClass.turn = 3 - pce;
        boardClass.last_row = last_row;
        boardClass.last_col = last_col;
        return boardClass;
    }

    private static void PrintBoard(StdBoard boardClass)
    {
        var board = boardClass.board;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                int pce = board[i, j];
                string pceChar = pce == noPce ? "- " : (pce == red ? "R " : "Y ");
                Console.Write(pceChar + " ");
            }
            Console.WriteLine();
            if (i != 5)
            {
                Console.WriteLine();
            }
        }
        Console.WriteLine("-------------------");
        Console.WriteLine("1| 2| 3| 4| 5| 6| 7 ");
        Console.WriteLine();
    }

    private static bool IsConnect4(StdBoard boardClass, int row, int col)
    {
        var board = boardClass.board;
        int pce = board[row, col] != noPce ? board[row, col] : -1;

        if (pce == -1) return false;

        // Check North
        if (row >= 3 && board[row - 1, col] == pce && board[row - 2, col] == pce && board[row - 3, col] == pce)
            return true;

        // Check South
        if (row <= 2 && board[row + 1, col] == pce && board[row + 2, col] == pce && board[row + 3, col] == pce)
            return true;

        // Check East
        if (col <= 3 && board[row, col + 1] == pce && board[row, col + 2] == pce && board[row, col + 3] == pce)
            return true;

        // Check West
        if (col >= 3 && board[row, col - 1] == pce && board[row, col - 2] == pce && board[row, col - 3] == pce)
            return true;

        // Check Northeast
        if (row >= 3 && col <= 3 && board[row - 1, col + 1] == pce && board[row - 2, col + 2] == pce && board[row - 3, col + 3] == pce)
            return true;

        // Check Southeast
        if (row <= 2 && col <= 3 && board[row + 1, col + 1] == pce && board[row + 2, col + 2] == pce && board[row + 3, col + 3] == pce)
            return true;

        // Check Northwest
        if (row >= 3 && col >= 3 && board[row - 1, col - 1] == pce && board[row - 2, col - 2] == pce && board[row - 3, col - 3] == pce)
            return true;

        // Check Southwest
        if (row <= 2 && col >= 3 && board[row + 1, col - 1] == pce && board[row + 2, col - 2] == pce && board[row + 3, col - 3] == pce)
            return true;

        return false;
    }

    private static int CheckWin(StdBoard boardClass)
    {
        var board = boardClass.board;
        if (IsConnect4(boardClass, boardClass.last_row, boardClass.last_col))
        {
            return board[boardClass.last_row, boardClass.last_col];
        }
        return noPce;
    }

    private static List<int> MoveGen(StdBoard boardClass)
    {
        var moves = new List<int>();
        var board = boardClass.board;
        for (int col = 0; col < 7; col++)
        {
            if (board[0, col] == noPce)
            {
                moves.Add(col);
            }
        }
        return moves;
    }

    private static int Perfd(StdBoard boardClass, int depth)
    {
        if (depth == 0 || CheckWin(boardClass) != noPce)
        {
            return 1;
        }
        int nodes = 0;
        var legals = MoveGen(boardClass);
        foreach (var col in legals)
        {
            var currentBoard = CopyBoard(boardClass);
            currentBoard = AddPiece(currentBoard, col);
            nodes += Perfd(boardClass, depth - 1);
        }
        return nodes;
    }

    public static void Perft(StdBoard boardClass, int maxDepth)
    {
        var startTime = DateTime.Now;
        for (int depth = 1; depth <= maxDepth; depth++)
        {
            int nodes = Perfd(boardClass, depth);
            var elapsed = DateTime.Now - startTime;
            Console.WriteLine($"info string perft depth {depth} time {(int)elapsed.TotalMilliseconds} nodes {nodes} nps {(int)(nodes / elapsed.TotalSeconds)}");
        }
    }

    private static MCTSNode bestMove;
    private static int computer_turn = noPce;

    public class MCTSNode
    {
        public StdBoard board;
        public int move;
        public MCTSNode parent;
        public List<MCTSNode> children;
        public int visits;
        public int value;

        public MCTSNode(StdBoard board, int move = -1, MCTSNode parent = null)
        {
            this.board = board;
            this.move = move;
            this.parent = parent;
            this.children = new List<MCTSNode>();
            this.visits = 0;
            this.value = 0;
        }
    }

    private static double Ucb1(MCTSNode node)
    {
        double c = Math.Sqrt(2);

        return node.value / (node.visits + 0.0000001) + c * Math.Sqrt(Math.Log(node.parent.visits) / (node.visits + 0.0000001));
    }

    private static MCTSNode Select(MCTSNode node)
    {
        while (node.children.Count > 0)
        {
            node = node.children.Aggregate((maxChild, nextChild) => Ucb1(nextChild) > Ucb1(maxChild) ? nextChild : maxChild);
        }
        return node;
    }

    private static void Expand(MCTSNode node)
    {
        var legals = MoveGen(node.board);
        foreach (var move in legals)
        {
            var newBoard = CopyBoard(node.board);
            newBoard = AddPiece(newBoard, move);
            var childNode = new MCTSNode(newBoard, move, node);
            node.children.Add(childNode);
        }
    }

    private static StdBoard CopyBoard(StdBoard board)
    {
        var newBoard = new StdBoard();
        Array.Copy(board.board, newBoard.board, board.board.Length);
        newBoard.turn = board.turn;
        return newBoard;
    }

    private static int Simulate(MCTSNode node)
    {
        var currentBoard = CopyBoard(node.board);
        while (CheckWin(currentBoard) == noPce)
        {
            var legals = MoveGen(currentBoard);
            if (legals.Count == 0)
            {
                return 0; // Draw
            }
            var move = legals[new Random().Next(legals.Count)];
            currentBoard = AddPiece(currentBoard, move);
        }
        return currentBoard.turn != computer_turn ? 1 : -1;
    }

    private static void Backpropagate(MCTSNode node, int result)
    {
        while (node != null)
        {
            node.visits++;
            node.value += result;
            node = node.parent;
        }
    }

    private static float Mcts(StdBoard board, int iterations = 100000)
    {
        bestMove = null;
        var root = new MCTSNode(board);
        for (int i = 0; i < iterations; i++)
        {
            var leaf = Select(root);
            if (leaf.visits > 0)
            {
                Expand(leaf);
                leaf = Select(leaf);
            }
            int result = Simulate(leaf);
            Backpropagate(leaf, result);
        }

        var bestChild = root.children.Aggregate((maxChild, nextChild) => nextChild.visits > maxChild.visits ? nextChild : maxChild);
        bestMove = bestChild;
        return (float)bestChild.value / (float)bestChild.visits;
    }

    public static void Main(string[] args)
    {
        while (true)
        {
            var boardClass = new StdBoard();
            Console.WriteLine(@"
   ______                            __        __ __
  / ____/___  ____  ____  ___  _____/ /_      / // /
 / /   / __ \/ __ \/ __ \/ _ \/ ___/ __/_____/ // /_
/ /___/ /_/ / / / / / / /  __/ /__/ /_/_____/__  __/
/____/\____/_/ /_/_/ /_/\___/\___/\__/        /_/   
                                                    
");
            Console.Write("Start as 1st player (red R) or 2nd player(yellow Y) [r/y]: ");
            string input = Console.ReadLine();
            int turn = input.ToLower() == "r" ? red : yellow;
            computer_turn = 3 - turn;
            while (true)
            {
                PrintBoard(boardClass);
                int potentialWin = CheckWin(boardClass);
                if (potentialWin != noPce)
                {
                    string winnerChar = potentialWin == red ? "Red" : "Yellow";
                    Console.WriteLine($"{winnerChar} won by connecting 4!");
                    break;
                }

                if (MoveGen(boardClass).Count == 0)
                {
                    Console.WriteLine("The game is drawn");
                }

                if (boardClass.turn == turn)
                {
                    Console.Write("Pick a column (1-7): ");
                    int col = int.Parse(Console.ReadLine());
                    boardClass = AddPiece(boardClass, col - 1);
                }
                else
                {
                    Console.WriteLine($"Computer WDL: {Mcts(boardClass)}");
                    boardClass = AddPiece(boardClass, bestMove.move);
                }
            }

            Console.Write("Type 'Q' to Exit else ENTER: ");
            string input2 = Console.ReadLine();
            if (input2 == "Q")
            {
                break;
            }

        }
    }
}