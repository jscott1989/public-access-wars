// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
public abstract class RPCEncoder
{
	public static string Encode(string[] p) {
		return string.Join(",", p);
	}

	public static string[] Decode(string p) {
		if (p == "") {
			return new string[]{};
		}
		return p.Split (',');
	}
}
