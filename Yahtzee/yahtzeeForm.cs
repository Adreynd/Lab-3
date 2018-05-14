using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// Example Yahtzee website if you've never played
// https://cardgames.io/yahtzee/

namespace Yahtzee
{
    public partial class yahtzeeForm : Form
    {
        public yahtzeeForm()
        {
            InitializeComponent();
        }

        // you may find these helpful in manipulating the scorecard and in other places in your code
        private const int NONE = -1;
        private const int ONES = 0;
        private const int TWOS = 1;
        private const int THREES = 2;
        private const int FOURS = 3;
        private const int FIVES = 4;
        private const int SIXES = 5;
        private const int THREE_OF_A_KIND = 6;
        private const int FOUR_OF_A_KIND = 7;
        private const int FULL_HOUSE = 8;
        private const int SMALL_STRAIGHT = 9;
        private const int LARGE_STRAIGHT = 10;
        private const int CHANCE = 11;
        private const int YAHTZEE = 12;

        private const int SCORE_CARD = 13;          // Number of elements the user can click on in the scorecard
        private const int SIDED_DICE = 6;           // Number of sides on the dice being used
        private const int DICE_COUNT = 5;           // Number of dice the user can roll/keep

        private int rollCount = 0;          // Number of times the dice have been rolled this turn
        private int uScoreCardCount = 0;    // Number of items that have been filled out in the UI by the user

        private int[] scores = new int[SCORE_CARD];                 // Array of scores the user has submited to the UI
        private List<int> keepDice = new List<int>(DICE_COUNT);     // List of dice to keep between rolls
        private List<int> rollDice = new List<int>(DICE_COUNT);     // List of dice to roll

        // you'll need an instance variable for the user's scorecard - an array of 13 ints
        // as well as an instance variable for 0 to 5 dice as the user rolls - array or list<int>?
        // as well as an instance variable for 0 to 5 dice that the user wants to keep - array or list<int>? 

        // this is the list of methods that I used

        // START WITH THESE 2
        // This method rolls numDie and puts the results in the list
        public void Roll(int numDie, List<int> dice)
        {
            Random rnd = new Random();

            for (int i = 0; i < numDie; i++)
                dice[i] = rnd.Next(1, SIDED_DICE + 1);
        }

        // This method moves all of the rolled dice to the keep dice before scoring.  All of the dice that
        // are being scored have to be in the same list 
        public void MoveRollDiceToKeep(List<int> roll, List<int> keep)
        {
            int index = GetFirstAvailablePB(keep);
            for (int i = 0; i < DICE_COUNT - index; i++)
            {
                keep[index + i] = roll[i];      // Put items from the roll list starting at index 0, into the keep list starting at index "index"
                roll[i] = -1;                   // Remove the item that you took from roll the roll list from the roll list
            }
        }

        #region Scoring Methods
        /* This method returns the number of times the parameter value occurs in the list of dice.
         * Calling it with 5 and the following dice 2, 3, 5, 5, 6 returns 2.
         */
        private int Count(int value, List<int> dice)
        {
            int counter = 0;

            for (int i = 0; i < dice.Count; i++)
                if (value == dice[i])           // If the value we are looking for is a part of the dice list...
                    counter++;                  // Add one to the counter
            return counter;
        }

        /* This method counts how many 1s, 2s, 3s ... 6s there are in a list of ints that represent a set of dice
         * It takes a list of ints as it's parameter.  It should create an array of 6 integers to store the counts.
         * It should then call Count with a value of 1 and store the result in the first element of the array.
         * It should then repeat the process of calling Count with 2 - 6.
         * It returns the array of counts.
         * All of the rest of the scoring methods can be "easily" calculated using the array of counts.
         */
        private int[] GetCounts(List<int> dice)
        {
            int[] array = new int[SIDED_DICE];

            for (int i = 0; i < SIDED_DICE; i++)
                array[i] = Count(i + 1, dice);
            return array;
        }

        /* Each of these methods takes the array of counts as a parameter and returns the score for a dice value.
         */
        private int ScoreOnes(int[] counts)
        {
            return counts[ONES];
        }

        private int ScoreTwos(int[] counts)
        {
            return counts[TWOS] * 2;
        }

        private int ScoreThrees(int[] counts)
        {
            return counts[THREES] * 3;
        }

