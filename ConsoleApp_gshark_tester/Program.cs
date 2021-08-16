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

			Point3 p3 = new Point3(2, 3, 4);
			List<Point3> pts = new List<Point3>() { p1, p3, p2 };

			Polyline pl = new Polyline(pts);

			Polyline pl2 = pl.Closed();
			//output
			print(pl2);

			Console.WriteLine();

			Line[] lines = pl2.Segments;
			foreach (Line l in lines)
			{
				Console.WriteLine(l);
			}

			Console.ReadLine();
		}

		private static void print(Object data)
		{
			PropertyInfo[] properties = data.GetType().GetProperties();
			foreach (PropertyInfo prop in properties)
			{
				try
				{
					Console.WriteLine(prop.Name + " = " + prop.GetValue(data));
				}
				catch
				{
					Console.WriteLine(prop.Name + " = ");
					try
					{
						print(prop.GetValue(data));
					}
					catch
					{
						Console.WriteLine(prop.Name + " = ");
					}
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
