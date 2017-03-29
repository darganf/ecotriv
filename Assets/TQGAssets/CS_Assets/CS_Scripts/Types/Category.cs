using System;
using UnityEngine;
using System.Xml;
using TriviaQuizGame.Types;

namespace TriviaQuizGame.Types
{
	[Serializable]
	public class Category:MonoBehaviour
	{
		[Tooltip("The name of the category. This is displayed in a categroy wheel or list")]
		public string categoryName;
		
		[Tooltip("The icon associated with this category. This is displayed in a category wheel or list")]
		public Sprite categoryIcon;

		[Tooltip("The color associated with this category. This is displayed in a category wheel or list")]
		public Color categoryColor;

		[Tooltip("A list of questions in the category. Each question has a number of correct/wrong answers, a followup text, a bonus value, time, and can also have an image/video as the background of the question")]
		public Question[] questions;

		// Has this category been used already?
		internal bool alreadyUsed = false;

		// The category object that displays info about this category
		internal GameObject categoryObject;

		// This is used when parsing the Xml info
		internal XmlNodeList xmlRecords;

		// A general use index
		internal int index = 0;

		/// <summary>
		/// Loads an Xml file from a path, then parses it and assigns it to the selected game controller
		/// </summary>
		/// <param name="xmlPath">The path of the Xml file</param>
		public void LoadXml( string xmlPath )
		{
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
							}
							
							answerIndex++;
						}
					}
				}
				
				questionIndex++;
				
			}
		}
	}
}