        private int ScoreFours(int[] counts)
        {
            return counts[FOURS] * 4;
        }

        private int ScoreFives(int[] counts)
        {
            return counts[FIVES] * 5;
        }

        private int ScoreSixes(int[] counts)
        {
            return counts[SIXES] * 6;
        }

        /* This method can be used to determine if you have 3 of a kind (or 4? or  5?).  The output parameter
         * whichValue tells you which dice value is 3 of a kind.
         */ 
        private bool HasCount(int howMany, int[] counts, out int whichValue)
        {
            int index = ONES;
            foreach (int count in counts)
            {
                if (howMany == count)
                {
                    whichValue = index;
                    return true;
                }
                index++;
            }
            whichValue = NONE;
            return false;
        }

        /* This method returns the sum of the dice represented in the counts array.
         * The sum is the # of 1s * 1 + the # of 2s * 2 + the number of 3s * 3 etc
         */ 
        private int Sum()
        {
            int score = 0;

            score += scores[ONES];
            score += scores[TWOS];
            score += scores[THREES];
            score += scores[FOURS];
            score += scores[FIVES];
            score += scores[SIXES];

            return score;
        }

        /* This method calls HasCount(3...) and if there are 3 of a kind calls Sum to calculate the score.
         */
        private int ScoreThreeOfAKind(int[] counts)
        {
            for (int i = 0; i < SIDED_DICE; i++)
                if (counts[i] == 3)
                    return (i + 1) * 3;

            return 0;
        }

        private int ScoreFourOfAKind(int[] counts)
        {
            for (int i = 0; i < SIDED_DICE; i++)
                if (counts[i] == 4)
                    return (i + 1) * 4;

            return 0;
        }

        private int ScoreYahtzee(int[] counts)      // Five of a kind
        {
            for (int i = 0; i < SIDED_DICE; i++)
                if (counts[i] == 5)
                    return 50;
            return 0;
        }

        /* This method calls HasCount(2 and HasCount(3 to determine if there's a full house.  It calls sum to 
         * calculate the score.
         */ 
        private int ScoreFullHouse(int[] counts)
        {
            int count = 0;
            if (HasCount(2, counts, out count) && HasCount(3, counts, out count))
                return 25;
            return 0;
        }

        private int ScoreSmallStraight(int[] counts)
        {
            if ((counts[THREES] > 0 && counts[FOURS] > 0) && ((counts[TWOS] > 0 && (counts[ONES] > 0 || counts[FIVES] > 0)) ||
                (counts[FIVES] > 0 && counts[SIXES] > 0)))
                return 30;

            return 0;
        }

        private int ScoreLargeStraight(int[] counts)
        {
            if ((counts[TWOS] > 0 && counts[THREES] > 0 && counts[FOURS] > 0 && counts[FIVES] > 0) &&
                (counts[SIXES] > 0 || counts[ONES] > 0))
                return 40;

            return 0;
        }

        private int ScoreChance(int[] counts)
        {
            int score = 0;

            score += ScoreOnes(counts);
            score += ScoreTwos(counts);
            score += ScoreThrees(counts);
            score += ScoreFours(counts);
            score += ScoreFives(counts);
            score += ScoreSixes(counts);

            return score;
        }

        /* This method makes it "easy" to call the "right" scoring method when you click on an element
         * in the user score card on the UI.
         */ 
        private int Score(int whichElement, List<int> dice)
        {
            int[] counts = GetCounts(dice);
            switch (whichElement)
            {
                case ONES:
                    return ScoreOnes(counts);
                case TWOS:
                    return ScoreTwos(counts);
                case THREES:
                    return ScoreThrees(counts);
                case FOURS:
                    return ScoreFours(counts);
                case FIVES:
                    return ScoreFives(counts);
                case SIXES:
                    return ScoreSixes(counts);
                case THREE_OF_A_KIND:
                    return ScoreThreeOfAKind(counts);
                case FOUR_OF_A_KIND:
                    return ScoreFourOfAKind(counts);
                case FULL_HOUSE:
                    return ScoreFullHouse(counts);
                case SMALL_STRAIGHT:
                    return ScoreSmallStraight(counts);
                case LARGE_STRAIGHT:
                    return ScoreLargeStraight(counts);
                case CHANCE:
                    return ScoreChance(counts);
                case YAHTZEE:
                    return ScoreYahtzee(counts);
                default:
                    return 0;
            }
        }
        #endregion

