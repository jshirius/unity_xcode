using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

//・plist追加
//・コンパイルオプションの追加
//・ソースコードの追加

//参考サイト
//https://bitbucket.org/Unity-Technologies/iosnativecodesamples/


public class XcodeProject : MonoBehaviour {

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {
	
		if (buildTarget == BuildTarget.iOS) {

			//Xcodeの設定
			ConfigXcodeSetting(buildTarget, path);
		}
	}

	private static void ConfigXcodeSetting(BuildTarget buildTarget, string path) {

		//xcodeprojの設定
		string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		
		PBXProject proj = new PBXProject();
		proj.ReadFromString(File.ReadAllText(projPath));

		string target = proj.TargetGuidByName("Unity-iPhone");

		Debug.Log("projPath:" + projPath);

		//フレームワークを追加する
		//falseは必須, trueはoption
		proj.AddFrameworkToProject(target, "iAd.framework", false );
		proj.AddFrameworkToProject(target, "Security.framework", false);


		//plistの追加
		var plistPath = Path.Combine (path, "Info.plist");
		var plist = new PlistDocument ();
		plist.ReadFromFile (plistPath);

		// 文字列の設定
		plist.root.SetString ("HogeHogeKey", "HogeHogeValue");

		// URLスキーマの追加
		var array = plist.root.CreateArray ("CFBundleURLTypes");
		var urlDict = array.AddDict ();
		urlDict.SetString ("CFBundleURLName", "HogeHogeBundleURLName");
		var urlInnerArray = urlDict.CreateArray ("CFBundleURLSchemes");
		urlInnerArray.AddString ("HogeHogeValue");

		//plistの設定を反映
		plist.WriteToFile (plistPath);


		//build設定変更
		proj.SetBuildProperty(target,"ENABLE_BITCODE","NO");


		//コンパイルオプションの設定
		// HttpNative.mに-fno-objc-arcを設定する
		//※すでにコンパイルフラグが設定されていると、追加されるので一旦削除すること
		var guid = proj.FindFileGuidByProjectPath("Libraries/Plugins/iOS/HttpNative.m");
		var flags = proj.GetCompileFlagsForFile(target, guid);
		flags.Clear();	//2回目以降のbuildで、すでにフラグが設定されていることを考慮しクリアする
		flags.Add("-fno-objc-arc");
		proj.SetCompileFlagsForFile(target, guid, flags);

		//SampleNative.mに-fno-objc-arcを設定する
		guid = proj.FindFileGuidByProjectPath("Libraries/Plugins/iOS/SampleNative.m");
		flags = proj.GetCompileFlagsForFile(target, guid);
		flags.Clear();	//2回目以降のbuildで、すでにフラグが設定されていることを考慮しクリアする
		flags.Add("-fno-objc-arc");
		proj.SetCompileFlagsForFile(target, guid, flags);


		//設定を書き出す
		File.WriteAllText(projPath, proj.WriteToString());
	}
}
