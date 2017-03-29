using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text;
using System.Xml;
using System.IO;
using TriviaQuizGame.Types;

namespace TriviaQuizGame
{
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over.
	/// </summary>
	public class TQGGameController:MonoBehaviour 
	{
        //Rashmi code | Start
        public GameObject startBackgrnd;
        public GameObject gameBackgrnd;
        public GameObject gameOverBackgrnd;
        public GameObject victoryBackgrnd;

        string currentCatName;
        //Rashmi code | Stop

		// Holds the current event system
		internal EventSystem eventSystem;
		
		[Header("<Player Options>")]
		[Tooltip("A list of the players in the game. Each player can be assigned a name, a score text, lives and lives bar. You must have at least one player in the list in order to play the game. You don't need to assign all fields. For example, a player may have a name with no lives bar and it will work fine.")]
		public Player[] players;

		//The current turn of the player. 0 means it's player 1's turn to play, 1 means it's player 2's turn, etc
		internal int currentPlayer = 0;
		
		// Is this game played in hot-seat mode? This mode lets each player answer a question in turn.
		internal bool playInTurns = true;
		
		[Tooltip("The number of lives each player has. You lose a life if time runs out, or you answer wrongly too many times")]
		public float lives = 3;
		
		// The width of a single life in the lives bar. This is calculated from the total width of a life bar divided by the number of lives
		internal float livesBarWidth = 128;
		
		[Tooltip("The number of players participating in the match. This number cannot be larger than the total number of players list ( The ones that you assign a scoreText/livesBar to )")]
		public int numberOfPlayers = 4;

		internal RectTransform playersObject;

		[Header("<Level Options>")]

		[Tooltip("The object that displays the current question")]
		public Transform questionObject;

		// The image and video objects inside the question object
		internal RectTransform imageObject;
		internal GameObject videoObject;

		[Tooltip("The objects that display the possible answers")]
		public Transform[] answerObjects;

		[Tooltip("Randomize the display order of answers when a new question is presented")]
		public bool randomizeAnswers = true;
		
		// The currently selected answer when using the ButtonSelector
		internal int currentAnswer = 0;
		
		// Holds the answers when shuffling them
		internal string[] tempAnswers;
		
		[Tooltip("A list of all possible questions in the game. Each question has a number of correct/wrong answers, a followup text, a bonus value, time, and can also have an image/video as the background of the question")]
		public Question[] questions;
		static Question[] questionsFromWeb = new Question[0];

		[Tooltip("The number of the first question being asked. You can change this to start from a higher question number")]
		public int firstQuestion = 1;
		
		// The index of the current question being asked. -1 is the index of the first question, 0 the index of the second, and so on
		internal int currentQuestion = -1;
		
		// Is a question being asked right now?
		internal bool askingQuestion;

		[Tooltip("Randomize the list of questions. Use this if you don't want the questions to appear in the same order every time you play. Combine this with 'sortQuestions' if you want the questions to be randomized within the bonus groups.")]
		public bool randomizeQuestions = true;

		[Tooltip("Sort the list questions from lowest bonus to highest bonus and put them into groups. Use this if you want the questions to be displayed from the easiest to the hardest ( The difficulty of a question is decided by the bonus value you give to it )")]
		public bool sortQuestions = true;

		[Tooltip("How many questions from the current bonus group should be asked before moving on to the next group. If we dont sort the questions by bonus groups, then this value is ignored. If there are several players in the game, the value of Questions Per Group will be multiplied by the number of players, so that each one can have a chance to answer a question from the same group before moving on to the next group")]
		public int questionsPerGroup = 2;
		internal int defaultQuestionsPerGroup;
		internal int questionCount = 0;

		// The progress object which shows all the progress tabs and how many questions are left to win
		internal RectTransform progressObject;

		// The progress tab and text objects which show which question we are on. The tab also shows if we answered a question correctly or not
		internal GameObject progressTabObject;
		internal GameObject progressTextObject;

		// The size of the progress tab. This is calculated automatically and used to align the progress bar to the center.
		internal float progressTabSize;

		[Tooltip("Limit the total number of questions asked, regardless of whether we answered correctly or not. Use this if you want to have a strict number of questions asked in the game (ex: 10 questions). If you keep it at 0 the number of questions will not be limited and you will go through all the question groups in the quiz before finishing it")]
		public int questionLimit = 0;

		// The total number of questions we asked. This is used to check if we reached the question limit.
		internal int questionLimitCount = 0;

		// The number of questions we answered correctly. This is used for displaying the result at the end of the game
		internal int correctAnswers = 0;

		[Tooltip("The maximum number of mistakes allowed. If you make to many mistakes you lose a life and move to the next question")]
		public int maximumMistakes = 2;
		internal int mistakeCount = 0;
		
		// How many seconds are left before game over
		internal float timeLeft = 10;
		
		// Is the timer running?
		internal bool timerRunning = false;
		
		// The bonus we currently have
		internal float bonus;

		[Tooltip("How much (percentage) do we lose from the bonus when we make a mistake")]
		public float bonusLoss = 0.5f;
		
		// The highscore recorded for a level ( used in single player only )
		internal float highScore = 0;

		[Tooltip("If we are using an image for a closeup game mode, how much should we be zoomed in?")]
		public float imageCloseupZoom = 0;

		[Header("<XML Options>")]
		
		[Tooltip("The address of the Xml you want to load from the web. This is used if you want to load a set of questions while the game is running online in a browser")]
		public string xmlWebAddress;
		
		[Tooltip("How many seconds to wait for the Xml to be loaded before giving up and showing the error message")]
		public float xmlLoadTimeout = 3;
		
		[Tooltip("The message we show when the Xml could not be loaded from the online address")]
		public Transform xmlLoadErrorCanvas;

		// This is used when parsing the Xml info
		internal XmlNodeList xmlRecords;
		
		[Header("<User Interface Options>")]
		[Tooltip("The bonus object that displays how much we can win if we answer correctly")]
		public Transform bonusObject;
		
		[Tooltip("The menu that appears at the start of the game. This is used in the hotseat mode where we show a menu asking how many players want to participate")]
		public Transform startCanvas;

		//The canvas of the timer in the game
		internal GameObject timerIcon;
		internal Image timerBar;
		internal Text timerText;

		[Tooltip("The menu that appears if we lose all lives in a single player game")]
		public Transform gameOverCanvas;
		
		[Tooltip("The menu that appears after finishing all the questions in the game. Used for single player and hotseat")]
		public Transform victoryCanvas;
		
		[Tooltip("The canvas that holds the larget image when we click on an image question")]
		public Transform largerImageCanvas;
		
		// Is the game over?
		internal bool  isGameOver = false;
		
		[Tooltip("The level of the main menu that can be loaded after the game ends")]
		public string mainMenuLevelName = "CS_StartMenu";
		
		[Header("<Animation & Sounds>")]
		[Tooltip("The animation that plays when showing an answer")]
		public AnimationClip animationShow;
		
		[Tooltip("The animation that plays when hiding an answer")]
		public AnimationClip animationHide;
		
		[Tooltip("The animation that plays when choosing the correct answer")]
		public AnimationClip animationCorrect;
		
		[Tooltip("The animation that plays when choosing the wrong answer")]
		public AnimationClip animationWrong;
		
		[Tooltip("The animation that plays when showing a new question")]
		public AnimationClip animationQuestion;
		
		[Tooltip("Various sounds and their source")]
		public AudioClip soundQuestion;
		public AudioClip soundCorrect;
		public AudioClip soundWrong;
		public AudioClip soundTimeUp;
		public AudioClip soundGameOver;
		public AudioClip soundVictory;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;
		
		internal bool isPaused = false;
		
		// A general use index
		internal int index = 0;
		internal int indexB = 0;
		
