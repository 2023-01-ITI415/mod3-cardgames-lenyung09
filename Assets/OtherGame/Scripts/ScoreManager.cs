using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OtherGame
{
    // An enum to handle all the possible scoring events
    public enum eScoreEvent
    {
        draw,
        mine,
        gameWin,
        gameLoss,
    }

    // ScoreManager handles all of the scoring
    public class ScoreManager : MonoBehaviour
    {
        // a
        static private ScoreManager S;

        // b

        static public int SCORE_FROM_PREV_ROUND = 0;
        static public int SCORE_THIS_ROUND = 0;
        static public int HIGH_SCORE = 0;

        [Header("Inscribed")]
        public float floatDuration = 0.75f;
        public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
        public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
        public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
        public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);

        [Tooltip("If true, then score events are logged to the Console.")]
        public bool logScoreEvents = true;

        [Header("Dynamic")]
        // Fields to track score info
        public int chain = 0;
        public int scoreRun = 0;
        public int score = 0;

        public static int multiplier = 1;

        [Header("Check this box to reset the ProspectorHighScore to 100")] // c
        public bool checkToResetHighScore = false;

        void Awake()
        {
            if (S != null)
                Debug.LogError("ScoreManager.S is already set!"); // d
            S = this; // Set the private singleton

            // Check for a high score in PlayerPrefs
            if (PlayerPrefs.HasKey("ProspectorHighScore"))
            {
                HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
            }
            // Add the score from last round, which will not be 0 if it was a win
            score += SCORE_FROM_PREV_ROUND;
            // And reset SCORE_THIS_ROUND to 0 (it is not used until GameOver(win))
            SCORE_THIS_ROUND = 0;
        }

        /// <summary>
        /// This static method enables any other class to post an eScoreEvent.
        /// </summary>
        static public void TALLY(eScoreEvent evt)
        {
            // e
            S.Tally(evt);
        }

        /// <summary>
        /// Handle eScoreEvents (mostly sent by the Prospector class).
        /// </summary>
        void Tally(eScoreEvent evt)
        {
            switch (evt)
            {
                // When a mine card is clicked...
                case eScoreEvent.mine: // Remove a mine card
                    chain++; // increase the score chain
                    scoreRun += chain; // add score for this card to run
                    break;

                // These same things need to happen whether it’s a draw, win, or loss
                case eScoreEvent.draw: // Drawing a card

                    chain = 0; // resets the score chain
                    score += scoreRun; // * multiplier; // add scoreRun to total score

                    scoreRun = 0; // reset scoreRun

                    break; // f
                case eScoreEvent.gameWin: // Won the round
                // f
                case eScoreEvent.gameLoss: // Lost the round
                    chain = 0; // resets the score chain
                    // add scoreRun to total score
                    scoreRun = 0; // reset scoreRun

                    break;
            }
            //score += scoreRun;
            string scoreStr = score.ToString("#,##0"); // The 0 is a zero // g
            // This second switch statement handles round wins and losses
            switch (evt)
            {
                case eScoreEvent.gameWin:
                    // SCORE_THIS_ROUND is used here and in UITextManager
                    SCORE_THIS_ROUND = score - SCORE_FROM_PREV_ROUND;
                    // h
                    // The next line shows a new syntax forstring interpolation
                    Log($"You won this round! Round score:{SCORE_THIS_ROUND}");
                    // i

                    // If it’s a win, add the score to the next round
                    // static fields are NOT reset by SceneManager.LoadScene()
                    SCORE_FROM_PREV_ROUND = score;

                    // If it’s higher than the HIGH_SCORE, updateHIGH_SCORE
                    if (HIGH_SCORE <= score)
                    {
                        Log($"Game Win. Your new high score was:{scoreStr}");
                        HIGH_SCORE = score;
                        PlayerPrefs.SetInt("ProspectorHighScore", score);
                    }
                    break;

                case eScoreEvent.gameLoss:
                    // If it’s a loss, check against the highscore
                    if (HIGH_SCORE <= score)
                    {
                        Log($"Game Over. Your new high score was:{scoreStr}");
                        HIGH_SCORE = score;
                        PlayerPrefs.SetInt("ProspectorHighScore", score);
                    }
                    else
                    {
                        Log($"Game Over. Your final score was:{scoreStr}");
                    }
                    // Reset SCORE_FROM_PREV_ROUND to 0 for a newgame
                    SCORE_FROM_PREV_ROUND = 0;
                    break;

                default:
                    Log($"score:{scoreStr} scoreRun:{scoreRun} chain:{chain}");
                    break;
            }
        }

        /// <summary>
        /// The Log method allows us to only send logs to theconsole if the bool
        /// logScoreEvents is checked in the Inspector.
        /// </summary>
        /// <param name="str">The string to be sent to theConsole.</param>
        void Log(string str)
        {
            // i
            if (logScoreEvents)
                Debug.Log(str);
        }

        /// <summary>
        /// If the public bool checkToResetHighScore is checkedin the Editor, then
        /// it will be set to false, and ProspectorHighScore inlayerPrefs will be
        /// reset to 100. I use this OnDrawGizmos() trickfrequently to manage
        /// PlayerPrefs resets like this.
        /// </summary>
        void OnDrawGizmos()
        {
            // c
            if (checkToResetHighScore)
            {
                checkToResetHighScore = false;
                PlayerPrefs.SetInt("ProspectorHighScore", 100);
                Debug.LogWarning("PlayerPrefs.ProspectorHighScore reset to 100!");
            }
        }

        // These static properties allow other classes to getScoreManager’s state
        static public int CHAIN
        {
            get { return S.chain; }
        } //

        static public int SCORE
        {
            get { return S.score; }
        }
        static public int SCORE_RUN
        {
            get { return S.scoreRun; }
        }

        private Transform canvasTrans;

        void Start()
        {
            ScoreBoard.SCORE = SCORE; // Show the score on theScoreBoard
            canvasTrans = GameObject.Find("Canvas").transform;
        }
    }
}
