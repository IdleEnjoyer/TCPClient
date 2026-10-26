using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace TCPDevice
{
	internal class Graph
	{
		private List<Point> MeasurePoints = new();
		private List<Point> CanvasPoints = new();
		private List<Point> MaxPeaks = new();
		private List<Point> MinPeaks = new();
		private Point MaxPoint = new(0, double.MinValue);
		private Point MinPoint = new(0, double.MaxValue);
		private string GraphName;
		private Brush GraphColor;

		public List<Point> GetMeasure()
		{
			return MeasurePoints;
		}

		public void SetMeasure(List<Point> List)
		{
			MeasurePoints = List;
		}

		public List<Point> GetGraph()
		{
			return CanvasPoints;
		}

		public string GetName()
		{
			return GraphName;
		}

		public List<Point> GetMaxPeaks()
		{
			return MaxPeaks;
		}

		public List<Point> GetMinPeaks()
		{
			return MinPeaks;
		}

		public void AddMaxPeak(Point point)
		{
			MaxPeaks.Add(point);
			if (point.Y > MaxPoint.Y)
			{
				MaxPoint = point;
			}
		}

		public void AddMinPeak(Point point)
		{
			MinPeaks.Add(point);
			if (point.Y < MinPoint.Y)
			{
				MinPoint = point;
			}
		}

		public void SetMaxPoint(Point point)
		{
			MaxPoint = point;
		}
		public Point GetMaxPoint()
		{
			return MaxPoint;
		}
		public void SetMinPoint(Point point)
		{
			MinPoint = point;
		}
		public Point GetMinPoint()
		{
			return MinPoint;
		}

		public Graph(string Name, List<Point> Points, Brush GColor)
		{
			GraphName = Name;
			MeasurePoints = Points;
			GraphColor = GColor;
		}
	}
}