		internal bool keyboardControls = false;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
            //Rashmi code        
            //Image backgroundImg = background.GetComponent<Image>();
            //backgroundImg.sprite = Resources.Load<Sprite>("Cat");   
            
            // Disable multitouch so that we don't tap two answers at the same time ( prevents multi-answer cheating, thanks to Miguel Paolino for catching this bug )
            Input.multiTouchEnabled = false;
			
			// Cache the current event system so we can enable disable it between questions
			eventSystem = UnityEngine.EventSystems.EventSystem.current;
			
			//Hide the game over ,victory ,and larger image screens
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( victoryCanvas )    victoryCanvas.gameObject.SetActive(false);
			if ( largerImageCanvas )    largerImageCanvas.gameObject.SetActive(false);
			if ( xmlLoadErrorCanvas )    xmlLoadErrorCanvas.gameObject.SetActive(false);

			//Get the highscore for the player
			#if UNITY_5_3
			highScore = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "HighScore", 0);
			#else
			highScore = PlayerPrefs.GetFloat(Application.loadedLevelName + "HighScore", 0);
			#endif

			//Assign the timer icon and text for quicker access
			if ( GameObject.Find("TimerIcon") )
			{
				timerIcon = GameObject.Find("TimerIcon");
				if ( GameObject.Find("TimerIcon/Bar") )    timerBar = GameObject.Find("TimerIcon/Bar").GetComponent<Image>();
				if ( GameObject.Find("TimerIcon/Text") )    timerText = GameObject.Find("TimerIcon/Text").GetComponent<Text>();
			}

			//Assign the players object for quicker access to player names, scores, and lives
			if ( GameObject.Find("PlayersObject") )
			{
				playersObject = GameObject.Find("PlayersObject").GetComponent<RectTransform>();
			}

			// Record the default number of questions per group, so that when we change the number of players we can update this value correctly
			defaultQuestionsPerGroup = questionsPerGroup;
			
			// Update the current list of players based on numberOfPlayers
			SetNumberOfPlayers(numberOfPlayers);

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
			
			// Clear the bonus object text
			if ( bonusObject )    bonusObject.Find("Text").GetComponent<Text>().text = "";
			
			// Clear the question text
			questionObject.Find("Text").GetComponent<Text>().text = "";

			// Hide the CONTINUE button so we can move on to the next question
			if ( questionObject.Find("ButtonContinue") )    questionObject.Find("ButtonContinue").gameObject.SetActive(false);
			
			// Assign the image and video objects from inside the question object
			if ( questionObject.Find("Image") )    imageObject = questionObject.Find("Image").GetComponent<RectTransform>();
			if ( questionObject.Find("Video") )    videoObject = questionObject.Find("Video").gameObject;

			// Clear the question image and video, if they exist
			if ( imageObject )    imageObject.gameObject.SetActive(false);
			if ( videoObject )    videoObject.SetActive(false);

			// Disable the button from the question, so that we don't accidentally try to open an image that isn't there
			questionObject.GetComponent<Button>().enabled = false;
			
			// Clear all the answers text
			foreach ( Transform answerObject in answerObjects )
			{
				// Clear the answer text
				answerObject.Find("Text").GetComponent<Text>().text = "";
				
				// Deactivate the answer object
				answerObject.gameObject.SetActive(false);
			}

