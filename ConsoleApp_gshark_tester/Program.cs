using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GShark.Geometry;

namespace ConsoleApp_gshark_tester
{
	class Program
	{
		static void Main(string[] args)
		{
			Point3 p1 = new Point3(1, 2, 3);
			Point3 p2 = new Point3(3, 4, 5);

			Point3 p3 = new Point3(2,3,4);

			Line ln = new Line(p1, p2);

			//output
			print(ln);

			Console.ReadLine();
		}

		private static void print(Object data)
		{
			PropertyInfo[] properties = data.GetType().GetProperties();
			foreach (PropertyInfo prop in properties)
			{
				if (isList(prop.GetValue(data)))
				{
					Console.WriteLine(prop.Name + " = ");
					dynamic value = prop.GetValue(data);
					Type type = value.GetType();
					Console.WriteLine(type);
					//foreach (dynamic obj in objs)
					//{
					//	Console.WriteLine(prop.GetValue(data));
					//}
				}
				else
				{
					Console.WriteLine(prop.Name + " = " + prop.GetValue(data)); 
				}
			}
		}

		private static bool isList(object obj)
		{
			if (obj == null) return false;
			return obj is IList &&
				obj.GetType().IsGenericType &&
				obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}

		private static bool TryCast<T>(Object obj, out T result)
		{
			if (obj is T)
			{
				result = (T)obj;
				return true;
			}

			result = default(T);
			return false;
		}
	}
}
