using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class QuestCutScene : MonoBehaviour
	{
		public bool DontPlayCutScenes;

		public QuestCollectionObject QuestCollection;

		public QuestCutSceneData CurrentCutScene;

		[ContextMenuItem("Test", "Test")] public Animator Animator;

		public QuestCutSceneSound QuestCutSceneSound;

		public Image CharacterImage;

		public Text CharacterName;

		public Text QuestName;

		public TextTypeEffect DialogueText;

		private int _currentCutSceneElement;

		public float Delay = 3f;

		private bool _sameQuestTracked;

		public bool IsPlaying;

		public bool textFinished;

		public bool voiceFinished;

		private void Start()
		{
			Animator = GetComponent<Animator>();
		}

		public void PlayCutScene(QuestTrigger trigger)
		{
			if (DontPlayCutScenes || trigger.TaskObject.CutScene == null)
			{
				return;
			}

			if (IsPlaying)
			{
				if (CurrentCutScene != trigger.TaskObject.CutScene)
				{
					OnCutSceneFinished();
					CurrentCutScene = trigger.TaskObject.CutScene;
					Invoke(nameof(PlayFirstElement), Delay + 3f + CurrentCutScene.Delay);
				}
			}
			else
			{
				CurrentCutScene = trigger.TaskObject.CutScene;
				Invoke(nameof(PlayFirstElement), Delay + CurrentCutScene.Delay);
			}
		}

		public void PlayCutScene(QuestCutSceneData cutscene)
		{
			if (!DontPlayCutScenes && !IsPlaying)
			{
				CurrentCutScene = cutscene;
				Invoke("PlayFirstElement", Delay + CurrentCutScene.Delay);
			}
		}

		public void PlayFirstElement()
		{
			textFinished = false;
			voiceFinished = false;
			QuestCutSceneSound.Stop();
			IsPlaying = true;
			_currentCutSceneElement = 0;
			bool flag = false;
			DialogueText.RestartAttributes();
			while (!flag && _currentCutSceneElement < CurrentCutScene.Elements.Count)
			{
				flag = true;
				foreach (QuestCutSceneData.CutSceneDependencyTask taskDependency in CurrentCutScene
					         .Elements[_currentCutSceneElement].TaskDependencyList)
				{
					if (taskDependency.Task.QuestTrigger != null &&
					    taskDependency.Task.QuestTrigger.Status != taskDependency.Status)
					{
						flag = false;
						_currentCutSceneElement++;
						break;
					}
				}
			}

			if (_currentCutSceneElement < CurrentCutScene.Elements.Count)
			{
				CharacterImage.sprite = CurrentCutScene.Elements[_currentCutSceneElement].Character.CharacterImage;
				CharacterName.text = CurrentCutScene.Elements[_currentCutSceneElement].Character.CharacterName;
				QuestName.text = Localization.GetLocalizedField(CurrentCutScene.QuestName, true);
				DialogueText.Text = CurrentCutScene.Elements[_currentCutSceneElement].Dialogue;
				gameObject.SetActive(true);
			}
			else
			{
				Invoke(nameof(OnCutSceneFinished), Delay + 2f);
			}
		}

		public void PlayNextElement()
		{
			_currentCutSceneElement++;
			while (_currentCutSceneElement < CurrentCutScene.Elements.Count)
			{
				foreach (QuestCutSceneData.CutSceneDependencyTask taskDependency in CurrentCutScene
					         .Elements[_currentCutSceneElement].TaskDependencyList)
				{
					if (taskDependency.Task.QuestTrigger != null &&
					    taskDependency.Task.QuestTrigger.Status != taskDependency.Status)
					{
						_currentCutSceneElement++;
						break;
					}
				}
			}

			if (_currentCutSceneElement < CurrentCutScene.Elements.Count)
			{
				if (CurrentCutScene.Elements[_currentCutSceneElement - 1].Character.CharacterImage !=
				    CurrentCutScene.Elements[_currentCutSceneElement].Character.CharacterImage)
				{
					Animator.SetTrigger("ChangeCharacter");
					return;
				}

				DialogueText.ResetWithNewText(CurrentCutScene.Elements[_currentCutSceneElement].Dialogue);
				PlayCutSceneSound();
			}
			else
			{
				OnCutSceneFinished();
			}
		}

		public void OnCutSceneFinished()
		{
			if (Animator != null)
			{
				Animator.SetTrigger("Close");
			}

			QuestCutSceneSound.Stop();
			IsPlaying = false;
		}

		public void OnElementFinished(bool voiceTextFinished)
		{
			if (!IsPlaying)
			{
				return;
			}

			if (voiceTextFinished)
			{
				textFinished = true;
				if (CurrentCutScene.Elements.Count > _currentCutSceneElement + 1 &&
				    !CurrentCutScene.Elements[_currentCutSceneElement + 1].PlaySound)
				{
					textFinished = false;
					PlayNextElement();
				}
				else if (voiceFinished || !QuestCutSceneSound.IsPlaying)
				{
					textFinished = false;
					voiceFinished = CurrentCutScene.Elements.Count > _currentCutSceneElement + 1 &&
					                !CurrentCutScene.Elements[_currentCutSceneElement + 1].PlaySound;
					PlayNextElement();
				}
			}
			else
			{
				voiceFinished = true;
				if (textFinished)
				{
					textFinished = false;
					voiceFinished = false;
					PlayNextElement();
				}
			}
		}

		public void PlayCutSceneSound()
		{
			if (CurrentCutScene.Elements[_currentCutSceneElement].PlaySound)
			{
				QuestCutSceneSound.Play(CurrentCutScene.Elements[_currentCutSceneElement].DialogueSound);
			}
		}

		public void ChangeCharacter()
		{
			CharacterImage.sprite = CurrentCutScene.Elements[_currentCutSceneElement].Character.CharacterImage;
			CharacterName.text = CurrentCutScene.Elements[_currentCutSceneElement].Character.CharacterName;
			DialogueText.ResetWithNewText(CurrentCutScene.Elements[_currentCutSceneElement].Dialogue);
		}
	}
}
