using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class NFCHandler {

	/* If you want a tag to be recognised (and not default to "-1", add the value to this array) */
	private readonly string[] validValues = {"0", "1", "2", "3", "4", "8", "9"};

	private AndroidJavaObject mActivity;
	private AndroidJavaObject mIntent;
	private string sAction;

	public NFCHandler() {}

	/* Returns the value of the last scanned tag as a string. "-1" for no tag scanned */
	public string getScannedTag() {
		return checkNFC();
	}

	private string checkNFC() {
		if (Application.platform == RuntimePlatform.Android) {
			try {
				mActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				mIntent = mActivity.Call<AndroidJavaObject>("getIntent");
				sAction = mIntent.Call<String>("getAction");
				if (sAction == "android.nfc.action.NDEF_DISCOVERED") {
					AndroidJavaObject[] mNdefMessage = mIntent.Call<AndroidJavaObject[]>("getParcelableArrayExtra", "android.nfc.extra.NDEF_MESSAGES");
					AndroidJavaObject[] mNdefRecord = mNdefMessage[0].Call<AndroidJavaObject[]>("getRecords");
					byte[] payLoad = mNdefRecord[0].Call<byte[]>("getPayload");

					if (mNdefMessage != null) {
						string nfcValue = System.Text.Encoding.UTF8.GetString(payLoad);
						mIntent.Call("removeExtra", "android.nfc.extra.NDEF_MESSAGES");
						mIntent.Call("removeExtra", "android.nfc.extra.TAG");
						if (validValues.Contains(nfcValue)) return nfcValue;
					}
				}
			}	catch (Exception ex) { }
		}
		return "-1";
	}

}