        // set each value to some negative number because 
        // a 0 or a positive number could be an actual score
        private void ResetScoreCard(int[] scoreCard, int scoreCardCount)
        {
            scoreCard[scoreCardCount] = -1;
        }

        // this set has to do with user's scorecard UI
        private void ResetUserUIScoreCard()             // Reset the user score card UI
        {
            for (int i = 0; i < scores.Length; i++)
                scores[i] = -1;

            for (int i = ONES; i < SCORE_CARD; i++)
            {
                Label scoreCardElement = (Label)this.scoreCardPanel.Controls["user" + i];
                scoreCardElement.Text = "";
                scoreCardElement.Enabled = true;
            }

            uScoreCardCount = 0;
            userSum.Text = "";
            userBonus.Text = "";
            userTotalScore.Text = "";
        }

        // this method adds the subtotals as well as the bonus points when the user is done playing
        public void UpdateUserUIScoreCard()             // Calculate the total score, check if the bonus was achieved, and update their two scoreboxes
        {
            int total = 0;
            int bonus = 0;
            int sum;
            for (int i = 0; i < scores.Length; i++)
                total += scores[i];

            sum = Sum();

            if (sum >= 65)
                bonus = 35;
            userSum.Text = sum.ToString();
            total += bonus;
            userBonus.Text = bonus.ToString();
            userTotalScore.Text = total.ToString();
        }

        /* When I move a die from roll to keep, I put a -1 in the spot that the die used to be in.
         * This method gets rid of all of those -1s in the list.
         */
        private void CollapseDice(List<int> dice)
        {
            int numDice = dice.Count;
            for (int count = 0, i = 0; count < numDice; count++)
            {
                if (dice[i] == -1)
                {
                    dice.RemoveAt(i);
                    dice.Add(-1);
                }
                else
                    i++;
            }
        }

        /* When I move a die from roll to keep, I need to know which pb I can use.  It's the first spot with a -1 in it
         */
        public int GetFirstAvailablePB(List<int> dice)
        {
            return dice.IndexOf(-1);
        }

        #region UI Dice Methods
        /* These are all UI methods */
        private PictureBox GetKeepDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["keep" + i];
            return die;
        }

