using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TriviaQuizGame.Types;

namespace TriviaQuizGame
{
	/// <summary>
	/// Includes functions for loading levels and URLs. It's intended for use with UI Buttons
	/// </summary>
	public class TQGCategorySelector:MonoBehaviour
	{
		// Various objects we cache for quicker access
		internal Transform categoryWheelObject;
		internal GameObject wheelPointer;
		internal RectTransform categoryObject;
		internal Transform thisTransform;
		internal GameObject gameController;

		[Tooltip("A list of categories, each containing a set of questions for the quiz. You can choose one category at a time")]
		public Category[] categories;

		// The current category we have selected
		internal int currentCategory = 0;
		internal GameObject currentCategoryObject;

		// The slice size and angle in the category wheel. This is calculated automatically based on the number of categories
		internal float sliceSize;
		internal float sliceAngle;

		[Tooltip("How many categories should we play and win before winning the game")]
		public int categoriesToVictory = 3;

		[Tooltip("The menu that appears after finishing enough categories in the game")]
		public Transform victoryCanvas;

		[Tooltip("The sound that plays after finishing enough categories in the game")]
		public AudioClip soundVictory;

		public bool randomize = true;

		// General use index
		internal int index;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			thisTransform = transform;

			if ( randomize == true )    categories = ShuffleCategories(categories);

			// If there is no sound source assigned, try to assign it from the tag name
			//if ( !soundSource && GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Cache the category wheel and category object for quicker access
			if ( GameObject.Find("CategoryWheel") )    categoryWheelObject = GameObject.Find("CategoryWheel").transform;
			if ( GameObject.Find("CategoryWheel/WheelHolder/Wheel/Category") )    categoryObject = GameObject.Find("CategoryWheel/WheelHolder/Wheel/Category").GetComponent<RectTransform>();
			if ( GameObject.FindGameObjectWithTag("GameController") )    gameController = GameObject.FindGameObjectWithTag("GameController");
			if ( GameObject.Find("CategoryWheel/Pointer") )    wheelPointer = GameObject.Find("CategoryWheel/Pointer");

			// If we have a category wheel, organize the categories in the wheel
			if ( categoryWheelObject && categoryObject )
			{
				// Calculate the size and angle of each category slice in the wheel
				sliceSize = 1.0f/categories.Length;
				sliceAngle = Mathf.RoundToInt(360/categories.Length);

				//Duplicate the the category object several times to accomodate the number of categories we have
				for ( index = 0 ; index < categories.Length ; index++ )
				{
					// Duplicate the category object
					RectTransform newCategory = Instantiate(categoryObject) as RectTransform;

					// Put it inside the category wheel
					newCategory.SetParent(categoryWheelObject.Find("WheelHolder/Wheel").transform);

					// Set the position and scale of the category to be the same as the default one
					newCategory.sizeDelta = categoryObject.sizeDelta;
					newCategory.position = categoryObject.position;
					newCategory.localScale = categoryObject.localScale;

					// Set the rotation of the category slice so that they are all nicely placed in a circle
					newCategory.eulerAngles = Vector3.forward * sliceAngle * index;

					// Set the color of the slice based on the value in the category object
					newCategory.Find("Slice").GetComponent<Image>().color = categories[index].categoryColor;

					// Set the size of the slice based on the general slice size
					newCategory.Find("Slice").GetComponent<Image>().fillAmount = sliceSize;

					// Set the text of the slice based on the category name
					newCategory.Find("Text").GetComponent<Text>().text = categories[index].categoryName;

					// Hold a reference for this category, for easier access
					categories[index].categoryObject = newCategory.gameObject;

					// Set the icon of the category based on the category icon
					newCategory.Find("Icon").GetComponent<Image>().sprite = categories[index].categoryIcon;

					// Rotate the icon and align it with the rotation of the category slice
					newCategory.Find("Icon").RotateAround( categoryWheelObject.transform.position, Vector3.forward, sliceAngle * 0.5f);
				}

				// Deactivate the original category object
				categoryObject.gameObject.SetActive(false);
			}

			// Choose the first category in the list
			SetCategory(currentCategory);
		}


		/// <summary>
		/// Set the current category from the list. Will go through all categories before repeating.
		/// </summary>
		public void SetCategory( int setValue )
		{
			if ( categoryWheelObject ) 
			{
				// Set the rotation of the category wheel so that it aligns with the wheel arrow
				categoryWheelObject.Find("WheelHolder/Wheel").localEulerAngles = Vector3.forward * currentCategory * -sliceAngle;

				// Play the wheel spin animation
				categoryWheelObject.GetComponent<Animation>().Play();

				// Set the questions in the quiz based on the currently selected category
				gameController.SendMessage("SetQuestions", categories[setValue].questions);
			}
		}

		/// <summary>
		/// Activates the quiz and starts the game using the current list of questions
		/// </summary>
		public void StartGame()
		{
            //Rashmi Code
            //Debug.Log(categories[currentCategory].categoryName);
            gameController.SendMessage("StartGame", categories[currentCategory].categoryName);

			// Deactivate the current category, because it has been used already
			categories[currentCategory].categoryObject.SetActive(false);

			// Set the category as "already used"
			categories[currentCategory].alreadyUsed = true;

			// Go to the next category
			currentCategory++;

			// If we finished enough categories, show the victory screen
			if ( currentCategory >= categoriesToVictory )
			{
				// Set the victory screen in the quiz as the screen to be displayed when we finish this category
				gameController.GetComponent<TQGGameController>().victoryCanvas = victoryCanvas;

				// Set the victory sound in the quiz as the sound to be played when we finish this category
				gameController.GetComponent<TQGGameController>().soundVictory = soundVictory;
			}

            // Rashmi's code change
            // Asking Game controller to start the game
            // Sending information about the current selected category
            //gameController.SendMessage("Start", categories[currentCategory-1]);
		}

		/// <summary>
		/// Shuffles the specified categories list, and returns it
		/// </summary>
		/// <param name="categories">A list of categories</param>
		Category[] ShuffleCategories( Category[] categories )
		{
			// Go through all the categories and shuffle them
			for ( index = 0 ; index < categories.Length ; index++ )
			{
				// Hold the question in a temporary variable
				Category tempCategory = categories[index];
				
				// Choose a random index from the question list
				int randomIndex = UnityEngine.Random.Range( index, categories.Length);
				
				// Assign a random question from the list
				categories[index] = categories[randomIndex];
				
				// Assign the temporary question to the random question we chose
				categories[randomIndex] = tempCategory;
			}
			
			return categories;
		}

		/// <summary>
		/// Set the next category when this screen is activated
		/// </summary>
		void OnEnable()
		{
			SetCategory(currentCategory);
		}
	}
}