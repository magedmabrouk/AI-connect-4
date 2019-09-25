using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace connect_4
{
    public partial class Form1 : Form
    {
        int depth = 9; //Dictates AI difficulty level 
        bool human_first = true;
        private Rectangle [] coloumns;

        //every player begins the game with a number of points represents all the remaining possible winning plays which is 69
        int player1_points = 69; 
        int player2_points = 69; 

        //each position in the board has an "effective value"
        //which represents the number of possible winning plays that you deny from the opponent after playing in that position
        int[,] vertical_value = new int[6, 7];  //vertical winning plays you deny after playing in certain position 
        int[,] horizontal_value = new int[6, 7]; //horizontal winning plays you deny after playing in certain position 
        int[,] diagonal_up_value = new int[6, 7];  //diagonal up winning plays you deny after playing in certain position 
        int[,] diagonal_down_value = new int[6, 7]; //diagonal down winning plays you deny after playing in certain position 

        int[,] currentStateOfTheBoard = new int[6, 7]; // 0 means the is empty, 1 means the hole is filled with red checker, 2 means the hole is filled with green checker
        int turn = 1;
        

        public Form1()
        {
            InitializeComponent();

            this.coloumns = new Rectangle[7]; //the game board
            DialogResult dialog = MessageBox.Show("Do you want to play first", "order", MessageBoxButtons.YesNo);
            if (dialog == DialogResult.No) human_first = false;

            //Calculating the effective value for each position
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    int horizontal_count = 0; int vertical_count = 0; int diagonal_up_count = 0;

                    //looking for how many "four in a row" this position is part of 
                    for (int k = 0; k < 4; k++)
                    {
                        bool x_exist = false; bool y_exist = false;
                        if (i - k >= 0 && i + (3 - k) < 6) { x_exist = true; vertical_count++; }
                        if (j - k >= 0 && j + (3 - k) < 7) { y_exist = true; horizontal_count++; }
                        if (x_exist && y_exist) diagonal_up_count++;
                    }
                    vertical_value[i, j] = vertical_count; 
                    horizontal_value[i, j] = horizontal_count;
                    diagonal_up_value[i, j] = diagonal_up_count; 
                    diagonal_down_value[i, 6 - j] = diagonal_up_count; //diagonal down value is just a mirrored version of diagonal up value 
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Drawing the board
            e.Graphics.FillRectangle(Brushes.Blue, 24, 24, 340, 300);
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (i == 0) this.coloumns[j] = new Rectangle(32 + (48 * j), 24, 32, 300);
                    e.Graphics.FillEllipse(Brushes.White, 32 + (48 * j), 32 + (48 * i), 32, 32);
                }
            }

            //initializing the first play
            if ( !human_first) AI(depth);
        }

        private int mouse_click_col_index(Point location)
        {
            for (int i = 0; i < 7; i++)
            {
                if ((location.X >= this.coloumns[i].X) && (location.Y >= this.coloumns[i].Y)
                     && (location.X <= this.coloumns[i].X + this.coloumns[i].Width) && (location.Y <= this.coloumns[i].Y + this.coloumns[i].Height))
                    return i;
            }
            return -1;
        }

        //Determines what happens when the human player click the mouse in his turn
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
           int col = mouse_click_col_index(e.Location);
           int row = -1 ;
           if (col != -1)
           {
               for (int i = 0; i < 6; i++)
               {
                   if (currentStateOfTheBoard[i, col] == 0)
                   { row = i; break; }
               }
           }
           if (row == -1) MessageBox.Show("The column is full!");
           else
           {
               currentStateOfTheBoard[row, col] = turn;
               if (turn == 1)
               {
                   Graphics CreateChecker = this.CreateGraphics();
                   CreateChecker.FillEllipse(Brushes.Red, 32 + (48 * col), 32 + (48 * (5 - row)), 32, 32);
                   player2_points = possible_winning_plays_remaining(currentStateOfTheBoard, row, col, turn, player2_points);
                   if (check_win(currentStateOfTheBoard, row, col, turn))
                   {
                       MessageBox.Show("Player "+turn.ToString() +"win!");
                       Application.Restart();
                   }
                   turn = 2;
                   AI(depth);
               }
               else
               {
                   Graphics CreateChecker = this.CreateGraphics();
                   CreateChecker.FillEllipse(Brushes.Green, 32 + (48 * col), 32 + (48 * (5 - row)), 32, 32);
                   player1_points = possible_winning_plays_remaining(currentStateOfTheBoard, row, col, turn, player1_points);
                   if (check_win(currentStateOfTheBoard, row, col, turn))
                   {
                       MessageBox.Show("Player " + turn.ToString() + "win!");
                       Application.Restart();
                   }
                   turn = 1;
                   AI(depth);
               }
           }
        }

        //Check the last move to detect if it caused a "four in a row" by any shape 
        public bool check_win(int[,] current_state, int x, int y, int AccordingToWhichPlayer)
        {
            if (horizontal_win(current_state, x, y, AccordingToWhichPlayer) || vertical_win(current_state, x, y, AccordingToWhichPlayer) ||
                diagonal_up_win(current_state, x, y, AccordingToWhichPlayer) || diagonal_down_win(current_state, x, y, AccordingToWhichPlayer))
                return true;
            else return false;
        }

        public bool diagonal_down_win(int[,] current_state, int x, int y, int AccordingToWhichPlayer)
        {
            int count = 0;
            while (x > 0 && y < 7)
            {
                x--; y++;
            }
            if (y == 7) { y--; x++; }
            while (x < 6 && y > 0)
            {
                if (current_state[x, y] == AccordingToWhichPlayer)
                    count++;
                else count = 0;
                if (count == 4) return true;
                x++; y--;
            }
            return false;
        }

        public bool diagonal_up_win(int[,] current_state, int x, int y, int AccordingToWhichPlayer)
        {
            int count = 0;
            while (x > 0 && y > 0)
            {
                x--; y--;
            }

            while (x < 6 && y < 7)
            {
                if (current_state[x, y] == AccordingToWhichPlayer)
                    count++;
                else count = 0;
                if (count == 4) return true;
                x++; y++;
            }
            return false;
        }

        public bool vertical_win(int[,] current_state, int x, int y, int AccordingToWhichPlayer)
        {
            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                if (current_state[i, y] == AccordingToWhichPlayer)
                    count++;
                else count = 0;
                if (count == 4) return true;
            }
            return false;
        }

        public bool horizontal_win(int[,] current_state, int x, int y, int AccordingToWhichPlayer)
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                if (current_state[x, i] == AccordingToWhichPlayer)
                    count++;
                else count = 0;
                if (count == 4) return true;
            }
            return false;
        }


        //Searching for the optimal solution using min/max algorithem with alpha-beta pruninig
        public int search(int[,] current_state, int us, int them, int according, int goal_depth, int current_depth, int alpha, int beta, out int x_goal, out int y_goal)
        {
            current_depth++;
            //end of recursion
            if (current_depth > goal_depth) 
            { x_goal = -1; y_goal = -1; return us - them; }  //heuristic = our remaining possible wins - their remaining possible wins

            bool max, min;
            if (current_depth % 2 == 1) { max = true; min = false; }
            else { max = false; min = true; }

            int next_turn;
            if (according == 1) next_turn = 2;
            else next_turn = 1;

            int BestCandidateSoFar;
            if (max) BestCandidateSoFar = -10000; //arbitrary min value
            else BestCandidateSoFar = 10000; //arbitrary max value

           int BestCandidate_X_Position = -1; int BestCandidate_Y_Position = -1;

            int[,] state = new int[6, 7]; // a copy of the actual current board state to modify it
            for (int i = 0; i < 7; i++)
            {
                if (alpha >= beta) break;
                Buffer.BlockCopy(current_state, 0, state, 0, current_state.Length * sizeof(int));
                for (int j = 0; j < 6; j++)
                {

                    if (current_state[j, i] == 0)
                    {
                        state[j, i] = according;
                        bool win = check_win(state, j, i, according); int heuristic; int new_us = us; int new_them = them;
                        if (!win)
                        {
                            if (max) { new_them = possible_winning_plays_remaining(state, j, i, according, them); }
                            else { new_us = possible_winning_plays_remaining(state, j, i, according, us); }
                            heuristic = search(state, new_us, new_them, next_turn, goal_depth, current_depth, alpha, beta, out x_goal, out y_goal);
                        }
                        else { if (max) heuristic = 1000; else { heuristic = -1000; } }

                        if (max) 
                        { 
                            if (heuristic > BestCandidateSoFar) 
                            { 
                                BestCandidateSoFar = heuristic; BestCandidate_X_Position = j; BestCandidate_Y_Position = i; 
                            } 
                            if (heuristic > alpha) { alpha = heuristic; } 
                        }
                        if (min)
                        { 
                            if (heuristic < BestCandidateSoFar) 
                            { 
                                BestCandidateSoFar = heuristic; BestCandidate_X_Position = j; BestCandidate_Y_Position = i; 
                            } 
                            if (heuristic < beta) { beta = heuristic; } 
                        }
                        break;

                    }
                }
            }
            x_goal = BestCandidate_X_Position; 
            y_goal = BestCandidate_Y_Position;
            return BestCandidateSoFar;
        }

        //To intialize the searching process
        public void solve(int[,] current_state, int us, int them, int according, int depth, out int x, out int y)
        {
            int goal_x = 10; int goal_y = 10;
            search(current_state, us, them, according, depth, 0, -10000, 10000, out goal_x, out goal_y);
            x = goal_x; y = goal_y;
        }

        public int possible_winning_plays_remaining(int[,] current_state, int x, int y, int according, int last)
        {
            int max_hor_rng = 0; int max_ver_rng = 0; int max_dia_up_rng = 0; int max_dia_down_rng = 0;
            for (int i = 1; i < 4; i++)
            {
                if (x + i < 6)
                {
                    if (current_state[x + i, y] == according && vertical_value[x + i, y] > max_ver_rng)
                    { max_ver_rng = vertical_value[x + i, y]; }
                }
                if (x - i >= 0)
                {
                    if (current_state[x - i, y] == according && vertical_value[x - i, y] > max_ver_rng)
                    { max_ver_rng = vertical_value[x - i, y]; }
                }
                if (y + i < 7)
                {
                    if (current_state[x, y + i] == according && horizontal_value[x, y + i] > max_hor_rng)
                    { max_hor_rng = horizontal_value[x, y + i]; }
                }
                if (y - i >= 0)
                {
                    if (current_state[x, y - i] == according && horizontal_value[x, y - i] > max_hor_rng)
                    { max_hor_rng = horizontal_value[x, y - i]; }
                }
                if (y + i < 7 && x + i < 6)
                {
                    if (current_state[x + i, y + i] == according && diagonal_up_value[x + i, y + i] > max_dia_up_rng)
                    { max_dia_up_rng = diagonal_up_value[x + i, y + i]; }
                }
                if (y - i >= 0 && x - i >= 0)
                {
                    if (current_state[x - i, y - i] == according && diagonal_up_value[x - i, y - i] > max_dia_up_rng)
                    { max_dia_up_rng = diagonal_up_value[x - i, y - i]; }
                }
                if (y + i < 7 && x - i >= 0)
                {
                    if (current_state[x - i, y + i] == according && diagonal_down_value[x - i, y + i] > max_dia_down_rng)
                    { max_dia_down_rng = diagonal_down_value[x - i, y + i]; }
                }
                if (y - i >= 0 && x + i < 6)
                {
                    if (current_state[x + i, y - i] == according && diagonal_down_value[x + i, y - i] > max_dia_down_rng)
                    { max_dia_down_rng = diagonal_down_value[x + i, y - i]; }
                }
            }
            int hor, ver, dia_up, dia_down;
            if (vertical_value[x, y] > max_ver_rng) ver = vertical_value[x, y] - max_ver_rng; else ver = 0;
            if (horizontal_value[x, y] > max_hor_rng) hor = horizontal_value[x, y] - max_hor_rng; else hor = 0;
            if (diagonal_up_value[x, y] > max_dia_up_rng) dia_up = diagonal_up_value[x, y] - max_dia_up_rng; else dia_up = 0;
            if (diagonal_down_value[x, y] > max_dia_down_rng) dia_down = diagonal_down_value[x, y] - max_dia_down_rng; else dia_down = 0;
            return last - (ver + hor + dia_down + dia_up);
        }

        //Control the AI turn
        public void AI(int depth)
        {
            int row, col;
            if (turn == 1)
            {
                solve(currentStateOfTheBoard, player1_points, player2_points, turn, depth, out row, out col);
                currentStateOfTheBoard[row, col] = turn;
                Graphics checker = this.CreateGraphics();
                checker.FillEllipse(Brushes.Red, 32 + (48 * col), 32 + (48 * (5 - row)), 32, 32);
                player2_points = possible_winning_plays_remaining(currentStateOfTheBoard, row, col, turn, player2_points);
                if (check_win(currentStateOfTheBoard, row, col, turn))
                {
                    MessageBox.Show("Player " + turn.ToString() + "win!");
                    Application.Restart();
                }
                turn = 2;

            }
            else
            {
                solve(currentStateOfTheBoard, player2_points, player1_points, turn, depth, out row, out col);
                currentStateOfTheBoard[row, col] = turn;
                Graphics checker = this.CreateGraphics();
                checker.FillEllipse(Brushes.Green, 32 + (48 * col), 32 + (48 * (5 - row)), 32, 32);
                player1_points = possible_winning_plays_remaining(currentStateOfTheBoard, row, col, turn, player1_points);
                if (check_win(currentStateOfTheBoard, row, col, turn))
                {
                    MessageBox.Show("Player " + turn.ToString() + "win!");
                    Application.Restart();
                }
                turn = 1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            depth = 3;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            depth = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            depth = 10;
        }
    }
}
