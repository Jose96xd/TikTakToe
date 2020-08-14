using System;
using System.Collections.Generic;

namespace TikTakToe
{
    /* This is how the map is going to look like: 2 spaces between walls and signs and 0 between floor and signs.
     * 
     *                                          Withd = 13
     *                                        ,111,111,111,
     *                             Ceiling 1  ╔═══╦═══╦═══╗ ,
     *                                        ║ X ║ X ║ X ║ 1    Y = 1
     *                         Block1    ╔    ╠═══╬═══╬═══╣ ,          
     *            Height = 7             ╚    ║ X ║ X ║ X ║ 1    Y = 3              
     *                         Block2    ╔    ╠═══╬═══╬═══╣ ,                 
     *                                   ╚    ║ X ║ X ║ X ║ 1    Y = 5                  
     *                              Floor3    ╚═══╩═══╩═══╝ ,                                                                                                     
     *                                        x = 2, 6, 10.
     *  The areas where the user can answer are (∀X, Y /  X = 2 + horizontalCenteringValue + 4λ  
     *                                                  &  Y = 1 + verticalCenteringValue + 2λ)  with λ ϵ [0-2]. 
     *  
     *  
     */

    class Program
    {   
        static void Main(string[] args) => new Program().Run();

        enum GameStatus { Tie, X, O, NothingYet}

        GameStatus currentGameState;                // The current status of the game, either the game is finished or NothingYet.

