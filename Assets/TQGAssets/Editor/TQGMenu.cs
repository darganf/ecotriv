using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using TriviaQuizGame.Types;

namespace TriviaQuizGame
{	
	/// <summary>
	/// This is a menu for Trivia Quiz Games. It appears under Tools in the top menu of Unity. In it you can
	/// import questions and answers from an XML to the selected game controller.
	/// </summary>
	public class TQGMenu:MonoBehaviour 
	{
		// Add a menu item named "Import Questions From XML" to MyMenu in the menu bar.
		[MenuItem("Tools/Trivia Quiz Game/Import Questions From XML")]
		static void ImportXML() 
		{
			// Check the currently selected gameobject in the editor
			GameObject gameController  = Selection.activeObject as GameObject;

			// If the selected gameobject does not contain a TQGGameController component, give an error
			if ( gameController == null || gameController.GetComponent<TQGGameController>() == null && gameController.GetComponent<Category>() == null ) 
			{
				EditorUtility.DisplayDialog("Quiz object not selected!","You must select a Quiz object in order to import the questions to it. A Quiz is any object with a TQGGameController or Category component attached to it.","Ok");
				return;
			}

			// Open the system file menu so we can select a file to import. We can only import XML files
			var path = EditorUtility.OpenFilePanel("Overwrite with xml","","xml");

			// If we chose an XML file, load it into the currently selected game controller 
			if ( path.Length != 0 ) 
			{	
				// Run the LoadXML function in the game controller with the XML file we loaded
				if ( gameController.GetComponent<TQGGameController>() )    gameController.GetComponent<TQGGameController>().LoadXml(File.ReadAllText(path));

				// Run the LoadXML function in the category with the XML file we loaded
				if ( gameController.GetComponent<Category>() )    gameController.GetComponent<Category>().LoadXml(File.ReadAllText(path));

				// Apply the changes made to the game controller ( imported questions and answers )
				PrefabUtility.ReplacePrefab( gameController, PrefabUtility.GetPrefabParent(gameController), ReplacePrefabOptions.ConnectToPrefab);
			}
		}
	}
}