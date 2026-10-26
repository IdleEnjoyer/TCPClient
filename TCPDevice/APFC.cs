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
		public double MaxX;
		public double Frequency;
		public int PeriodPoints;
		private int MinXIndex = int.MaxValue;
		private int MaxXIndex;
		

		private double TimeMeasurement;
		private Line XAxis = new();
		private Line YAxis = new();
		private Line Zero = new();

		public APFC() {
			YAxis.X1 = 50;
			YAxis.Y1 = 50;
			YAxis.X2 = 50;
			YAxis.Y2 = 4950;

			XAxis.X1 = 50;
			XAxis.Y1 = 1450;
			XAxis.X2 = 9950;
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
			G.SetMeasure(NormalizeData(G.GetMeasure()));
			GraphData.Add(G);
		}
		public int GetGraphCount()
		{
			return GraphData.Count;
		}
		private List<Point> NormalizeData(List<Point> UnprocessedData)
		{
			try
			{
				List<Point> ProcessedData = new();
				int Offset = 0;
				foreach (Point p in UnprocessedData)
				{
					if (p == UnprocessedData.First()){
						if (p.Y > 180.0)
						{
							Offset -= 1;
						}
					}
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
				return ProcessedData;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}");
				return new List<Point>();
			}
		}

		private void FindPeaks(Graph ProcGraph)
		{
			foreach(Point p in ProcGraph.GetMeasure())
			{
				if (ProcGraph.GetMeasure().IndexOf(p) > 15)
				{
					//Point p = ProcGraph.GetMeasure()[Index];
					if (p != ProcGraph.GetMeasure().First() && p != ProcGraph.GetMeasure().Last())
					{
						if (p.Y < ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) - 1).Y && p.Y <= ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) + 1).Y)
						{
							ProcGraph.AddMinPeak(p);
						}
						if (p.Y > ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) - 1).Y && p.Y >= ProcGraph.GetMeasure().ElementAt(ProcGraph.GetMeasure().IndexOf(p) + 1).Y)
						{
							ProcGraph.AddMaxPeak(p);
						}
					}
				}
			}
			MinXIndex = Math.Min(ProcGraph.GetMeasure().IndexOf(ProcGraph.GetMaxPeaks()[0]), MinXIndex);
			MaxXIndex = Math.Max(ProcGraph.GetMeasure().IndexOf(ProcGraph.GetMinPeaks()[0]), MaxXIndex);
		}
		private double CalcRelativeError(Graph GMeas, Graph GIdeal)
		{			
			double AmpIdeal = (GIdeal.GetMaxPeaks()[0].Y - GIdeal.GetMinPeaks()[0].Y) / 2;
			double AmpMeas = (GMeas.GetMaxPeaks()[0].Y - GMeas.GetMinPeaks()[0].Y) / 2;
			//MessageBox.Show(AmpIdeal.ToString());
			return (AmpMeas - AmpIdeal) / AmpIdeal * 100.0;
		}
		private double CalcPhaseShift(Graph GMeas, Graph GIdeal)
		{
			return 360.0 * (GMeas.GetMaxPeaks()[0].X - GIdeal.GetMaxPeaks()[0].X) / (1 / Frequency);
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
					for(int Index = Math.Max(0, MinXIndex - PeriodPoints / 4); Index < Math.Min(MaxXIndex + PeriodPoints / 4, G.GetMeasure().Count); Index++)
					{
						Point P = G.GetMeasure()[Index];
						Ellipse E = new();
						ToolTip TT = new();
						TT.Content = $"{G.GetName()} [ {P.X} ; {P.Y} ]";
						E.ToolTip = TT;
						E.Width = PointSize;
						E.Height = PointSize;
						E.Fill = BrushCollection[GraphData.IndexOf(G)];
						Canvas.SetLeft(E, Map(P.X, 0, MaxX, 100, 3400) - PointSize / 2);
						Canvas.SetTop( E, Map(P.Y, 0, MaxY, 1400, 100) - PointSize / 2);
						APFC_Canvas.Children.Add(E);
						if (P != G.GetMeasure().Last())
						{
							Line L = new();
							L.Stroke = BrushCollection[GraphData.IndexOf(G)];
							L.StrokeThickness = 3;
							L.X1 = Map(P.X, 0, MaxX, 100, 3400);
							L.Y1 = Map(P.Y, 0, MaxY, 1400, 100);
							L.X2 = Map(G.GetMeasure()[Index + 1].X, 0, MaxX, 100, 3400);
							L.Y2 = Map(G.GetMeasure()[Index + 1].Y, 0, MaxY, 1400, 100);
							APFC_Canvas.Children.Add(L);
						}
					}
					Line lmax = new();
					lmax.X1 = Map(G.GetMaxPeaks()[0].X, 0, MaxX, 100, 3400);
					lmax.Y1 = 50;
					lmax.X2 = Map(G.GetMaxPeaks()[0].X, 0, MaxX, 100, 3400);
					lmax.Y2 = 1450;
					lmax.Stroke = Brushes.Red;
					lmax.StrokeThickness = 2;
					APFC_Canvas.Children.Add(lmax);
					Line lmin = new();
					lmin.X1 = Map(G.GetMinPeaks()[0].X, 0, MaxX, 100, 3400);
					lmin.Y1 = 50;
					lmin.X2 = Map(G.GetMinPeaks()[0].X, 0, MaxX, 100, 3400);
					lmin.Y2 = 1450;
					lmin.Stroke = Brushes.Blue;
					lmin.StrokeThickness = 2;
					APFC_Canvas.Children.Add(lmin);
					//MessageBox.Show($"{G.GetMaxPeaks().Count.ToString()} {G.GetMinPeaks().Count.ToString()}");
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
						Canvas.SetLeft(E, Map(P.X, 0, MaxX, 100, 3400) - PointSize);
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

		public List<double> ShowResults()
		{
			List<double> Results = new();
			try
			{
				for (int Index = 0; Index + 1 < GraphData.Count; Index += 2)
				{
					Results.Add(Math.Round(CalcRelativeError(GraphData[Index], GraphData[Index + 1]),3));
					Results.Add(Math.Round(CalcPhaseShift(GraphData[Index], GraphData[Index + 1]),3));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}");
			}
			return Results;
		}
		public void GetDebugInfo()
		{
			MessageBox.Show(Map(GraphData[0].GetMaxPeaks()[0].X, 0, MaxX, 100, 3400).ToString());
		}
		public void ClearGraph()
		{
			GraphData.Clear();
			MinXIndex = int.MaxValue;
			MaxXIndex = int.MinValue;
	}
	}
}