        // A struct to save all the posible locations where a player can place their sing.
        class Coordinate                    
        {
            public int X, Y;
            public Coordinate(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        // We are going to have 2 sets of colors (for backGround and foreGround) to change between them every time we restart the game.
        ConsoleColor currentBackgroundColor = ConsoleColor.Black, nextBackgroundColor = ConsoleColor.DarkCyan;
        ConsoleColor currentForegroundColor = ConsoleColor.White, nextForegroundColor = ConsoleColor.White;

        int horizontalCenteringValue, verticalCenteringValue;
        int distanceBetweenRows, distanceBetweenCollumns;           // The distance between  the places where the user can place their sings.
        int currentPositionX, currentPositionY;                     // This is to print in the screen.
        int scorePlayer1, scorePlayer2;

        bool isAgainstAI;                                         // If the game is multiplayer then each turn is played by a user.
                                                                    // If is not multiplayer then one of the turn is played by the IA.

        
        string gameFinishString = "Game finished !!!";
        string scorePlayer1String = "Player1 score is: ";
        string scorePlayer2String = "Player2 score is: ";
        string controlsString = "R: Restart. \tEsc to finish.";
        string infoString = "Tic Tac Toe by José96xd in C#. August 2020.";
        string turnString = "Is the turn of ";
        string winnerString = "And the winner is ";
        string tieString    = "Ups, Nobody has win...";

        private char[][] gameBoard;     

        // The first player, second player and the player which turn is the actual one.
        ConsoleKeyInfo firstPlayerKeyInfo, secondPlayerKeyInfo, currentPlayerKeyInfo;   

        // This method invoke all the necesary methods in the correct order to start the game.
        public void Run()
        {
            SetInitialSettings(true);
            Draw();
            PrintInitialMessages();

            GameLoop();
        }

        // Ask the player with which sign they want to play between X and O. 
        // If the player answers with an incorrect sign, ask again until they answer something valid 
        private void SetPlayersSing()
        {
            Console.WriteLine("What sing do you want to play with? (O/X)\nClick the corresponding key.");
            firstPlayerKeyInfo = Console.ReadKey(true);
            Console.WriteLine();

            while (!firstPlayerKeyInfo.Key.Equals(ConsoleKey.X) && !firstPlayerKeyInfo.Key.Equals(ConsoleKey.O))
            {
                Console.WriteLine("Thats not a valid sing, please choose between O and X.");
                firstPlayerKeyInfo = Console.ReadKey(true);
            }

            if (firstPlayerKeyInfo.Key.Equals(ConsoleKey.X))
                secondPlayerKeyInfo = new ConsoleKeyInfo('O', ConsoleKey.O, false, false, false);
            else
                secondPlayerKeyInfo = new ConsoleKeyInfo('X', ConsoleKey.X, false, false, false);

            currentPlayerKeyInfo = firstPlayerKeyInfo;

            Console.Clear();
        }

        // Ask the player if they want to play against other human or against the AI.
        private void SetGameMode()
        {
            ConsoleKey userModeSelectionAnswer;

            // Asking which game mode the user wants to play.
            Console.WriteLine("Do you want to play agains my ultra-advanced AI or agains other human? (I = Against Ai;  M = Multiplayer;).");
            userModeSelectionAnswer = Console.ReadKey().Key;

            while (!userModeSelectionAnswer.Equals(ConsoleKey.I) && !userModeSelectionAnswer.Equals(ConsoleKey.M))
            {
                Console.WriteLine("Thats not a valid option, please choose between I and M.");
                userModeSelectionAnswer = Console.ReadKey().Key;
            }
            isAgainstAI = userModeSelectionAnswer.Equals(ConsoleKey.I) ? true : false;

            Console.Clear();
        }

        // This method initialize some filds and ask the user which sing want to play with (either X or O) and if they want to play agains other humar or the AI.
        private void SetInitialSettings(bool firstMatch)
        {
            currentPositionX = currentPositionY = 0;
            currentGameState = GameStatus.NothingYet;

            if (firstMatch)         // If this is not the first game we should not delete the scores.
            {
                scorePlayer1 = 0;
                scorePlayer2 = 0;
            }

            gameBoard = new char[][] { new[] { ' ', ' ', ' ' }, new[] { ' ', ' ', ' ' }, new[] { ' ', ' ', ' ' } };


            Console.BackgroundColor = currentBackgroundColor;
            Console.ForegroundColor = currentForegroundColor;

            Console.Clear();

            SetPlayersSing();
            SetGameMode();
        }

        // This method prints all the board in the screen centering in the middle.
        private void Draw()
        {
            Console.BackgroundColor = currentBackgroundColor;
            Console.ForegroundColor = currentForegroundColor;

            string Ceiling         = "╔═══╦═══╦═══╗";
            string Walls           = "║   ║   ║   ║";      // This middle section needs to be repeated 2 times.
            string MiddleSection   = "╠═══╬═══╬═══╣";      // In the end is going to be: Ceiling-Wals; Middle-Walls x 2; Floor.
            string Floor           = "╚═══╩═══╩═══╝";

            horizontalCenteringValue = (Console.BufferWidth - Ceiling.Length) / 2;      // This two variables are used to center the game.
            verticalCenteringValue = horizontalCenteringValue / 6;
            distanceBetweenCollumns = 4;
            distanceBetweenRows = 2;

            char[][] boardDecorations = new char[][] { Ceiling.ToCharArray(), Walls.ToCharArray(), MiddleSection.ToCharArray(), Walls.ToCharArray(),
                                    MiddleSection.ToCharArray(), Walls.ToCharArray(), Floor.ToCharArray() };

            for (int i = 0; i < boardDecorations.Length; i++)
            {
                Console.SetCursorPosition(horizontalCenteringValue, verticalCenteringValue + i);

                for (int j = 0; j < boardDecorations[i].Length; j++)
                {
                    Console.Write(boardDecorations[i][j]);
                }
            }
        }

        // Prints the initial messages (Current player, score of both players, control keys, and author.
        private void PrintInitialMessages()
        {
            Console.BackgroundColor = currentBackgroundColor;
            Console.ForegroundColor = currentForegroundColor;

            // Info sing
            Console.SetCursorPosition((Console.BufferWidth - infoString.Length) / 2, 3 * verticalCenteringValue);
            Console.Write(infoString);

            // Control sing.
            Console.SetCursorPosition((horizontalCenteringValue + (3 * distanceBetweenCollumns) + (controlsString.Length / 2)), verticalCenteringValue + distanceBetweenRows);
            Console.Write(controlsString);

            // Score Player1 sing.
            Console.SetCursorPosition((scorePlayer1String.Length + 1) / 2 + distanceBetweenCollumns, verticalCenteringValue + distanceBetweenRows);
            Console.Write(scorePlayer1String + scorePlayer1);

            // Score Player2 sing.
            Console.SetCursorPosition((scorePlayer2String.Length + 1) / 2 + distanceBetweenCollumns, verticalCenteringValue + (2 * distanceBetweenRows));
            Console.Write(scorePlayer2String + scorePlayer2);


            // Player turn sing.
            Console.SetCursorPosition((Console.BufferWidth - turnString.Length) / 2, verticalCenteringValue / 2);
            Console.Write(turnString + Char.ToUpper(currentPlayerKeyInfo.KeyChar) );
            Console.SetCursorPosition(horizontalCenteringValue + 2, verticalCenteringValue + 1);
        }

        // This method gets the input from the user (in  this case the arrows) and moves the current position of the Cursor.
        // It jumps from allowed position to allowed position and if the user wants to go to an illegal position it jumps to the other end of the board to prevent it.
        private void InputMove(ConsoleKey keyInput)
        {
            switch (keyInput)
            {
                case ConsoleKey.UpArrow:
                    if (Console.CursorTop - verticalCenteringValue - distanceBetweenRows >= 0)
                    {
                        currentPositionY -= 1;
                    }
                    else
                    {
                        currentPositionY += 2;
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (Console.CursorTop + distanceBetweenRows <= verticalCenteringValue + (3 * distanceBetweenRows))
                    {
                        currentPositionY += 1;
                    }
                    else
                    {
                        currentPositionY -= 2;
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (Console.CursorLeft - distanceBetweenCollumns > horizontalCenteringValue)
                    {
                        currentPositionX -= 1;
                    }
                    else
                    {
                        currentPositionX += 2;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (Console.CursorLeft <= horizontalCenteringValue + (3 * distanceBetweenRows))
                    {
                        currentPositionX += 1;
                    }
                    else
                    {
                        currentPositionX -= 2;
                    }
                    break;

            }

            Console.SetCursorPosition((horizontalCenteringValue + 2) + (currentPositionX * distanceBetweenCollumns), (verticalCenteringValue + 1) + (currentPositionY * distanceBetweenRows));

        }

        // This method swap the background and foreground colors between two sets.
        // Its called once per restart to add some variety.
        private void Swap<T>(ref T object1, ref T object2)
        {
            T aux = object1;

            object1 = object2;
            object2 = aux;
        }

        // This method is called once the user press the R key. It paints the map again and clean all the places where an answer was.
        private void Restart()
        {
            Swap<ConsoleColor>(ref currentBackgroundColor, ref nextBackgroundColor);
            Swap<ConsoleColor>(ref currentForegroundColor, ref nextForegroundColor);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            SetInitialSettings(false);
            Draw();
            PrintInitialMessages();
        }

        // Returns a list with all the coordinates of the array in which a sign can be placed.
        private List<Coordinate> PossibleMoves(char[][] board)
        {
            List<Coordinate> posibleMovesList = new List<Coordinate>();

            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if (board[i][j].Equals(' '))
                    {
                        posibleMovesList.Add(new Coordinate(j, i));
                    }
                    
                }
            }
            return posibleMovesList;
        }

        // Returns the sing of the winner. Is called once we have verified that there are 3 equal sings in line.
        private GameStatus WhoIsTheWinner(char aux)
        {
            GameStatus winner = GameStatus.NothingYet;

            if (aux == 'X')
                winner = GameStatus.X;
            else if(aux == 'O')
                winner = GameStatus.O;

            return winner;
        }

        // Checks all columns, rows and diagonals to check if there are 3 equal sings. In that case it calls the WhoIsTheWinner() method and returns the winner sing.
        private GameStatus CheckForWiners(char[][] board)
        {
            List<Coordinate> possibleMoves = PossibleMoves(board);

            for(int i = 0; i < 3; i++)  // Checking the rows.
            {
                if (board[i][0] == board[i][1] && board[i][0] == board[i][2] && !WhoIsTheWinner(board[i][0]).Equals(GameStatus.NothingYet) )    // Checking rows.
                    return WhoIsTheWinner(board[i][0]);
            }

            for (int j = 0; j < 3 ; j++) // Checking the collumns.
            {
                if ( (board[0][j] == board[1][j] && board[0][j] == board[2][j]) && !(WhoIsTheWinner(board[0][j]).Equals(GameStatus.NothingYet)) )   // Checking collumns.
                    return WhoIsTheWinner(board[0][j]);
            }

            if ( (board[0][0] == board[1][1] && board[0][0] == board[2][2]) && !(WhoIsTheWinner(board[0][0]).Equals(GameStatus.NothingYet)) )   // Checking the first diagonal '\'.
                return WhoIsTheWinner(board[0][0]);
            
            if ( (board[2][0] == board[1][1] && board[1][1] == board[0][2]) && !(WhoIsTheWinner(board[1][1]).Equals(GameStatus.NothingYet)))   // Checking the second diagonal '/'.
                return WhoIsTheWinner(board[1][1]);

            if (possibleMoves.Count == 0 )     // Checking if there is no more available moves and no ones has win (a tie situation).
                return GameStatus.Tie;

            return GameStatus.NothingYet;
        }

        // Prints the winner of the game whenever someone has win. It also clears the Player's turn message to show who has won.
        private void DisplayWinnerMessage(GameStatus currentStatus)
        {
            int oldTop = Console.CursorTop, oldLeft = Console.CursorLeft;
            Console.BackgroundColor = currentBackgroundColor;
            Console.ForegroundColor = currentForegroundColor;

            Console.SetCursorPosition((Console.BufferWidth - winnerString.Length - 4) / 2, verticalCenteringValue / 2);

            switch(currentStatus)
            {
                case GameStatus.X:
                    Console.Write(winnerString + "X!!!");
                    break;
                        
                case GameStatus.O:
                    Console.Write(winnerString + "O!!!");
                    break;
                
                case GameStatus.Tie:
                    Console.Write(tieString);
                    break;
            }

            Console.SetCursorPosition(oldLeft, oldTop);
        }

        // Prints the user sing in the 
        private void WritePlayerSing(ConsoleKeyInfo playerKeyInfo)
        {
            Console.Write(playerKeyInfo.KeyChar.ToString().ToUpper());
            gameBoard[currentPositionY][currentPositionX] = Char.ToUpper(playerKeyInfo.KeyChar);
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);       // We have to go back 1 position to keep the cursor aligned with the logic of the gameBoard.
        }

        // Prints the gameFinishString in the lower part of the screen (centered) and moves the cursor far away from the gameBoard.
        private void WriteGameFinish(string endGameString)
        {
            Console.SetCursorPosition((Console.BufferWidth - gameFinishString.Length) / 2, (2 * verticalCenteringValue));   
            Console.Write(gameFinishString);
            Console.SetCursorPosition(0, 3 * verticalCenteringValue + 2);       // After printing the string we move the Console.Cursor to a position far away of the map 
                                                                                // to prevent any system string messing the screeb.
        }

        // Updates the score messages with the new scores.
        private void UpdateScoreMessage()
        {
            int oldRow = Console.CursorTop, oldCollumn = Console.CursorLeft;

            // Score Player1 sing.
            Console.SetCursorPosition((scorePlayer1String.Length + 1) / 2 + distanceBetweenCollumns + scorePlayer1String.Length, verticalCenteringValue + distanceBetweenRows);
            Console.Write(scorePlayer1);

            // Score Player2 sing.
            Console.SetCursorPosition((scorePlayer2String.Length + 1) / 2 + distanceBetweenCollumns + scorePlayer2String.Length, verticalCenteringValue + (2 * distanceBetweenRows));
            Console.Write(scorePlayer2);

            Console.SetCursorPosition(oldCollumn, oldRow);
        }

        // This method combines both, the UpdateScoreMessage and the ChecForWinner.
        // First it updates the game status to see if someone has alredy win or if we are in a tie situation.
        // Then if we are in a terminal situation, it prints the situation in the screen and, if needed, update the score.
        // Returns True if the game can continue or false if there is already a winner or a tie situation.
        private bool UpdateScoresAndCheckWinners()
        {
            bool turnHasPassed = true;

            currentGameState = CheckForWiners(gameBoard);

            if (!currentGameState.Equals(GameStatus.NothingYet))
            {
                DisplayWinnerMessage(currentGameState);

                if (!currentGameState.Equals(GameStatus.Tie))
                {
                    if (currentPlayerKeyInfo.Key.Equals(firstPlayerKeyInfo.Key))
                        scorePlayer1++;
                    else
                        scorePlayer2++;

                    UpdateScoreMessage();
                }
                turnHasPassed = false;
            }
            return turnHasPassed;
        }

        // Gets the input of the user (the user sing or the restart/escape option) and then
        // it checks to only allow the sing of the current turn and to onlt allow it if the position if empty.
        private bool InputAnswer(ConsoleKeyInfo InputKey)
        {
            bool turnHasPassed = false;


            if (InputKey.Key.Equals(ConsoleKey.Escape))
            {
                WriteGameFinish(gameFinishString);
            }
            else if (InputKey.Key.Equals(ConsoleKey.R))
            {
                Restart();
            }
            else if (gameBoard[currentPositionY][currentPositionX] == ' ' && InputKey.Key.Equals(currentPlayerKeyInfo.Key))
            {
                WritePlayerSing(InputKey);
                
                turnHasPassed = UpdateScoresAndCheckWinners();
            }
            return turnHasPassed;
        }

        // Changes the current turn and also the message with the sing of that turn.
        private void UpdateCurrentPlayer()
        {
            Console.BackgroundColor = currentBackgroundColor;
            Console.ForegroundColor = currentForegroundColor;

            int oldRow = Console.CursorTop, oldCollumn = Console.CursorLeft;

            if (currentPlayerKeyInfo.Equals(firstPlayerKeyInfo))
            {
                currentPlayerKeyInfo = secondPlayerKeyInfo;
            }
            else
            {
                currentPlayerKeyInfo = firstPlayerKeyInfo;
            }
            // Player turn sing.
            Console.SetCursorPosition(((Console.BufferWidth - turnString.Length) / 2) + turnString.Length, verticalCenteringValue / 2);
            Console.Write(Char.ToUpper(currentPlayerKeyInfo.KeyChar));
            Console.SetCursorPosition(oldCollumn, oldRow);
        }


        // This is part of the IA implementation. In this case I have used the Minimax algorithm.
        // We give a value of 1 to a winning end, 0 to tie and -1 if the IA lose. 
        private int MiniMax(char[][] board, int depth, bool isMaximizing)
        {
            GameStatus currentState = CheckForWiners(gameBoard);

            // Using the int.MaxValue - depth we make the algorithm prefer to win in the least number of turns 
            // instead of having the immediate victory and the one more distant in time being equally optimal.
            switch (currentState)
            {
                case GameStatus.Tie:
                    return 0;
                    break;
                case GameStatus.O:                                       // Original was +1 : -1 which doesnt take into acount the time it takes to win.
                    return secondPlayerKeyInfo.Key.Equals(ConsoleKey.O) ? int.MaxValue - depth : int.MinValue + depth;
                    break;
                case GameStatus.X:                                       // Original was +1 : -1 
                    return secondPlayerKeyInfo.Key.Equals(ConsoleKey.X) ? int.MaxValue - depth : int.MinValue + depth;
                    break;
            }


            if(isMaximizing)
            {
                List<Coordinate> availableMoves = PossibleMoves(board);
                int bestScore = int.MinValue;

                foreach(Coordinate coord in availableMoves)
                {
                    board[coord.Y][coord.X] = Char.ToUpper(secondPlayerKeyInfo.KeyChar);
                    int score = MiniMax(board, depth + 1, false);
                    board[coord.Y][coord.X] = ' ';

                    bestScore = Math.Max(score, bestScore);
                }

                return bestScore;
            }
            else
            {
                List<Coordinate> availableMoves = PossibleMoves(board);
                int bestScore = int.MaxValue;

                foreach (Coordinate coord in availableMoves)
                {
                    board[coord.Y][coord.X] = Char.ToUpper(firstPlayerKeyInfo.KeyChar);
                    int score = MiniMax(board, depth + 1, true);
                    board[coord.Y][coord.X] = ' ';

                    bestScore = Math.Min(score, bestScore);
                }

                return bestScore;
            }


        }

        // Use the MiniMax algorithm to find the best possible move and then it does it.
        // Returns the boolean of the UpdateScoreAndCheckForWinners();
        private bool AIMove()
        {
            int oldTop = Console.CursorTop, oldLeft = Console.CursorLeft;
            List<Coordinate> available = PossibleMoves(gameBoard);
            Coordinate bestCoordinate = available[0];
            int bestScore = int.MinValue;
            bool turnHasPassed;

            foreach(Coordinate coord in available)
            {
                gameBoard[coord.Y][coord.X] = secondPlayerKeyInfo.KeyChar;
                int score = MiniMax(gameBoard, 0, false);
                if(score > bestScore)
                {
                    bestScore = score;
                    bestCoordinate = new Coordinate(coord.X, coord.Y);
                }
                gameBoard[coord.Y][coord.X] = ' ';

            }

            Console.SetCursorPosition(horizontalCenteringValue + 2 + (bestCoordinate.X * distanceBetweenCollumns), verticalCenteringValue + 1 + (bestCoordinate.Y * distanceBetweenRows) );
            Console.Write(Char.ToUpper(secondPlayerKeyInfo.KeyChar));
            gameBoard[bestCoordinate.Y][bestCoordinate.X] = secondPlayerKeyInfo.KeyChar;
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);


            turnHasPassed = UpdateScoresAndCheckWinners();

            Console.SetCursorPosition(oldLeft, oldTop);

            return turnHasPassed;
        }

        // This is the game loop where we check for user input and react in consecuense.
        private void GameLoop()
        {
            ConsoleKeyInfo userAnswer = new ConsoleKeyInfo();

            do
            {
                if (Console.KeyAvailable)
                {
                    userAnswer = Console.ReadKey(true);

                    if (char.IsLetter(userAnswer.KeyChar) || userAnswer.Key.Equals(ConsoleKey.Escape))
                    {
                        if (InputAnswer(userAnswer))
                        {
                            UpdateCurrentPlayer();

                            if (isAgainstAI && currentGameState == GameStatus.NothingYet)
                            {
                                if (AIMove())
                                    UpdateCurrentPlayer();
                            }
                        }
                    }
                    else
                    {
                        InputMove(userAnswer.Key);
                    }

                }

            } while (!userAnswer.Key.Equals(ConsoleKey.Escape));


        }

    }
}

