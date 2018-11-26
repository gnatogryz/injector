using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEngine {
	public static class LazyGetter {

		public static T Lazy<T>(this MonoBehaviour beh, ref T var) where T : Component {
			if (var == null) {
				var = beh.GetComponent<T>();
			}
			return var;
		}

		public static T LazyByName<T>(this MonoBehaviour beh, ref T var, string name) where T : Component {
			if (var == null) {
				var = beh.transform.Find(name).GetComponent<T>();
			}
			return var;
		}


		public static T LazyByTag<T>(this MonoBehaviour beh, ref T var, string tag) where T : Component {
			if (var == null) {
				var = GameObject.FindWithTag(tag).GetComponent<T>();
			}
			return var;
		}
	}





	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class FindInChildrenByNameAttribute : PropertyAttribute {
		public string name => _name;
		string _name;

		public FindInChildrenByNameAttribute([CallerMemberName] string name = null) {
			this._name = name;
		}
	}
}