        public void HideKeepDie(int i)
        {
            GetKeepDie(i).Visible = false;
            GetKeepDie(i).Enabled = false;
        }
        public void HideAllKeepDice()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                HideKeepDie(i);
        }

        public void ShowKeepDie(int i)
        {
            PictureBox die = GetKeepDie(i);
            die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + keepDice[i] + ".png");
            die.Visible = true;
            die.Enabled = true;
        }

        public void ShowAllKeepDie()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                if (keepDice[i] > 0)
                    ShowKeepDie(i);
        }

        private PictureBox GetComputerKeepDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["computerKeep" + i];
            return die;
        }

        public void HideComputerKeepDie(int i)
        {
            GetComputerKeepDie(i).Visible = false;
        }

        public void HideAllComputerKeepDice()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                HideComputerKeepDie(i);
        }

        public void ShowComputerKeepDie(int i)
        {
            PictureBox die = GetComputerKeepDie(i);
            //die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + keep[i] + ".png");
            die.Visible = true;
        }

        public void ShowAllComputerKeepDie()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                ShowComputerKeepDie(i);
        }

        private PictureBox GetRollDie(int i)
        {
            PictureBox die = (PictureBox)this.Controls["roll" + i];
            return die;
        }

        public void HideRollDie(int i)
        {
            GetRollDie(i).Visible = false;
            GetRollDie(i).Enabled = false;
        }

        public void HideAllRollDice()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                HideRollDie(i);
        }

        public void ShowRollDie(int i)
        {
            PictureBox die = GetRollDie(i);
            die.Image = Image.FromFile(System.Environment.CurrentDirectory + "\\..\\..\\Dice\\die" + rollDice[i] + ".png");
            die.Visible = true;
            die.Enabled = true;
        }

        public void ShowAllRollDie()
        {
            for (int i = 0; i < DICE_COUNT; i++)
                if (rollDice[i] > 0)
                    ShowRollDie(i);
        }
        #endregion

        #region Event Handlers
        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < DICE_COUNT; i++)    // Initialize the lists of dice to non-values
            {
                rollDice.Add(-1);
                keepDice.Add(-1);
            }
            ResetUserUIScoreCard();
            HideAllRollDice();
            HideAllKeepDice();
            HideAllComputerKeepDice();
        }

        private void rollButton_Click(object sender, EventArgs e)       // Rolls dice equal 5 minus the amount of dice in the keep section, disables itself after 3 rolls
        {
                HideAllKeepDice();
                CollapseDice(keepDice);     // Collapse the keep dice to the left/lower indexes
                ShowAllKeepDie();

            if (GetFirstAvailablePB(keepDice) >= 0)         // If the user has dice to roll...
            {
                HideAllRollDice();
                CollapseDice(rollDice);     // Collapse the roll dice to the left/lower indexes
                Roll(DICE_COUNT - GetFirstAvailablePB(keepDice), rollDice);
                ShowAllRollDie();

                rollCount++;
                if (rollCount > 2)
                    rollButton.Enabled = false;
            }
            else                                            // Else if the user has no dice to roll...
                MessageBox.Show("You must move dice from the keep zone onto the board to roll them.");
        }

        private void userScorecard_DoubleClick(object sender, EventArgs e)      // When the user clicks on a section in the scorecard...
        {
            if (rollCount > 0)
            {
                HideAllRollDice();          // Prepare dice to be scored...
                HideAllKeepDice();
                CollapseDice(rollDice);
                CollapseDice(keepDice);
                if (GetFirstAvailablePB(keepDice) >= 0)
                    MoveRollDiceToKeep(rollDice, keepDice);

                Label scoreCardElement = (Label)sender;                     // Find out where the user clicked
                int i = int.Parse(scoreCardElement.Name.Substring(4));

                scores[i] = Score(i, keepDice);                             // Update the scorebox the user clicked
                scoreCardElement.Text = scores[i].ToString();
                scoreCardElement.Enabled = false;

                for (int k = 0; k < DICE_COUNT; k++)                // Reset keepDice
                    keepDice[k] = -1;
                rollCount = 0;                                      // Reset rollCounter and allow them to roll again
                rollButton.Enabled = true;

                uScoreCardCount++;
                if (uScoreCardCount == SCORE_CARD)                   // Check if the user has filled out his score card...
                {
                    UpdateUserUIScoreCard();                        // Calculate the total score and check if the user satisfied the bonus
                    rollButton.Enabled = false;                     // Disable rerolling until a new game starts
                    string mess = "Your final score is " + userTotalScore.Text + ".";       // End of game message
                    MessageBox.Show(mess);
                }
            }
        }

        private void roll_DoubleClick(object sender, EventArgs e)
        {
            PictureBox dice = (PictureBox)sender;           // Check what dice the user clicked on

            int index2 = int.Parse(dice.Name.Substring(4)); // Extrapolate the index
            int index = GetFirstAvailablePB(keepDice);      // Get the index of the first empty spot in the keep zone

            keepDice[index] = rollDice[index2];             // Remove the played dice from the board and place it into the keep zone
            ShowKeepDie(index);
            HideRollDie(index2);
            rollDice[index2] = -1;
        }

        private void keep_DoubleClick(object sender, EventArgs e)
        {
            PictureBox dice = (PictureBox)sender;           // Check what dice the user clicked on

            int index2 = int.Parse(dice.Name.Substring(4)); // Extrapolate the index
            int index = GetFirstAvailablePB(rollDice);      // Get the index of the first empty spot on the board

            rollDice[index] = keepDice[index2];             // Remove the played dice from the keep zone and place it onto the board
            ShowRollDie(index);
            HideKeepDie(index2);
            keepDice[index2] = -1;
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < DICE_COUNT; i++)        // Reset the dice values
                rollDice[i] = -1;
            for (int i = 0; i < DICE_COUNT; i++)
                keepDice[i] = -1;

            rollButton.Enabled = true;                  // Allow them to roll again
            ResetUserUIScoreCard();
            HideAllRollDice();
            HideAllKeepDice();
            HideAllComputerKeepDice();
        }
        #endregion
    }
}
