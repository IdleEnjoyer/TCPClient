using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WindowsAPICodePack.Shell.Interop;
using Point = System.Windows.Point;

namespace TCPDevice
{
	internal class APFC
	{
		private List<Brush> BrushCollection = new()
		{
			Brushes.Blue,
			Brushes.Red,
			Brushes.Green,
			Brushes.Cyan,
			Brushes.Magenta,
			Brushes.Yellow,
			Brushes.Orange,
			Brushes.Aquamarine,
			Brushes.Black,
			Brushes.Gray,
			Brushes.Purple
		};
		private List<Graph> GraphData = new();

		private double PointSize = 10;
		private int ColorIndex = 0;
		public double MaxY;
		

		private double TimeMeasurement;
		private Line XAxis = new();
		private Line YAxis = new();
		private Line Zero = new();

		public APFC() {
			YAxis.X1 = 50;
			YAxis.Y1 = 50;
			YAxis.X2 = 50;
			YAxis.Y2 = 1450;

			XAxis.X1 = 50;
			XAxis.Y1 = 1450;
			XAxis.X2 = 3450;
			XAxis.Y2 = 1450;

			Zero.X1 = 50;
			Zero.Y1 = 750;
			Zero.X2 = 3450;
			Zero.Y2 = 750;
			Zero.Opacity = 0.2;
			Zero.StrokeDashArray = new DoubleCollection { 6, 12 };
		}

		public void AddGraph(string Name, List<Point> Points)
		{
			Graph G = new(Name, Points, BrushCollection[ColorIndex]);
			NormalizeData(G.GetMeasure());
			GraphData.Add(G);
		}
		private void NormalizeData(List<Point> UnprocessedData)
		{
			try
			{
				List<Point> ProcessedData = new();
				int Offset = 0;
				foreach (Point p in UnprocessedData)
				{
					ProcessedData.Add(new(p.X, p.Y + Offset * 360.0));
					if (p != UnprocessedData.Last())
					{
						if (p.Y - UnprocessedData.ElementAt(UnprocessedData.IndexOf(p) + 1).Y > 90.0)
						{
							Offset++;
						}
						if (p.Y - UnprocessedData.ElementAt(UnprocessedData.IndexOf(p) + 1).Y < -90.0)
						{
							Offset--;
						}
					}
				}
				UnprocessedData = ProcessedData;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}");
			}
		}

		public void FindPeaks(Graph ProcGraph)
		{
			foreach (Point p in ProcGraph.GetMeasure())
			{
				if (p != ProcGraph.GetMeasure().First() && p != ProcGraph.GetMeasure().Last())
				{
					if (p.Y < ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) - 1).Y && p.Y < ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) + 1).Y)
					{
						ProcGraph.AddMinPeak(p);
					}
					if (p.Y > ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) - 1).Y && p.Y > ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) + 1).Y)
					{
						ProcGraph.AddMaxPeak(p);
					}
				}
			}
		}

		private static double Map(double value, double inputMin, double inputMax, double outputMin, double outputMax)
		{
			return ((value - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;
		}

		public void Draw(Canvas APFC_Canvas)
		{
			try
			{
				APFC_Canvas.Children.Add(YAxis);
				APFC_Canvas.Children.Add(XAxis);
				APFC_Canvas.Children.Add(Zero);

				//NormalizeData(GraphData);
				
				foreach (Graph G in GraphData)
				{
					
					FindPeaks(G);
					foreach (Point P in G.GetGraph())
					{
						Ellipse E = new();
						ToolTip TT = new();
						TT.Content = $"{G.GetName()} [ {P.X} ; {P.Y} ]";
						E.ToolTip = TT;
						E.Width = PointSize;
						E.Height = PointSize;
						E.Fill = BrushCollection[GraphData.IndexOf(G)];
						Canvas.SetLeft(E, Map(P.X, 0, G.GetGraph().Last().X, 100, 3400) - PointSize);
						Canvas.SetTop( E, Map(P.Y, 0, MaxY, 1400, 100) - PointSize);
						APFC_Canvas.Children.Add(E);
					}
					foreach (Point p in G.GetMaxPeaks())
					{
						Line l = new();
						l.X1 = p.X;
						l.Y1 = 0;
						l.X2 = p.X;
						l.Y2 = 1400;
						l.Stroke = Brushes.Red;
						l.StrokeThickness = 1;
						APFC_Canvas.Children.Add(l);
					}
					foreach (Point p in G.GetMinPeaks())
					{
						Line l = new();
						l.X1 = p.X;
						l.Y1 = 0;
						l.X2 = p.X;
						l.Y2 = 1400;
						l.Stroke = Brushes.Blue;
						l.StrokeThickness = 1;
						APFC_Canvas.Children.Add(l);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}");
			}
		}

		public void Draw(Canvas Graph, List<int> Indexes)
		{
			try
			{
				Graph.Children.Add(YAxis);
				Graph.Children.Add(XAxis);
				Graph.Children.Add(Zero);

				//NormalizeData(GraphData);

				foreach (int Index in Indexes)
				{
					Graph G = GraphData[Index];
					foreach (Point P in G.GetGraph())
					{
						Ellipse E = new();
						ToolTip TT = new();
						TT.Content = $"{G.GetName()} [ {P.X} ; {P.Y} ]";
						E.ToolTip = TT;
						E.Width = PointSize;
						E.Height = PointSize;
						E.Fill = BrushCollection[GraphData.IndexOf(G)];
						Canvas.SetLeft(E, Map(P.X, 0, G.GetGraph().Last().X, 100, 3400) - PointSize);
						Canvas.SetTop(E, Map(P.Y, 0, MaxY, 1400, 100) - PointSize);
						Graph.Children.Add(E);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}");
			}
		}

		public void ClearGraph()
		{
			GraphData.Clear();
		}
	}
}