			// If we have a start canvas, pause the game and display it. Otherwise, just start the game.
			if ( startCanvas )    
			{
				// Show the start screen
				startCanvas.gameObject.SetActive(true);
			}
			else
			{
				// Start the game! Setup the question list ( shuffle it ) and ask the first question
				StartCoroutine(StartGame(null));
			}
		}

		/// <summary>
		/// Prepares the question list and starts the game. Also loads the XML questions from an online address if it exists.
		/// </summary>
		public IEnumerator StartGame(string catName)
		{
            Debug.Log("StartGame() called from scene " + SceneManager.GetActiveScene().name);
			isGameOver = false;

            //Rashmi Code | Start |  

            //Storing category name in a global variable
            currentCatName = catName;

            //check category name null or empty
            if ((!(catName.Length < 0)) && (catName.ToString() != null))
            {
                currentCatName = catName;
                if (catName == "Ice")
                {
                    //Condition satisfied - enters this block
                    Debug.Log(catName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");
                }
                else if (catName == "Ocean")
                {
                    Debug.Log(catName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("clownfish1-1");
                }
                else if (catName == "Rainforest")
                {
                    Debug.Log(catName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Tucan_Happy");

                }
                else if (catName == "Mountain")
                {
                    Debug.Log(catName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Pika_Happy");
                }
            }
            //Rashmi code | Stop | 

            // The index of the first question in the question list is actually -1, so we adjust the number from the component ( 1 becomes -1, 2 becomes 0, 10 becomes 8, etc )
            currentQuestion = firstQuestion - 2;

			// Reset the question counter
			questionCount = 0;

			// Make sure the question limit isn't larger than the actual number of questions available
			questionLimit = Mathf.Clamp( questionLimit, 0, questions.Length);

			// Assign the progress object and all related objects for easier access
			if ( GameObject.Find("ProgressObject") )    
			{
				progressObject = GameObject.Find("ProgressObject").GetComponent<RectTransform>();
				
				// If we have a tab object in the progress object, assign it and duplicate the tabs based on the total number of questions we are limited to
				if ( GameObject.Find("ProgressObject/Tab") )
				{
					// Assign the tab object
					progressTabObject = GameObject.Find("ProgressObject/Tab");
					
					for ( index = 0 ; index < questionLimit ; index++ )
					{
						// Duplicate the tab object
						Transform newTab = Instantiate(progressTabObject.transform) as Transform;
						
						// Put it inside the progress object
						newTab.SetParent(progressObject.transform);
						
						// Reset the scale and position of the tab so it fits the progress object
						newTab.localScale = Vector3.one;
						newTab.localPosition = Vector3.one;
						
						// Set the number of the tab in text
						newTab.Find("Text").GetComponent<Text>().text = (index+1).ToString();
					}
					
					// Calculate the size of a single tab. This is used to align the progress bar to the center.
					progressTabSize = progressObject.GetComponent<GridLayoutGroup>().cellSize.x;
					
					// Deactivate the original tab object
					progressTabObject.SetActive(false);
				}

				// If we have a text object in the progress object, assign it and set the number of the first question
				if ( GameObject.Find("ProgressObject/Text") )
				{
					// Assign the text object
					progressTextObject = GameObject.Find("ProgressObject/Text");
					
					// Reset the question limit counter
					questionLimitCount = 0;
					
					// Update the question count in the text
					if ( progressTextObject )    progressTextObject.GetComponent<Text>().text = questionLimitCount.ToString() + "/" + questionLimit.ToString();
				}
			}

			// If we have a	web address for the Xml, load it from the site. This is not executed in the unity editor, but only in the actual build
			if ( xmlWebAddress != String.Empty )
			{
				// If we already have questions from web for this quiz, don't load them again. Otherwise, load them from the web address.
				if ( questionsFromWeb.Length > 0 )   questions = questionsFromWeb;
				else
				{
					// Get the web address
					WWW webAddress = new WWW(xmlWebAddress);
					
					// Wait until it has been loaded
					yield return webAddress;
					
					// Print the error to the console
					if ( !String.IsNullOrEmpty(webAddress.error) )    
					{
						// Display the actual error in the console
						Debug.Log(webAddress.error);
						
						//Show the xml load error screen, and display the relevant error
						if ( xmlLoadErrorCanvas )    
						{
							xmlLoadErrorCanvas.gameObject.SetActive(true);
							
							// If we have a "404" or "Failed" error, it means the address of the XML file is wrong. If we have a "Host" error, it means we have no internet connection. Otherwise, display the full error text
							if ( webAddress.error.Contains("404") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found (404)";
							else if ( webAddress.error.Contains("Host Not Found") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Internet connection problem";
							else if ( webAddress.error.Contains("Failed") )    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += "Question list can't be found";
							else    xmlLoadErrorCanvas.Find("Text").GetComponent<Text>().text += webAddress.error;
						}
					}

					// Load the Xml info from the web file
					LoadXml(webAddress.text);
				}
			}
			
			// Wait for the next frame. This line was added because the function needs to return something or it won't work.
			yield return new WaitForFixedUpdate();
			
			// Set the list of questions for this match
			SetQuestionList();		

			// Ask the first question
			StartCoroutine(AskQuestion(false));
		}
		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void  Update()
		{
			// Move the progress object so that the current question is centered in the screen
			if ( progressObject && progressTabObject )    progressObject.anchoredPosition = new Vector2( Mathf.Lerp( progressObject.anchoredPosition.x, progressTabSize * (0.5f - questionLimitCount), Time.deltaTime * 10), progressObject.anchoredPosition.y);

			if ( currentPlayer < players.Length )
			{
				// Move the players object so that the current player is centered in the screen
				if ( players[currentPlayer].nameText && bonusObject.position.x != players[currentPlayer].nameText.transform.position.x )
				{
					playersObject.anchoredPosition = new Vector2( Mathf.Lerp( playersObject.anchoredPosition.x, currentPlayer * -200 - 100, Time.deltaTime * 10), playersObject.anchoredPosition.y);
				}

				// Make the score count up to its current value, for the current player
				if ( players[currentPlayer].score < players[currentPlayer].scoreCount )
				{
					// Count up to the courrent value
					players[currentPlayer].score = Mathf.Lerp( players[currentPlayer].score, players[currentPlayer].scoreCount, Time.deltaTime * 10);
					
					// Round up the score value
					players[currentPlayer].score = Mathf.CeilToInt(players[currentPlayer].score);
					
					// Update the score text
					UpdateScore();
				}
				
				// Update the lives bar
				if ( players[currentPlayer].livesBar )    players[currentPlayer].livesBar.rectTransform.sizeDelta = Vector2.Lerp( players[currentPlayer].livesBar.rectTransform.sizeDelta, new Vector2( players[currentPlayer].lives * livesBarWidth, players[currentPlayer].livesBar.rectTransform.sizeDelta.y), Time.deltaTime * 8);
			}

			if ( isGameOver == false )
			{
				// If we use the keyboard or gamepad, keyboardControls take effect
				if ( keyboardControls == false && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) )    
				{
					keyboardControls = true;
					
					// If no answer is selected, select the first answer
					if ( askingQuestion == true && eventSystem.firstSelectedGameObject == null )    eventSystem.SetSelectedGameObject(answerObjects[0].gameObject);
				}
				
				// If we move the mouse in any direction or click it, or touch the screen on a mobile device, then keyboard/gamepad controls are lost
				if ( Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 || Input.GetMouseButtonDown(0) || Input.touchCount > 0 )    keyboardControls = false;
				
				// Count down the time until game over
				if ( timeLeft > 0 && timerRunning == true )
				{
					// Count down the time
					timeLeft -= Time.deltaTime;
				}
				
				// Update the timer
				UpdateTime();
			}
		}

		/// <summary>
		/// Sets the question list, first shuffling them, then sorting them by bonus value, 
		/// and finally choosing a limited number of questions from each bonus group
		/// </summary>
		void SetQuestionList()
		{
			// Shuffle all the available questions
			if ( randomizeQuestions == true )   questions = ShuffleQuestions(questions);

			// Sort the questions into groups by the bonus they give, from lowest to highest
			if ( sortQuestions == true )   
			{
				Array.Sort(questions,delegate(Question x, Question y) { return x.bonus.CompareTo(y.bonus); });
			}
			else
			{
				// If we are not sorting by bonus groups, then there is no need to limit the number of questions per group
				questionsPerGroup = questions.Length;
			}
		}
		
		/// <summary>
		/// Shuffles the specified questions list, and returns it
		/// </summary>
		/// <param name="questions">A list of questions</param>
		Question[] ShuffleQuestions( Question[] questions )
		{
			// Go through all the questions and shuffle them
			for ( index = 0 ; index < questions.Length ; index++ )
			{
				// Hold the question in a temporary variable
				Question tempQuestion = questions[index];
				
				// Choose a random index from the question list
				int randomIndex = UnityEngine.Random.Range( index, questions.Length);
				
				// Assign a random question from the list
				questions[index] = questions[randomIndex];
				
				// Assign the temporary question to the random question we chose
				questions[randomIndex] = tempQuestion;
			}
			
			return questions;
		}
		
		/// <summary>
		/// Shuffles the specified answers list, and returns it
		/// </summary>
		/// <param name="answers">A list of answers</param>
		Answer[] ShuffleAnswers( Answer[] answers )
		{
			// Go through all the answers and shuffle them
			for ( index = 0 ; index < answers.Length ; index++ )
			{
				// Hold the question in a temporary variable
				Answer tempAnswer = answers[index];
				
				// Choose a random index from the question list
				int randomIndex = UnityEngine.Random.Range( index, answers.Length);
				
				// Assign a random question from the list
				answers[index] = answers[randomIndex];
				
				// Assign the temporary question to the random question we chose
				answers[randomIndex] = tempAnswer;
			}
			
			return answers;
		}
		
		
		
		/// <summary>
		/// Presents a question from the list, along with possible answers.
		/// </summary>
		IEnumerator AskQuestion( bool animateQuestion )
		{
			if ( isGameOver == false )
			{
				// We are now asking a question
				askingQuestion = true;
				
				// If we asked enough questions, move on to the next bonus group
				if ( questionCount >= questionsPerGroup )
				{
					// Holding the current bonus to compare to the next bonus
					float tempBonus = questions[currentQuestion].bonus;
					
					// Move through the questions until you find a question with a higher bonus
					while ( tempBonus >= questions[currentQuestion].bonus )
					{
						// Go to the next question
						currentQuestion++;
						
						// If we ran out of questions, stop checking
						if ( currentQuestion >= questions.Length )    break;
					}
					
					// Animate the question
					if ( animationQuestion )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( questionObject.GetComponent<Animation>().GetClip(animationQuestion.name) == null )    questionObject.GetComponent<Animation>().AddClip( animationQuestion, animationQuestion.name);
						
						// Play the animation
						questionObject.GetComponent<Animation>().Play(animationQuestion.name);
						
						// Wait for half the animation time, then display the next question. This will make the question appear while the question tab flips. Just a nice effect
						yield return new WaitForSeconds(questionObject.GetComponent<Animation>().clip.length * 0.5f);
					}
					
					// Reset the question counter
					questionCount = 0;
				}
				else  
				{
					// Go to the next question
					currentQuestion++;
					
					// Animate the question
					if ( animationQuestion )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( questionObject.GetComponent<Animation>().GetClip(animationQuestion.name) == null )    questionObject.GetComponent<Animation>().AddClip( animationQuestion, animationQuestion.name);
						
						// Play the animation
						questionObject.GetComponent<Animation>().Play(animationQuestion.name);
						
						// Wait for half the animation time, then display the next question. This will make the question appear while the question tab flips. Just a nice effect
						yield return new WaitForSeconds(questionObject.GetComponent<Animation>().clip.length * 0.5f);
					}
				}

				// If we still have questions in the list, ask the next question
				if ( currentQuestion < questions.Length )
				{
					// Display the current question
					questionObject.Find("Text").GetComponent<Text>().text = questions[currentQuestion].question;

					if ( questions[currentQuestion].image )    // If we have a question image, display it. Otherwise, hide the image object
					{
						// Hide the video object
						if ( videoObject )    videoObject.gameObject.SetActive(false);

						// Unhide the image object
						if ( imageObject )
						{
							imageObject.gameObject.SetActive(true);
						
							// Display the image for the current question
							imageObject.GetComponent<Image>().sprite = questions[currentQuestion].image;

							// If we have a closeup of the current image, zoom in to conceal it
							if ( imageCloseupZoom > 0 )
							{
								imageObject.offsetMin = Vector2.one * -imageCloseupZoom;
								imageObject.offsetMax = Vector2.one * imageCloseupZoom;
							}
						}

						// Enable the button from the question, so that we can click it to open a larger image
						questionObject.GetComponent<Button>().enabled = true;
					}
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_BLACKBERRY && !UNITY_WP8
					else if ( questions[currentQuestion].video )    // If we have a question video, display it. Otherwise, hide the video object
					{
						// Hide the image object
						if ( imageObject )    imageObject.gameObject.SetActive(false);
						
						// Disable the button from the question, so that we don't accidentally try to open an image that isn't there
						questionObject.GetComponent<Button>().enabled = true;
						
						if ( videoObject )    
						{
							// Unhide the video object
							videoObject.gameObject.SetActive(true);

							// Get the object that we will display the video on
							RawImage rawImage = videoObject.GetComponent<RawImage>();
							
							// Assign the video that we want to show
							rawImage.texture = questions[currentQuestion].video;
							
							// Set the video to loop forever
							questions[currentQuestion].video.loop = true;
							
							// Play the video
							questions[currentQuestion].video.Play();

							// Set the audio clip of the video
							//videoObject.GetComponent<AudioSource>().clip = questions[currentQuestion].video.audioClip;

							// Play the audio clip of the video
							//videoObject.GetComponent<AudioSource>().Play();
						}
					}
#endif
					else // Show the question without an image or a video
					{
						// Hide the image object
						if ( imageObject )    imageObject.gameObject.SetActive(false);
						
						// Disable the button from the question, so that we don't accidentally try to open an image that isn't there
						questionObject.GetComponent<Button>().enabled = false;
						
						// Hide the video object
						if ( videoObject )    videoObject.gameObject.SetActive(false);
					}
					
					// Set the timer for this question
					timeLeft = questions[currentQuestion].time;
					
					// Clear all the answers
					foreach ( Transform answerObject in answerObjects )
					{
						answerObject.Find("Text").GetComponent<Text>().text = "";
					}
					
					// Shuffle the list of answers
					if ( randomizeAnswers == true )    questions[currentQuestion].answers = ShuffleAnswers(questions[currentQuestion].answers);
					
					// Display the wrong and correct answers in the answer slots
					for ( index = 0 ; index < answerObjects.Length ; index++ )
					{
						// If the answer object is inactive, activate it
						if ( answerObjects[index].gameObject.activeSelf == false )    answerObjects[index].gameObject.SetActive(true);
						
						// Play the animation Show
						if ( animationShow )    
						{
							// If the animation clip doesn't exist in the animation component, add it
							if ( answerObjects[index].GetComponent<Animation>().GetClip(animationShow.name) == null )    answerObjects[index].GetComponent<Animation>().AddClip( animationShow, animationShow.name);
							
							// Play the animation
							answerObjects[index].GetComponent<Animation>().Play(animationShow.name);
						}
						
						// Enable the button so we can press it
						answerObjects[index].GetComponent<Button>().interactable = true;
						
						// Display the text of the answer
						if ( index < questions[currentQuestion].answers.Length )    answerObjects[index].Find("Text").GetComponent<Text>().text = questions[currentQuestion].answers[index].answer;
						else    answerObjects[index].gameObject.SetActive(false);
					}
					
					// If we started a new bonus group, reset the question counter
					if ( bonus > questions[currentQuestion].bonus )    questionCount = 0;
					
					// Set the bonus we can get for this question 
					bonus = questions[currentQuestion].bonus;
					
					if ( bonusObject && bonusObject.GetComponent<Animation>() )    
					{
						// Animate the bonus object
						bonusObject.GetComponent<Animation>().Play();
						
						// Reset the bonus animation
						bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = -1;
						
						// Display the bonus text
						bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();
					}
					
					// Start the timer
					timerRunning = true;
					
					// If keyboard controls are on, highlight the first answer. Otherwise, deselect all answers
					if ( keyboardControls == true )
                        eventSystem.SetSelectedGameObject(answerObjects[0].gameObject);
					else
                        eventSystem.SetSelectedGameObject(null);
					
					//If there is a source and a sound, play it from the source
					if ( soundSource && soundQuestion )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundQuestion);
				}
				else // If we have no more questions in the list, win the game
				{
					//Disable the question object
					//questionObject.gameObject.SetActive(false);

					//If we have no more questions, we win the game!
					StartCoroutine(Victory(0)); 
				}

				// If we have a question limit, count towards it to win
				if ( questionLimit > 0 )
				{
					questionLimitCount++;

					if ( progressTextObject )    
					{
						// Update the question count in the text
						progressTextObject.GetComponent<Text>().text = questionLimitCount.ToString() + "/" + questionLimit.ToString();
					}

					// If we reach the question limit, win the game
					if ( questionLimitCount > questionLimit )    StartCoroutine(Victory(0));
				}
			}
		}
		
		/// <summary>
		/// Chooses an answer from the list by index
		/// </summary>
		/// <param name="answerIndex">The number of the answer we chose</param>
		public void ChooseAnswer( int answerIndex )
		{
            //Rashmi code | Start

            //Debug.Log(currentCatName);

            //check category name null or empty
            if ((!(currentCatName.Length < 0)) && (currentCatName.ToString() != null))
            {
                //currentCatName = catName;
                if (currentCatName == "Ice")
                {
                    //Condition satisfied - enters this block
                    Debug.Log(currentCatName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");
                }
                else if (currentCatName == "Ocean")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("clownfish1-1");
                }
                else if (currentCatName == "Rainforest")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Tucan_Happy");

                }
                else if (currentCatName == "Mountain")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Pika_Happy");
                }
            }

            //Rashmi code | Stop

            // We can only choose an answer if a question is being asked now
            if ( askingQuestion == true )
			{
				// If the chosen answer is wrong, disable it and reduce the bonus for this question
				//if ( answerObjects[answerIndex].Find("Text").GetComponent<Text>().text != questions[currentQuestion].correctAnswer )
				if ( questions[currentQuestion].answers[answerIndex].isCorrect == false )
				{
					// Play the animation Wrong
					if ( animationWrong )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationWrong.name) == null )    answerObjects[answerIndex].GetComponent<Animation>().AddClip( animationWrong, animationWrong.name);
						
						// Play the animation
						answerObjects[answerIndex].GetComponent<Animation>().Play(animationWrong.name);
					}
					
					// Disable the button so we can't press it again
					answerObjects[answerIndex].GetComponent<Button>().interactable = false;

					// Set the color of the tab to green, if it exists
					if ( progressTabObject )    progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.red;
					
					// Cut the bonus to half its current value
					bonus *= bonusLoss;
					
					// Display the bonus text
					if ( bonusObject )    bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();
					
					// Increase the mistake count
					mistakeCount++;
					
					// If we reach the maximum number of mistakes, give no bonus and move on to the next question
					if ( mistakeCount >= maximumMistakes )
					{
						// Give no bonus
						bonus = 0;
						
						// Display the bonus text
						if ( bonusObject )    bonusObject.Find("Text").GetComponent<Text>().text = bonus.ToString();
						
						// Reduce from lives
						players[currentPlayer].lives--;
						
						// Update the lives we have left
						Updatelives();

						// Show the result of this question, which is wrong
						ShowResult(false);
					}
					
					// If we have more than one player and we are playing in turns (hotseat), go to the next player turn
					if ( playInTurns == true )
					{
						if ( currentPlayer < numberOfPlayers - 1 )    currentPlayer++;
						else    currentPlayer = 0;
					}

					//If there is a source and a sound, play it from the source
					if ( soundSource && soundWrong )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundWrong);
				}
				else // Choosing the correct answer
				{
					// If we answered correctly this round, increase the question count for this bonus group
					questionCount++;
					
					// Play the animation Correct
					if ( animationCorrect )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( answerObjects[answerIndex].GetComponent<Animation>().GetClip(animationCorrect.name) == null )    answerObjects[answerIndex].GetComponent<Animation>().AddClip( animationCorrect, animationCorrect.name);
						
						// Play the animation
						answerObjects[answerIndex].GetComponent<Animation>().Play(animationCorrect.name);
					}

					// If we have a progress object, color the question tab based on the relevant answer object
					if ( progressObject )    
					{
						// Set the color of the tab to green, if it exists
						if ( progressTabObject )    progressObject.transform.GetChild(questionLimitCount).GetComponent<Image>().color = Color.green;
					
						// Increase the count of the correct answers. This is used to show how many answers we got right at the end of the game
						correctAnswers++;
					}

					// Animate the bonus being added to the score
					if ( bonusObject && bonusObject.GetComponent<Animation>() )    
					{
						// Play the animation
						bonusObject.GetComponent<Animation>().Play();
						
						// Reset the speed of the animation
						bonusObject.GetComponent<Animation>()[bonusObject.GetComponent<Animation>().clip.name].speed = 1;
					}

					// Add the bonus to the score of the current player
					players[currentPlayer].scoreCount += bonus;

					//If there is a source and a sound, play it from the source
					if ( soundSource && soundCorrect )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundCorrect);

                    //Rashmi code | Start

                    Debug.Log(currentCatName);
                    
                    //Rashmi code | Stop



                    // Show the result of this question, which is correct
                    ShowResult(true);
				}
			}
		}

		/// <summary>
		/// Shows the result of the question, whether we answered correctly or not. Also displays a followup text and reveals a closeup image, if they exist
		/// </summary>
		/// <param name="isCorrectAnswer">We got the correct answer.</param>
		public void ShowResult( bool isCorrectAnswer )
		{
			// We are not asking a question now
			askingQuestion = false;
			
			// Stop the timer
			timerRunning = false;
			
			// Reset the mistake counter
			mistakeCount = 0;
			
			// Hide the larger image screen
			if ( largerImageCanvas )    largerImageCanvas.gameObject.SetActive(false);
			
			// Disable the button from the question, so that we don't accidentally try to open an image that isn't there
			questionObject.GetComponent<Button>().enabled = false;
			
			// If we have a closeup of the current image, zoom out to reveal it
			if ( imageCloseupZoom > 0 && imageObject )
			{
				StartCoroutine(RevealImage(1));
			}
            

            // If we have a followup to the question, display it
            if ( questions[currentQuestion].followup != null && questions[currentQuestion].followup != String.Empty )
			{
				// Display the followup to the question
				questionObject.Find("Text").GetComponent<Text>().text = questions[currentQuestion].followup;
				
				// Show the continue button so we can move on to the next question
				if ( questionObject.Find("ButtonContinue") )    questionObject.Find("ButtonContinue").gameObject.SetActive(true);
				
				// Go through all the answers and make them unclickable
				for ( index = 0 ; index < answerObjects.Length ; index++ )
				{
					// If this is the correct answer, highlight it and delay its animation
					if ( index < questions[currentQuestion].answers.Length )
					{
						if ( questions[currentQuestion].answers[index].isCorrect == true )
						{
							// Highlight the correct answer
							eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);
						}
						else
						{
							// Make all the buttons uninteractable
							answerObjects[index].GetComponent<Button>().interactable = false;
						}
					}
				}
			}
			else
			{
				// Reset the question and answers in order to display the next question
				StartCoroutine(ResetQuestion(0.5f));
			}
		}
		
		/// <summary>
		/// Resets the question and answers, in preparation for the next question
		/// </summary>
		/// <returns>The question.</returns>
		/// <param name="delay">Delay in seconds before showing the next question</param>
		IEnumerator ResetQuestion( float delay )
		{
			// Go through all the answers hide the wrong ones
			for ( index = 0 ; index < answerObjects.Length ; index++ )
			{
				// If this is a wrong answer, hide it
				//if ( answerObjects[index].Find("Text").GetComponent<Text>().text != questions[currentQuestion].correctAnswer )
				if ( index < questions[currentQuestion].answers.Length && questions[currentQuestion].answers[index].isCorrect == false )
				{
					// Play the animation Hide, after the current animation is over
					if ( animationHide )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( answerObjects[index].GetComponent<Animation>().GetClip(animationHide.name) == null )    answerObjects[index].GetComponent<Animation>().AddClip( animationHide, animationHide.name);
						
						// Play the animation queded in line after te current animation
						answerObjects[index].GetComponent<Animation>().PlayQueued(animationHide.name);
					}
				}
			}

			// Go through all the answers again and highlight the correct one
			for ( index = 0 ; index < answerObjects.Length ; index++ )
			{
				// If this is the correct answer, highlight it and delay its animation
				//if ( answerObjects[index].Find("Text").GetComponent<Text>().text == questions[currentQuestion].correctAnswer )
				if ( index < questions[currentQuestion].answers.Length && questions[currentQuestion].answers[index].isCorrect == true )
				{
					// Highlight the correct answer
					eventSystem.SetSelectedGameObject(answerObjects[index].gameObject);
					
					// For for a while
					yield return new WaitForSeconds(2.0f);
					
					// Play the animation Hide
					if ( animationHide )    
					{
						// If the animation clip doesn't exist in the animation component, add it
						if ( answerObjects[index].GetComponent<Animation>().GetClip(animationHide.name) == null )    answerObjects[index].GetComponent<Animation>().AddClip( animationHide, animationHide.name);
						
						// Play the animation
						answerObjects[index].GetComponent<Animation>().Play(animationHide.name);
					}
				}
			}
			
			// For for a while
			yield return new WaitForSeconds(delay);
			
			// If we have more than one player and we are playing in turns (hotseat), go to the next player turn
			if ( numberOfPlayers > 0 && playInTurns == true )
			{
				if ( currentPlayer < numberOfPlayers - 1 )    currentPlayer++;
				else    currentPlayer = 0;
			}
			
			// Deselect the currently selected answer
			eventSystem.SetSelectedGameObject(null);
			
			// Ask the next question
			StartCoroutine(AskQuestion(true));
		}
		
		/// <summary>
		/// Updates the timer text, and checks if time is up
		/// </summary>
		void UpdateTime()
		{
			// Update the time only if we have a timer icon canvas assigned
			if ( timerIcon )
			{
				// Update the timer circle, if we have one
				if ( timerBar )
				{
					// If the timer is running, display the fill amount left. Otherwise refill the amount back to 100%
					if ( timerRunning == true )    timerBar.fillAmount = timeLeft/questions[currentQuestion].time;
					else    timerBar.fillAmount = Mathf.Lerp( timerBar.fillAmount, 1, Time.deltaTime * 10);
				}

				// Update the timer text, if we have one
				if ( timerText )
				{
					// If the timer is running, display the timer left. Otherwise hide the text
					if ( timerRunning == true )    timerText.text = Mathf.RoundToInt(timeLeft).ToString();
					else    timerText.text = "";
				}
				
				// Time's up!
				if ( timeLeft <= 0 && timerRunning == true )    
				{
					// Reduce from lives
					players[currentPlayer].lives--;
					
					// Update the lives we have left
					Updatelives();
					
					// If we have more than one player and we are playing in turns (hotseat), go to the next player turn
					if ( playInTurns == true )
					{
						if ( currentPlayer < numberOfPlayers - 1 )    currentPlayer++;
						else    currentPlayer = 0;
					}

					// Play the timer icon animation
					if ( timerIcon.GetComponent<Animation>() )    timerIcon.GetComponent<Animation>().Play();

                    //If there is a source and a sound, play it from the source
					if ( soundSource && soundTimeUp )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundTimeUp);
					
					// Show the result of this question, which is wrong ( because we ran out of time, we lost the question )
					ShowResult(false);
				}
			}
		}
		
		/// <summary>
		/// Updates the score value and checks if we got to the next level
		/// </summary>
		void  UpdateScore()
		{
			//Update the score text
			//if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();
			
			//Update the score text for the current player
			if ( players[currentPlayer].scoreText )    players[currentPlayer].scoreText.GetComponent<Text>().text = players[currentPlayer].score.ToString();
		}
		
		/// <summary>
		/// Runs the game over event and shows the game over screen
		/// </summary>
		IEnumerator GameOver(float delay)
		{
			isGameOver = true;

            //Rashmi code | Start 

            Debug.Log(currentCatName);

            //check category name null or empty
            if ((!(currentCatName.Length < 0)) && (currentCatName.ToString() != null))
            {              
                if (currentCatName == "Ice")
                {
                    //Condition satisfied - enters this block
                    Debug.Log(currentCatName);
                    
                    Image bgImg = gameOverBackgrnd.GetComponent<Image>();
					bgImg.sprite = Resources.Load<Sprite>("Cat");
                }
                else if (currentCatName == "Ocean")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameOverBackgrnd.GetComponent<Image>();
					bgImg.sprite = Resources.Load<Sprite>("Cat");
                }
                else if (currentCatName == "Forest")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameOverBackgrnd.GetComponent<Image>();
					bgImg.sprite = Resources.Load<Sprite>("Cat");

                }
                else if (currentCatName == "Mountain")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = gameOverBackgrnd.GetComponent<Image>();
					bgImg.sprite = Resources.Load<Sprite>("Cat");
                }
            }

            //Rashmi code | Stop

            yield return new WaitForSeconds(delay);
			
			//Show the game over screen
			if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);

				//Write the score text, if it exists
				if ( gameOverCanvas.Find("ScoreTexts/TextScore") )    gameOverCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text = "SCORE " + players[currentPlayer].score.ToString();

				//Check if we got a high score
				if ( players[currentPlayer].score > highScore )    
				{
					highScore = players[currentPlayer].score;
					
					//Register the new high score
					#if UNITY_5_3
					PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", players[currentPlayer].score);
					#else
					PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", players[currentPlayer].score);
					#endif
				}
				
				//Write the high sscore text
				gameOverCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + highScore.ToString();

                //Rashmi Sharma | Start
                //GameObject.Find()
                /*
                Category myCategory = new Category();
                
                RectTransform selectedCategory = gameOverCanvas.gameObject.transform.Find("CategoryWheel/WheelHolder/Wheel/Category").GetComponent<RectTransform>();

                switch(selectedCategory.Find("Text").GetComponent<Text>().text)
                {
                    case "polar":
                        break;
                    default:
                        break;
                }
                Rashmi code | Stop
                 */


                //This will select all the MONOBEHAVIOURS attached to the prefab

                //First get all Monobehavious attached
                MonoBehaviour[] behaviours = gameOverCanvas.gameObject.GetComponents<MonoBehaviour>() as MonoBehaviour[];

                //Check if the length of the array is > 0. This is required, since a GameObject 
                //may have no scripts attached to it
                if(behaviours.Length > 0) {
                //Print all attached Monobehvaiours to console
                    foreach(MonoBehaviour behaviour in behaviours)
                        Debug.Log (behaviour);
                }

				//If there is a source and a sound, play it from the source
				if ( soundSource && soundGameOver )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);
			}
		}
		
		/// <summary>
		/// Runs the victory event and shows the victory screen
		/// </summary>
		IEnumerator Victory(float delay)
		{
			isGameOver = true;
			
			yield return new WaitForSeconds(delay);

            //Rashmi code | Start        
            Debug.Log(currentCatName);

            //check category name null or empty
            if ((!(currentCatName.Length < 0)) && (currentCatName.ToString() != null))
            {
                //currentCatName = catName;

                if (currentCatName == "Ice")
                {
                    //Condition satisfied - enters this block
                    Debug.Log(currentCatName);                    
                    Image bgImg = victoryBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");
                }
                else if (currentCatName == "Ocean")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = victoryBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");
                }
                else if (currentCatName == "Forest")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = victoryBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");

                }
                else if (currentCatName == "Mountain")
                {
                    Debug.Log(currentCatName);
                    Image bgImg = victoryBackgrnd.GetComponent<Image>();
                    bgImg.sprite = Resources.Load<Sprite>("Bear_Happy");
                }
            }

            //Rashmi code | Stop

            //Show the game over screen
            if ( victoryCanvas )    
			{
				//Show the victory screen
				victoryCanvas.gameObject.SetActive(true);

				// If we have a TextScore and TextHighScore objects, then we are using the single player victory canvas
				if ( victoryCanvas.Find("ScoreTexts/TextScore") && victoryCanvas.Find("ScoreTexts/TextHighScore") )
				{
					//Write the score text, if it exists
					victoryCanvas.Find("ScoreTexts/TextScore").GetComponent<Text>().text = "SCORE " + players[currentPlayer].score.ToString();
					
					//Check if we got a high score
					if ( players[currentPlayer].score > highScore )
					{
						highScore = players[currentPlayer].score;
						
						//Register the new high score
						#if UNITY_5_3
						PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "HighScore", players[currentPlayer].score);
						#else
						PlayerPrefs.SetFloat(Application.loadedLevelName + "HighScore", players[currentPlayer].score);
						#endif
					}
					
					//Write the high sscore text
					victoryCanvas.Find("ScoreTexts/TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + highScore.ToString();
				}
				
				// If we have a Players object, then we are using the hotseat results canvas
				if ( victoryCanvas.Find("ScoreTexts/Players") )
				{
					// Sort the players by their score and then check the winners ( could be a draw with more than one winner )
					// The number of winners, could be more than one in case of a draw
					int winnerCount = 0;
					
					// Sort the players by the score they have, from highest to lowest
					Array.Sort( players, delegate(Player x, Player y) { return y.score.CompareTo(x.score); });
					
					// Go through all the players and check if we have more than one winner
					for ( index = 0 ; index < numberOfPlayers ; index++ )
					{
						// The first player in the list is always the winner. After that we check if there are other players with the same score ( a draw between several winners )
						if ( index == 0 )    winnerCount = 1;
						else if ( players[index].score == players[0].score )
						{
							winnerCount++;
						}
					}

					// Go through all the players in the table and hide the winner icon from all the losers, or if everyone got 0 points.
					for ( index = 0 ; index < numberOfPlayers ; index++ )
					{
						if ( players[index].score <= 0 || index >= winnerCount )    victoryCanvas.Find("ScoreTexts/Players").GetChild(index).Find("WinnerIcon").gameObject.SetActive(false);
					}

					// Go through all the score texts and update them each player
					for ( index = 0 ; index < numberOfPlayers ; index++ )
					{
						// Display the name of the player
						victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().text = players[index].name;
						
						// Set the color of the player name
						victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetComponent<Text>().color = players[index].color;
						
						// Display the score of the player
						victoryCanvas.Find("ScoreTexts/Players").GetChild(index).GetChild(0).GetComponent<Text>().text = players[index].score.ToString();
					}
					
					// If the value of numberOfPlayers is lower than the actual number of players, remove any excess players from the list
					if ( numberOfPlayers < players.Length )
					{
						// Go through all the extra players in the list, and remove their name, score, and lives objects
						for ( index = numberOfPlayers ; index < players.Length ; index++ )
						{
							victoryCanvas.Find("ScoreTexts/Players").GetChild(index).gameObject.SetActive(false);
						}
					}
					
					// Display the list of winners
					// If we have one winner display a single name. Otherwise, display a "draw" message with the names of all the winners
					if ( winnerCount == 1 )
					{
						// Display the single winner
						victoryCanvas.Find("TextResult").GetComponent<Text>().text = players[0].name + " wins with " + players[0].score.ToString() + " points!";
					}
					else
					{
						// Display the "draw" message between several winners
						victoryCanvas.Find("TextResult").GetComponent<Text>().text = "It's a draw between ";
						
						// Display the names of the winners
						while ( winnerCount > 0 )
						{
							winnerCount--;
							
							// Add to the text the name of the next winner
							if ( winnerCount == 0 )    victoryCanvas.Find("TextResult").GetComponent<Text>().text += "and " + players[winnerCount].name;
							else    victoryCanvas.Find("TextResult").GetComponent<Text>().text += players[winnerCount].name + ", ";
						}
						
						// Display the score they got
						victoryCanvas.Find("TextResult").GetComponent<Text>().text += ", each with " + players[0].score.ToString() + " points!";
					}
				}

				// If we have a TextProgress object, then we can display how many questions we answered correctly
				if ( victoryCanvas.Find("ScoreTexts/TextProgress")  )
				{
					//Write the progress text
					victoryCanvas.Find("ScoreTexts/TextProgress").GetComponent<Text>().text = correctAnswers.ToString() +  "/" +  questionLimit.ToString();
				}
				
				//If there is a source and a sound, play it from the source
				if ( soundSource && soundVictory )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundVictory);
			}
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			#if UNITY_5_3
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#else
			Application.LoadLevel(Application.loadedLevelName);
			#endif
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
			#if UNITY_5_3
			SceneManager.LoadScene(mainMenuLevelName);
			#else
			Application.LoadLevel(mainMenuLevelName);
			#endif
		}
		
		/// <summary>
		/// Updates the lives we have
		/// </summary>
		public void Updatelives()
		{
			// Update lives only if we have a lives bar assigned
			if ( players[currentPlayer].livesBar )
			{
                //Rashmi code | start
                //Change background based on number of lives
                if (players[currentPlayer].lives == 2)
                {
                    //check category name null or empty
                    if ((!(currentCatName.Length < 0)) && (currentCatName.ToString() != null))
                    {
                        //currentCatName = catName;
                        if (currentCatName == "Ice")
                        {
                            //Condition satisfied - enters this block
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Bear_Expression");
                        }
                        else if (currentCatName == "Ocean")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("clownfish2-2");
                        }
                        else if (currentCatName == "Rainforest")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Tucan_Exp");

                        }
                        else if (currentCatName == "Mountain")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Pika_Exp");
                        }
                    }
                }
                else if (players[currentPlayer].lives <= 1) {
                    if ((!(currentCatName.Length < 0)) && (currentCatName.ToString() != null))
                    {
                        //currentCatName = catName;
                        if (currentCatName == "Ice")
                        {
                            //Condition satisfied - enters this block
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Bear_Sad");
                        }
                        else if (currentCatName == "Ocean")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("clownfish3-3");
                        }
                        else if (currentCatName == "Rainforest")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Tucan_Sad");

                        }
                        else if (currentCatName == "Mountain")
                        {
                            Debug.Log(currentCatName);
                            Image bgImg = gameBackgrnd.GetComponent<Image>();
                            bgImg.sprite = Resources.Load<Sprite>("Pika_Sad");
                        }
                    }
                }
                //Rashmi Sharma | Stop

				// If we run out of lives, it's game over
				if ( players[currentPlayer].lives <= 0 )    StartCoroutine(GameOver(1));
                
			}
		}
		
		/// <summary>
		/// Shows the larger image from the thumbnail in the question image
		/// </summary>
		public void ShowLargerImage()
		{
			// Unhide the larger image canvas
			largerImageCanvas.gameObject.SetActive(true);
			
			if ( largerImageCanvas )
			{
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WEBGL
				// If we have a question video, display it in a larger box. Otherwise, display the image
				if ( questions[currentQuestion].video )
				{
					// Hide the image object
					largerImageCanvas.Find("Image").gameObject.SetActive(false);
					
					// Unhide the video object
					largerImageCanvas.Find("Video").gameObject.SetActive(true);
					
					// Get the object that we will display the video on
					RawImage rawImage = largerImageCanvas.Find("Video").GetComponent<RawImage>();
					
					// Assign the video that we want to show
					rawImage.texture = questions[currentQuestion].video;
					
					// Set the video to loop forever
					//questions[currentQuestion].video.loop = true;
					
					// Play the video
					//questions[currentQuestion].video.Play();
				}
#endif
				if ( questions[currentQuestion].image )
				{
					// Hide the video object
					largerImageCanvas.Find("Video").gameObject.SetActive(false);
					
					// Unhide the image object
					largerImageCanvas.Find("Image").gameObject.SetActive(true);
					
					// Assign the image from the question to the image in the larger image canvas
					largerImageCanvas.Find("Image").GetComponent<Image>().sprite = questions[currentQuestion].image;
				}
			}
		}

		/// <summary>
		/// Sets the number of players in the game
		/// </summary>
		/// <param name="setValue">Set value.</param>
		public void SetNumberOfPlayers( float setValue )
		{
			numberOfPlayers = Mathf.RoundToInt(setValue);
			
			// Multiply the number of questions per group by the number of players. 
			// This way, each player will get a chance to answer a question from the same group before moving on to the next group.
			questionsPerGroup = numberOfPlayers * defaultQuestionsPerGroup;

			// Update the text in the NumberOfPlayers button
			if ( startCanvas && startCanvas.Find("NumberOfPlayers/HandleSlideArea/Handle/Text") )    startCanvas.Find("NumberOfPlayers/HandleSlideArea/Handle/Text").GetComponent<Text>().text = numberOfPlayers.ToString() + " PLAYERS";

			// Create the list of players based on the number of players
			//players = new Player[numberOfPlayers];

			// Update the current list of players based on numberOfPlayers
			UpdatePlayers();
		}
		
		/// <summary>
		/// Update the list of players based on the number of players in the game
		/// </summary>
		void UpdatePlayers()
		{
			// Limit the value of numberOfPlayers to the actual number of players in the list
			if ( numberOfPlayers > players.Length )    numberOfPlayers = players.Length;
			
			// Go through all the extra players in the list, and remove their name, score, and lives objects
			for ( index = 0 ; index < players.Length ; index++ )
			{
				if ( playersObject )    
				{
					if ( index < numberOfPlayers )    
					{
						// Set the score of the active player to 0, so that it gets counted in the results page
						players[index].score = 0;

						// Activate the player object along with its name and score text
						playersObject.transform.GetChild(index).gameObject.SetActive(true);
					}
					else    
					{
						// Set the score of the inactive player to -1, so that it doesn't get counted in the results page
						players[index].score = -1;
						
						// Deactivate the player object along with its name and score text
						playersObject.transform.GetChild(index).gameObject.SetActive(false);
					}
				}
			}
			
			// Go through all players in the match and update the score and lives and names, if they exist
			if ( players.Length > 0 )
			{
				for ( index = numberOfPlayers - 1 ; index >= 0 ; index-- )
				{
					// Set the current player
					currentPlayer = index;
					
					//Update the score
					UpdateScore();
					
					// Set the lives we have
					players[currentPlayer].lives = lives;
					
					// Update the lives
					Updatelives();

					// Set the name of each player at the start of the game
					if ( players[currentPlayer].nameText )    players[currentPlayer].nameText.GetComponent<Text>().text = players[currentPlayer].name.ToString();

					// Set the color of the player name
					if ( players[currentPlayer].nameText )    players[currentPlayer].nameText.GetComponent<Text>().color = players[currentPlayer].color;
				}
				
				// Calculate the width of a single life in the lives bar
				if ( players[currentPlayer].livesBar )    livesBarWidth = players[currentPlayer].livesBar.rectTransform.sizeDelta.x/players[currentPlayer].lives;
			}
			else
			{
				Debug.LogError("You cannot play the game without setting up at least one player in the Player Options list");
			}
		}
		
		/// <summary>
		/// Loads an Xml file from a path, then parses it and assigns it to the selected game controller
		/// </summary>
		/// <param name="xmlPath">The path of the Xml file</param>
		public void LoadXml( string xmlPath )
		{
			/// This holds the highest number of answers for a question. It's used to check if there are extra answers that can't be shown because there are not enough Answer Objects in the game.
			/// ex: One of the questions has 5 answers, but the game only has 4 answer boxes. This will trigger a warning to tell you to increase the number of answer objects to 5 in order to hold them. 
			int extraAnswers = 0;
			
			// Create a new Xml document
			XmlDocument xmlDocument = new XmlDocument();
			
			// Load the Xml file from the path into the document
			xmlDocument.LoadXml(xmlPath);

			// Get the records from the Xml file. Each record contains a question, several answers, and bonus and time info
			xmlRecords = xmlDocument.GetElementsByTagName("record");

			// Set the length of the question list ( in the game controller ) based on the number of questions in the Xml file
			questions = new Question[xmlRecords.Count];

			// Go through all the questions and declare each one, so that we can fill it with info from the Xml
			for ( index = 0 ; index < questions.Length ; index++ )    questions[index] = new Question();

			// Set the index of the question, starting from 0, which is the first question
			int questionIndex = 0;
			
			// Go through all the Xml nodes, these are the ones that contain a Question, Image, Answers, Bonus, and Item nodes.
			foreach ( XmlNode XmlRecord in xmlRecords )
			{
				// Get all the questions from the Xml record
				XmlNodeList XmlQuestions = XmlRecord.ChildNodes;
				
				// Go through all the Xml questions and check which part we are accessing ( Question, Image, Answers, Bonus, or Time )
				foreach ( XmlNode XmlQuestion in XmlQuestions )
				{
					// Assign the question to the right slot in the game controller
					if ( XmlQuestion.Name == "Question" )    questions[questionIndex].question = XmlQuestion.InnerText;

					// Assign the followup text of the question to the correct slot in the game controller
					if ( XmlQuestion.Name == "Followup" )    questions[questionIndex].followup = XmlQuestion.InnerText;
					
					// Assign the image to the right slot in the game controller. All images should be placed in the Resources/Images/ path. You should enter the name of the image without the path and without an extension (ex; .png )
					if ( XmlQuestion.Name == "Image" )    questions[questionIndex].image = Resources.Load<Sprite>("Images/" + XmlQuestion.InnerText);

					if ( XmlQuestion.Name == "Video" )
					{
						#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WEBGL
						// Assign the video to the right slot in the game controller. All videos should be placed in the Resources/Videos/ path. You should enter the name of the video without the path and without an extension (ex; .mp4 )
						questions[questionIndex].video = Resources.Load<MovieTexture>("Videos/" + XmlQuestion.InnerText); 
						#else
						if ( XmlQuestion.InnerText != string.Empty )    Debug.LogWarning("You have imported a question that contains a video while using a mobile platform. Unity does not support videos on mobile platforms. The question is '" + questions[questionIndex].question + "'");
						#endif
					}

					// Assign the bonus value to the right slot in the game controller
					if ( XmlQuestion.Name == "Bonus" )    questions[questionIndex].bonus = int.Parse(XmlQuestion.InnerText);
					
					// Assign the time value to the right slot in the game controller
					if ( XmlQuestion.Name == "Time" )    questions[questionIndex].time = int.Parse(XmlQuestion.InnerText);
					
					// Set the index of the answer, starting from 0, which is the first answer
					int answerIndex = 0;
					
					// When we detect an "Answers" record, we must go through it to find all the answers
					if ( XmlQuestion.Name == "Answers" )
					{
						// Set the length of the answers list ( in the game controller ) based on the number of answers in the Xml file
						questions[questionIndex].answers = new Answer[XmlQuestion.ChildNodes.Count];
						
						// Go through all the answers and declare each one, so that we can fill it with info from the Xml
						for ( index = 0 ; index < questions[questionIndex].answers.Length ; index++ )    questions[questionIndex].answers[index] = new Answer();
						
						// Go through all the Xml nodes, these are the ones that contain an Answer, Image, and IsCorrect state nodes.
						foreach( XmlNode XmlAnswer in XmlQuestion.ChildNodes )
						{
							// If this is an answer, assign it to the game controller, and also assign the IsCorrect and Image attribute, if it exists
							if ( XmlAnswer.Name == "Answer" )
							{
								// Assign the answer
								questions[questionIndex].answers[answerIndex].answer = XmlAnswer.InnerText;
								
								// Assign the IsCorrect attribute, true or false
								questions[questionIndex].answers[answerIndex].isCorrect = bool.Parse(XmlAnswer.Attributes[0].InnerText);
								
								//questions[questionIndex].answers[answerIndex].image = bool.Parse(XmlAnswer.Attributes[1].InnerText);
								
								if ( answerIndex >= answerObjects.Length && answerIndex > extraAnswers )    extraAnswers = answerIndex;
							}
							
							answerIndex++;
						}
					}
				}
				
				questionIndex++;
				
			}
			
			// If we have question with extra answers, display a warning
			if ( extraAnswers > 0 )    Debug.LogWarning("Some of the questions in the Xml file have more answers than the maximum number of Answer Objects you have in the game. Increase the number of Answer Objects to " + (extraAnswers + 1).ToString() + " to accomodate the extra answers. Read more about it in the documentation file." );

			// If this XML was loaded from a web address, record it in the static questions list so that we don't load it each time we enter this level or restart it ( It will be reloaded if we close and open the entire game )
			if ( xmlWebAddress != String.Empty )    questionsFromWeb = questions;
		}

		/// <summary>
		/// Sets the questions list from an external question list. This is used when getting the questions from a category selector.
		/// </summary>
		/// <param name="setValue">The list of questions we got</param>
		public void SetQuestions( Question[] setValue )
		{
			questions = setValue;
		}

		public void SkipQuestion()
		{
			// Reset the question and answers in order to display the next question
			StartCoroutine(ResetQuestion(0.5f));
		}

		/// <summary>
		/// Reveals a closeup image by zooming out to show it
		/// </summary>
		/// <param name="delay">How many seconds it takes to fully reveal the image</param>
		IEnumerator RevealImage( float revealTime ) 
		{
			// If we have a closeup of the current image, zoom out to reveal it
			if ( imageCloseupZoom > 0 && imageObject )
			{
				// While the image is not fully revealed, keep zooming out
				while ( revealTime > 0 )
				{
					// Reduce the reveal time
					revealTime -= Time.deltaTime;

					// Wait for update. This is used to allow animation
					yield return new WaitForFixedUpdate();

					// Gradually reveal the full image (animated)
					imageObject.offsetMin = Vector2.Lerp( imageObject.offsetMin, Vector2.zero, Time.deltaTime * 10);
					imageObject.offsetMax = Vector2.Lerp( imageObject.offsetMax, Vector2.zero, Time.deltaTime * 10);
				}

				// Make sure the image is fully revealed
				imageObject.offsetMin = Vector2.zero;
				imageObject.offsetMax = Vector2.zero;
			}
		}
	}
}