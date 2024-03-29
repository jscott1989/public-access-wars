using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO: a lot of this code (and the code in AfternoonProp.cs) is copied from the prop selection stage - remove the duplication
// using inheritance/etc
public class Props : MonoBehaviour {
	int selectedIndex;
	
	dfScrollPanel mMyPropsList;
	GameObject mAfternoonPropPrefab;
	NetworkManager mNetworkManager;
	
	Dictionary<PurchasedProp, AfternoonProp> mPurchasedPropObjects = new Dictionary<PurchasedProp, AfternoonProp>();
	
	public PurchasedProp uSelectedPurchasedProp = null;
	
	void Awake() {
		mMyPropsList = (dfScrollPanel)FindObjectOfType(typeof(dfScrollPanel));
		mAfternoonPropPrefab = (GameObject)Resources.Load ("Afternoon/Prefabs/AfternoonProp");
		mNetworkManager = (NetworkManager)FindObjectOfType (typeof(NetworkManager));
	}

	void Start() {
		// Here we just need to look at the props available to the current player and populate it automatically
		foreach(KeyValuePair<string, PurchasedProp> kv in mNetworkManager.myPlayer.uPurchasedProps) {
			if (kv.Value.GetType() == typeof(PurchasedProp)) {
				Add (kv.Value);
			}
		}
	}
	
	public void Add(PurchasedProp pPurchasedProp) {
		GameObject prop = (GameObject) Instantiate(mAfternoonPropPrefab, Vector3.zero, Quaternion.identity);
		AfternoonProp p = (AfternoonProp)prop.GetComponent (typeof(AfternoonProp));
		p.uPurchasedProp = pPurchasedProp;
		p.uImage.Texture = pPurchasedProp.uIconTexture;
		prop.transform.parent = mMyPropsList.transform;
		mPurchasedPropObjects.Add (pPurchasedProp, p);
	}
	
	public void Remove(PurchasedProp pPurchasedProp) {
		Destroy(mPurchasedPropObjects[pPurchasedProp].gameObject);
		mPurchasedPropObjects.Remove(pPurchasedProp);
		
		if (uSelectedPurchasedProp != null && uSelectedPurchasedProp.uID == pPurchasedProp.uID) {
			uSelectedPurchasedProp = null;
		}
	}
}
