using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class SG_Email : MonoBehaviour
{
	string api_user = "htp2015";
	string api_key = "dellintel123";
	string fromEmail = "your.roomate@your.house";
	string toEmail = "roycraft3@gmail.com";
	string subject = "Sendgrid Email Awesomeness";
	string body;

	void Start ()
	{
		this.body = "Dear Fellow Space Sharer,";
		this.body += "\nI have sent you multiple text alerts to inform you of your continuous sound-making this night. Maybe you lost your phone (that could explain the racket if you are trying to find it), otherwise you are being way too loud. I care about my health and well-being- and I am really not that pleasant to be around when I don’t get enough sleep. For both of our sakes, keep it down!";
		this.body += "\n\nI am attaching a demographic to help share with you the importance of good sleeping habits.";
		this.body += "\n\nhttp://bit.ly/1H3KB6L";
		this.body += "Thank you and good night,\nYour Roommate";
	}

	string xsmtpapiJSON = "{\"category\":\"unity_game_email\"}";

	public void SendSendgridEmailWebBatch (string[] to)
	{
		foreach (string s in to) {
			if (s == "") {
				continue;
			}
			this.toEmail = s;
			this.SendSendgridEmailWebAPI();
		}
	}

	public void SendSendgridEmailSMTP ()
	{
		MailMessage mail = new MailMessage ();
		
		mail.From = new MailAddress (fromEmail);
		mail.To.Add (toEmail);
		mail.Subject = subject;
		mail.Body = body;
		mail.Headers.Add ("X-SMTPAPI", xsmtpapiJSON);
		
		SmtpClient smtpServer = new SmtpClient ("smtp.sendgrid.net");
		smtpServer.Port = 587;
		smtpServer.Credentials = new System.Net.NetworkCredential (api_user, api_key) as ICredentialsByHost;
		smtpServer.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = 
			delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};
		smtpServer.Send (mail);
		Debug.Log ("Success, email sent through SMTP!");
	}
	
	public void SendSendgridEmailWebAPI ()
	{
		string url = "https://sendgrid.com/api/mail.send.json?";
		WWWForm form = new WWWForm ();
		form.AddField ("to", toEmail);
		form.AddField ("from", fromEmail);
		
		//you have to change every instance of space to %20 or you'll get a 400 error
		string subjectWithoutSpace = subject.Replace (" ", "%20");
		form.AddField ("subject", subjectWithoutSpace);
		string bodyWithoutSpace = body.Replace (" ", "%20");
		form.AddField ("text", bodyWithoutSpace);

		//form.AddField ("x-smtpapi", xsmtpapiJSON);
		form.AddField ("api_user", api_user);
		form.AddField ("api_key", api_key);

		WWW www = new WWW (url, form);
		StartCoroutine (WaitForRequest (www));
	}
	
	IEnumerator WaitForRequest (WWW www)
	{
		yield return www;
		
		// check for errors
		if (www.error == null) {
			Debug.Log ("WWW Ok! Email sent through Web API: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}
}