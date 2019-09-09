using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System {
	public static class HashCodeExtensions {
		//private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

		private class ReferenceEqualityComparer : EqualityComparer<Object> {
			public override bool Equals(object x, object y) {
				return ReferenceEquals(x, y);
			}
			public override int GetHashCode(object obj) {
				if (obj == null) return 0;
				return obj.GetHashCode();
			}
		}

		private static bool IsPrimitive(this Type type) {
			if (type == typeof(String)) return true;
			return (type.IsValueType & type.IsPrimitive);
		}

		public static ulong GetStaticHashCode(this object originalObject) {
			ULong hash = new ULong();
			Dictionary<String, ulong> savedStringHashes = new Dictionary<string, ulong>();
			InternalCopy(originalObject, hash, savedStringHashes, new HashSet<object>(new ReferenceEqualityComparer()));

			return (ulong)hash.Value;
		}

		public static object CastToUnsigned(object number) {
			Type type = number.GetType();
			unchecked {
				if (type == typeof(int)) return (uint)(int)number;
				if (type == typeof(long)) return (ulong)(long)number;
				if (type == typeof(bool)) return Convert.ToUInt32(number);
				if (type == typeof(char)) return Convert.ToUInt32(number);
				if (type == typeof(short)) return (ushort)(short)number;
				if (type == typeof(sbyte)) return (byte)(sbyte)number;
			}
			return number;
		}

		private static ulong? DoHash(Object o, ULong hash, Dictionary<string, ulong> savedHashes, bool hashingType = false) {
			if (o != null) {
				unchecked {
					ulong obj = 0;
					if (o is String || o is double || o is float || o is decimal) {
						var s = o.ToString();
						if (savedHashes.ContainsKey(s)) {
							obj = savedHashes[s];
						} else {
							foreach (var c in s)
								obj += c;
							savedHashes[s] = obj;
						}
					} else {
						obj = Convert.ToUInt64(CastToUnsigned(o));
					}

					hash.Value = (hash.Value * 397) ^ obj;

					if (!hashingType)
						DoHash(o.GetType().ToString(), hash, savedHashes, true);

					return hash.Value;
				}
			}
			return 0;
		}

		private class ULong {
			public ulong Value { get; set; }
		}

		private static void InternalCopy(Object originalObject, ULong hash, Dictionary<string, ulong> savedHashes, HashSet<object> visited) {
			if (originalObject == null) return;
			var typeToReflect = originalObject.GetType();

			if (IsPrimitive(typeToReflect)) {
				DoHash(originalObject, hash, savedHashes);
				return;
			}

			if (visited.Contains(originalObject) || typeof(Delegate).IsAssignableFrom(typeToReflect)) {
				return;
			}

			if (typeToReflect.IsArray) {
				var arrayType = typeToReflect.GetElementType();
				if (!IsPrimitive(arrayType)) {
					Array clonedArray = (Array)originalObject;
					clonedArray.ForEach((array, indices) => InternalCopy(clonedArray.GetValue(indices), hash, savedHashes, visited));
				}
			}

			visited.Add(originalObject);
			CopyFields(originalObject, hash, savedHashes, visited, typeToReflect);
			RecursiveCopyBaseTypePrivateFields(originalObject, hash, savedHashes, visited, typeToReflect);
			
			//DoHash(cloneObject, hash);
			//return;
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, ULong hash, Dictionary<string, ulong> savedHashes, HashSet<object> visited, Type typeToReflect) {
			if (typeToReflect.BaseType != null) {
				RecursiveCopyBaseTypePrivateFields(originalObject, hash, savedHashes, visited, typeToReflect.BaseType);
				CopyFields(originalObject, hash, savedHashes, visited, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
			}
		}

		private static void CopyFields(object originalObject, ULong hash, Dictionary<string, ulong> savedHashes, HashSet<object> visited, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null) {
			foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags)) {
				if (filter != null && filter(fieldInfo) == false)
					continue;
				if (IsPrimitive(fieldInfo.FieldType)) {
					DoHash(fieldInfo.GetValue(originalObject), hash, savedHashes);
					continue;
				}
				InternalCopy(fieldInfo.GetValue(originalObject), hash, savedHashes, visited);
			}
		}

		private static void ForEach(this Array array, Action<Array, int[]> action) {
			if (array.LongLength == 0) return;
			ArrayTraverse walker = new ArrayTraverse(array);
			do action(array, walker.Position);
			while (walker.Step());
		}

		private class ArrayTraverse {
			public int[] Position;
			private int[] maxLengths;

			public ArrayTraverse(Array array) {
				maxLengths = new int[array.Rank];
				for (int i = 0; i < array.Rank; ++i) {
					maxLengths[i] = array.GetLength(i) - 1;
				}
				Position = new int[array.Rank];
			}

			public bool Step() {
				for (int i = 0; i < Position.Length; ++i) {
					if (Position[i] < maxLengths[i]) {
						Position[i]++;
						for (int j = 0; j < i; j++) {
							Position[j] = 0;
						}
						return true;
					}
				}
				return false;
			}
		}
	}
}
