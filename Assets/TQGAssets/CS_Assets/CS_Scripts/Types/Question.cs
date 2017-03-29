using System;
using UnityEngine;
using TriviaQuizGame.Types;

namespace TriviaQuizGame.Types
{
	[Serializable]
	public class Question
	{
		[Tooltip("The question presented")]
		public string question;
		
		[Tooltip("An image that accompanies the question. You can leave this empty if you don't want an image")]
		public Sprite image;

#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_BLACKBERRY && !UNITY_WP8 && !UNITY_WEBGL
		[Tooltip("A video that plays along with the question. You can leave this empty if you don't want a video")]
		public MovieTexture video;
#endif

		[Tooltip("A list of answers to choose from. A question may have several correct/wrong answers")]
		public Answer[] answers;

		[Tooltip("A followup text that will be displayed after this question is answered. While displayed, the game is paused.")]
		public String followup;
		
		[Tooltip("How many point we get if we answer this question correctly. The bonus value is also used to sort the questions from the easy ( low bonus ) to the difficult ( high bonus )")]
		public float bonus;
		
		[Tooltip("How many seconds to answer this question we have. This should logically be tied to the difficulty of the question, same as the bonus. But the questions are sorted only based on the bonus, and not the time")]
		public float time;
	}
}
