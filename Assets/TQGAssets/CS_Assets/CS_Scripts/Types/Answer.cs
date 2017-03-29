using System;
using UnityEngine;

namespace TriviaQuizGame.Types
{
	[Serializable]
	public class Answer
	{
		[Tooltip("The answer to a question")]
		public string answer;

		//[Tooltip("An image that accompanies the answer. You can leave this empty if you don't want an image")]
		//public Sprite image;

		[Tooltip("This answer is correct")]
		public bool isCorrect = false;
	}
}
