using PlayFab;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace UI
{
	public class LoginRegisterUI : MonoBehaviour
	{
		[Header("Header Buttons")]
		public GameObject loginPanel;
		public GameObject leaderboardPanel;
		public Button LoginBTN;
		public Button RegisterBTN;
		public Color NormalColor;
		public Color HighlightedColor;
		[Header("Form")]
		public GameObject DisplayNameGO;
		public TMP_InputField DisplayName;
		public TMP_InputField UserName;
		public TMP_InputField Password;
		public GameObject ConfirmPassGO;
		public TMP_InputField ConfirmPass;
		public Button submit;
		public TextMeshProUGUI submitText;
		public TextMeshProUGUI feedBackText;
		public Toggle rememberMe;
		[Space]
		public UnityEvent OnLoginSuccessfull;
		private const string EMAIL = "UserEmail";
		private const string PASSWORD = "Password";
		private void Start()
		{
			string email = PlayerPrefs.GetString(EMAIL, "");
			if (!string.IsNullOrEmpty(email))
			{
				loginPanel.SetActive(false);
				string password = PlayerPrefs.GetString(PASSWORD, "");
				PlayFabLogin.Instance.Login(email, password, OnLogin, OnLoginFail);
			}
			else
			{
				loginPanel.SetActive(true);
				RegisterBTN.image.color = NormalColor;

				LoginBTN.image.color = HighlightedColor;
				Login();
				feedBackText.gameObject.SetActive(false);
			}
		}
		private void OnDisable()
		{
			PlayFabLogin.Instance.LogOut();
		}
		public void Login()
		{
			DisplayNameGO.SetActive(false);
			ConfirmPassGO.SetActive(false);
			RegisterBTN.image.color = NormalColor;
			LoginBTN.image.color = HighlightedColor;
			submitText.text = "Login";
			submit.onClick.RemoveAllListeners();
			submit.onClick.AddListener(BindLogin);
		}
		public void Register()
		{
			DisplayNameGO.SetActive(true);
			ConfirmPassGO.SetActive(true);
			RegisterBTN.image.color = HighlightedColor;
			LoginBTN.image.color = NormalColor;
			submitText.text = "Register";
			submit.onClick.RemoveAllListeners();
			submit.onClick.AddListener(BindRegister);
		}
	[ContextMenu("logout")]
		public void Logout()
		{
			PlayFabLogin.Instance.LogOut();
			leaderboardPanel.SetActive(false);
			loginPanel.SetActive(true);

			PlayerPrefs.SetString(EMAIL, "");
			PlayerPrefs.SetString(PASSWORD, "");
		}

		private void BindLogin()
		{
			if (string.IsNullOrEmpty(UserName.text) || string.IsNullOrEmpty(Password.text))
			{
				DisplayError("Email and Password are Mandatory");
			}	
			else if (Password.text.Length < 6)
			{
				DisplayError("Password is too short");
			}
			else
			{
				if (rememberMe.isOn)
				{
					PlayerPrefs.SetString(EMAIL, UserName.text);
					PlayerPrefs.SetString(PASSWORD, Password.text);
				}
				PlayFabLogin.Instance.Login(UserName.text, Password.text, OnLogin, OnLoginFail);
			}
		}
		private void BindRegister()
		{
			if (string.IsNullOrEmpty(UserName.text) || string.IsNullOrEmpty(Password.text))
			{
				DisplayError("Email and Password are Mandatory");
			}
			else if (string.Compare(Password.text, ConfirmPass.text, false) != 0)
			{
				DisplayError("Both password should match!");

			}
			else if (Password.text.Length < 6)
			{
				DisplayError("Password is too short");
			}
			else
			{
				if (rememberMe.isOn)
				{
					PlayerPrefs.SetString(EMAIL, UserName.text);
					PlayerPrefs.SetString(PASSWORD, Password.text);
				}
				PlayFabLogin.Instance.Register(UserName.text, DisplayName.text, Password.text, OnLogin, OnLoginFail);
			}
		}
		private void OnLogin()
		{
			OnLoginSuccessfull?.Invoke();
			DisplaySuccess("Successfully logged in");
		}
		private void OnLoginFail(string v)
		{
			DisplayError(v);
		}
		private void DisplayError(string msg)
		{
			feedBackText.gameObject.SetActive(true);
			feedBackText.color = Color.red;
			feedBackText.text = msg;
			StartCoroutine(HideFeedbackText());
		}
		private void DisplaySuccess(string msg)
		{
			feedBackText.gameObject.SetActive(true);
			feedBackText.color = Color.green;
			feedBackText.text = msg;
			StartCoroutine(HideFeedbackText());
		}

		IEnumerator HideFeedbackText()
		{
			yield return new WaitForSeconds(2);
			feedBackText.gameObject.SetActive(false);
		}
	}
}
