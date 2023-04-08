using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVotingUI : MonoBehaviour
{
	public MeetingUI meetingScreen { get; private set; }
	public PlayerObject player { get; private set; }

	public Button voteButton;
	public CanvasGroup fader;
	public Transform voteHolder;
    public TMP_Text playerNameText;
    public Image playerAvatar;
	public GameObject iVotedBadge;
	public GameObject abstainedBadge;
	public GameObject meetingCallerIcon;
	public GameObject deadOverlay;
	public Image speakerIcon;
	public Image border;

	AudioSource speakerSource;
	Photon.Voice.Unity.Recorder rec;
	float[] samples;

	private void Start()
	{
		speakerSource = player.VoiceObject.SpeakerInUse.GetComponent<AudioSource>();
		rec = player.VoiceObject.RecorderInUse;
		if (player != PlayerObject.Local)
			samples = new float[512];
	}

	private void FixedUpdate()
	{
		if (player == PlayerObject.Local)
		{
			speakerIcon.color = rec.IsCurrentlyTransmitting ? player.Controller.IsDead ? Color.gray : Color.green : Color.black;

			//speakerIcon.color = Color.Lerp(
			//	Color.black,
			//	Color.green,
			//	rec.LevelMeter.CurrentPeakAmp
			//);
		}
		else
		{
			speakerIcon.color = player.VoiceObject.IsSpeaking ? player.Controller.IsDead ? Color.gray : Color.green : Color.black;

			//speakerSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);

			//float sum = 0;
			//for (int i = 0; i < 512; i++)
			//{
			//	sum += Mathf.Abs(samples[i]);
			//}
			//speakerIcon.color = Color.Lerp(Color.black, Color.green, sum / 512f);
		}
	}

	public void Init(PlayerObject player, MeetingUI screen)
	{
		meetingScreen = screen;
		this.player = player;

		playerNameText.text = player.Nickname.Value;
		playerAvatar.color = player.GetColor;
		if (player.Controller.IsDead)
		{
			deadOverlay.SetActive(true);
			voteButton.interactable = false;
			fader.alpha = 0.5f;
		}
		
		if (player == GameManager.Instance.MeetingCaller)
		{
			meetingCallerIcon.SetActive(true);
			border.color = new Color32(0xFF, 0xAD, 0x00, 0xFF);
		}
	}

	public void AddVote()
	{
		meetingScreen.VoteFor(player);
	}

	public void AddVote(PlayerObject voter)
	{
		GameObject voteObj = Instantiate(voteHolder.GetChild(0).gameObject, voteHolder);
		voteObj.SetActive(true);
		voteObj.GetComponent<Image>().color = voter.GetColor;
	}

	public void Abstain()
	{
		abstainedBadge.SetActive(true);
	}
}
