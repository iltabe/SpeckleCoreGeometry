﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleCoreGeometryDynamo
{
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser() { }
  }

  public static class Conversions
  {
    private const double EPS = 1e-6;
    private const string speckleKey = "speckle";
    private const string appId = null;

    public static bool SetBrepDisplayMesh = true;
    public static bool AddMeshTextureCoordinates = false;
    public static bool AddRhinoObjectProperties = false;
    public static bool AddBasicLengthAreaVolumeProperties = false;


    #region Helper Methods

    public static double[] ToArray(this Point pt)
    {
      return new double[] { pt.X, pt.Y, pt.Z };
    }

    public static Point ToPoint(this double[] arr)
    {
      return Point.ByCoordinates(arr[0], arr[1], arr[2]);
    }

    public static double ToDegrees(this double radians)
    {
      return radians * (180 / Math.PI);
    }

    public static double ToRadians(this double degrees)
    {
      return degrees * (Math.PI / 180);
    }

    public static bool Threshold(double value1, double value2, double error = EPS)
    {
      return Math.Abs(value1 - value2) <= error;
    }

    public static double Median(double min, double max)
    {
      return ((max - min) * 0.5) + min;
    }

    /// SpeckleCore does not currently support dictionaries, therofere avoiding the canonical ToSpeckle
    public static Dictionary<string, object> ToSpeckleX(this DesignScript.Builtin.Dictionary dict)
    {
      if (dict == null) { return null; }
      var speckleDict = new Dictionary<string, object>();
      foreach (var key in dict.Keys)
      {
        object value = dict.ValueAtKey(key);
        if (value is DesignScript.Builtin.Dictionary)
        {
          value = (value as DesignScript.Builtin.Dictionary).ToSpeckleX();
        }
        else if (value is Geometry)
        {
          value = Converter.Serialise(value);
        }
        speckleDict.Add(key, value);
      }
      return speckleDict;
    }

    /// SpeckleCore does not currently support dictionaries, therofere avoiding the canonical ToNative
    public static DesignScript.Builtin.Dictionary ToNativeX(this Dictionary<string, object> speckleDict)
    {
      if (speckleDict == null) { return null; }
      var keys = new List<string>();
      var values = new List<object>();
      foreach (var pair in speckleDict)
      {
        object value = pair.Value;
        if (value is Dictionary<string, object>)
        {
          value = (value as Dictionary<string, object>).ToNativeX();
        }
        else if (value is SpeckleObject)
        {
          value = Converter.Deserialise(value as SpeckleObject);
        }
        keys.Add(pair.Key);
        values.Add(value);
      }
      return DesignScript.Builtin.Dictionary.ByKeysValues(keys, values);
    }

    public static Dictionary<string, object> GetSpeckleProperties(this DesignScriptEntity geometry)
    {
      var userData = geometry.Tags.LookupTag(speckleKey) as DesignScript.Builtin.Dictionary;
      return userData.ToSpeckleX();
    }

    public static T SetSpeckleProperties<T>(this DesignScriptEntity geometry, Dictionary<string, object> properties)
    {
      if (properties != null)
      {
        geometry.Tags.AddTag(speckleKey, properties.ToNativeX());
      }
      return (T)Convert.ChangeType(geometry, typeof(T));
    }

    #endregion

    #region Numbers
    public static SpeckleNumber ToSpeckle(this float num)
    {
      return new SpeckleNumber(num);
    }

    public static SpeckleNumber ToSpeckle(this long num)
    {
      return new SpeckleNumber(num);
    }

    public static SpeckleNumber ToSpeckle(this int num)
    {
      return new SpeckleNumber(num);
    }

    public static SpeckleNumber ToSpeckle(this double num)
    {
      return new SpeckleNumber(num);
    }

    public static double? ToNative(this SpeckleNumber num)
    {
      return num.Value;
    }
    #endregion

    #region Booleans
    public static SpeckleBoolean ToSpeckle(this bool b)
    {
      return new SpeckleBoolean(b);
    }

    public static bool? ToNative(this SpeckleBoolean b)
    {
      return b.Value;
    }
    #endregion

    #region Strings
    public static SpeckleString ToSpeckle(this string b)
    {
      return new SpeckleString(b);
    }

    public static string ToNative(this SpeckleString b)
    {
      return b.Value;
    }
    #endregion

    #region Points

    /// <summary>
    /// DS Point to SpecklePoint
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    public static SpecklePoint ToSpeckle(this Point pt)
    {
      return new SpecklePoint(pt.X, pt.Y, pt.Z, appId, pt.GetSpeckleProperties());
    }

    /// <summary>
    /// Speckle Point to DS Point
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    public static Point ToNative(this SpecklePoint pt)
    {
      var point = Point.ByCoordinates(pt.Value[0], pt.Value[1], pt.Value[2]);

      return point.SetSpeckleProperties<Point>(pt.Properties);
    }

    /// <summary>
    /// Array of point coordinates to array of DS Points
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static Point[] ToPoints(this IEnumerable<double> arr)
    {
      if (arr.Count() % 3 != 0) throw new Exception("Array malformed: length%3 != 0.");

      Point[] points = new Point[arr.Count() / 3];
      var asArray = arr.ToArray();
      for (int i = 2, k = 0; i < arr.Count(); i += 3)
        points[k++] = Point.ByCoordinates(asArray[i - 2], asArray[i - 1], asArray[i]);

      return points;
    }

    /// <summary>
    /// Array of DS Points to array of point coordinates
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static double[] ToFlatArray(this IEnumerable<Point> points)
    {
      return points.SelectMany(pt => pt.ToArray()).ToArray();
    }

    #endregion

    #region Vectors
    /// <summary>
    /// DS Vector to SpeckleVector
    /// </summary>
    /// <param name="vc"></param>
    /// <returns></returns>
    public static SpeckleVector ToSpeckle(this Vector vc)
    {
      return new SpeckleVector(vc.X, vc.Y, vc.Z);
    }

    /// <summary>
    /// SpeckleVector to DS Vector
    /// </summary>
    /// <param name="vc"></param>
    /// <returns></returns>
    public static Vector ToNative(this SpeckleVector vc)
    {
      return Vector.ByCoordinates(vc.Value[0], vc.Value[1], vc.Value[2]);
    }

    /// <summary>
    /// DS Vector to array of coordinates
    /// </summary>
    /// <param name="vc"></param>
    /// <returns></returns>
    public static double[] ToArray(this Vector vc)
    {
      return new double[] { vc.X, vc.Y, vc.Z };
    }

    /// <summary>
    /// Array of coordinates to DS Vector
    /// </summary>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static Vector ToVector(this double[] arr)
    {
      return Vector.ByCoordinates(arr[0], arr[1], arr[2]);
    }
    #endregion

    #region Planes
    /// <summary>
    /// DS Plane to SpecklePlane
    /// </summary>
    /// <param name="plane"></param>
    /// <returns></returns>
    public static SpecklePlane ToSpeckle(this Plane plane)
    {
      return new SpecklePlane(
      plane.Origin.ToSpeckle(),
      plane.Normal.ToSpeckle(),
      plane.XAxis.ToSpeckle(),
      plane.YAxis.ToSpeckle(),
      appId,
      plane.GetSpeckleProperties());
    }

    /// <summary>
    /// SpecklePlane to DS Plane
    /// </summary>
    /// <param name="plane"></param>
    /// <returns></returns>
    public static Plane ToNative(this SpecklePlane plane)
    {
      var pln = Plane.ByOriginXAxisYAxis(
        plane.Origin.ToNative(),
        plane.Xdir.ToNative(),
        plane.Ydir.ToNative());

      return pln.SetSpeckleProperties<Plane>(plane.Properties);
    }
    #endregion

    #region Linear

    /// <summary>
    /// DS Line to SpeckleLine
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static SpeckleLine ToSpeckle(this Line line)
    {
      return new SpeckleLine(
        (new Point[] { line.StartPoint, line.EndPoint }).ToFlatArray(),
        appId,
        line.GetSpeckleProperties());
    }

    /// <summary>
    /// SpeckleLine to DS Line
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static Line ToNative(this SpeckleLine line)
    {
      var pts = line.Value.ToPoints();
      var ln = Line.ByStartPointEndPoint(pts[0], pts[1]);

      pts.ForEach(pt => pt.Dispose());

      return ln.SetSpeckleProperties<Line>(line.Properties);
    }

    /// <summary>
    /// DS Polygon to closed SpecklePolyline
    /// </summary>
    /// <param name="polygon"></param>
    /// <returns></returns>
    public static SpecklePolyline ToSpeckle(this Polygon polygon)
    {
      return new SpecklePolyline(polygon.Points.ToFlatArray(), null, polygon.GetSpeckleProperties())
      {
        Closed = true
      };
    }

    /// <summary>
    /// DS Rectangle to SpecklePolyline
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static SpecklePolyline ToSpeckle(this Rectangle rectangle)
    {
      var rect = (rectangle as Polygon).ToSpeckle();
      rect.Properties = rectangle.GetSpeckleProperties();
      return rect;
    }

    /// <summary>
    /// SpecklePolyline to DS Rectangle if closed , four points and sides parallel; 
    /// DS Polygon if closed or DS Polycurve otherwise
    /// </summary>
    /// <param name="polyline"></param>
    /// <returns></returns>
    public static Curve ToNative(this SpecklePolyline polyline)
    {
      var points = polyline.Value.ToPoints();
      if (polyline.Closed) return PolyCurve.ByPoints(points).CloseWithLine().SetSpeckleProperties<PolyCurve>(polyline.Properties);
      return PolyCurve.ByPoints(points).SetSpeckleProperties<PolyCurve>(polyline.Properties);
    }

    #endregion

    #region Curves Helper Methods

    public static bool IsLinear(this Curve curve)
    {
      try
      {
        if (curve.IsClosed) { return false; }
        //Dynamo cannot be trusted when less than 1e-6
        var extremesDistance = curve.StartPoint.DistanceTo(curve.EndPoint);
        return Threshold(curve.Length, extremesDistance);
      }
      catch (Exception e)
      {
        return false;
      }
    }

    public static Line GetAsLine(this Curve curve)
    {
      if (curve.IsClosed) { throw new ArgumentException("Curve is closed, cannot be a Line"); }
      return Line.ByStartPointEndPoint(curve.StartPoint, curve.EndPoint);
    }

    public static bool IsPolyline(this PolyCurve polycurve)
    {
      return polycurve.Curves().All(c => c.IsLinear());
    }

    public static bool IsArc(this Curve curve)
    {
      try
      {
        if (curve.IsClosed) { return false; }
        using (Point midPoint = curve.PointAtParameter(0.5))
        using (Arc arc = Arc.ByThreePoints(curve.StartPoint, midPoint, curve.EndPoint))
        {
          return Threshold(arc.Length, curve.Length);
        }
      }
      catch (Exception e)
      {
        return false;
      }
    }

    public static Arc GetAsArc(this Curve curve)
    {
      if (curve.IsClosed) { throw new ArgumentException("Curve is closed, cannot be an Arc"); }
      using (Point midPoint = curve.PointAtParameter(0.5))
      {
        return Arc.ByThreePoints(curve.StartPoint, midPoint, curve.EndPoint);
      }

    }

    public static bool IsCircle(this Curve curve)
    {
      try
      {
        if (!curve.IsClosed) { return false; }
        using (Point midPoint = curve.PointAtParameter(0.5))
        {
          double radius = curve.StartPoint.DistanceTo(midPoint) * 0.5;
          return Threshold(radius, (curve.Length) / (2 * Math.PI));
        }
      }
      catch (Exception e)
      {
        return false;
      }
    }

    public static Circle GetAsCircle(this Curve curve)
    {
      if (!curve.IsClosed) { throw new ArgumentException("Curve is not closed, cannot be a Circle"); }

      Point start = curve.StartPoint;
      using (Point midPoint = curve.PointAtParameter(0.5))
      using (Point centre = Point.ByCoordinates(Median(start.X, midPoint.X), Median(start.Y, midPoint.Y), Median(start.Z, midPoint.Z)))
      {
        return Circle.ByCenterPointRadiusNormal(
            centre,
            centre.DistanceTo(start),
            curve.Normal
        );
      }
    }

    public static bool IsEllipse(this Curve curve)
    {
      try
      {
        if (!curve.IsClosed) { return false; }

        //http://www.numericana.com/answer/ellipse.htm
        double[] parameters = new double[4] { 0, 0.25, 0.5, 0.75 };
        Point[] points = parameters.Select(p => curve.PointAtParameter(p)).ToArray();
        double a = points[0].DistanceTo(points[2]) * 0.5; // Max Radius
        double b = points[1].DistanceTo(points[3]) * 0.5; // Min Radius
        points.ForEach(p => p.Dispose());

        double h = Math.Pow(a - b, 2) / Math.Pow(a + b, 2);
        double perimeter = Math.PI * (a + b) * (1 + (3 * h / (10 + Math.Sqrt(4 - 3 * h))));

        return Threshold(curve.Length, perimeter, 1e-5); //Ellipse perimeter is an approximation
      }
      catch (Exception e)
      {
        return false;
      }
    }

    public static Ellipse GetAsEllipse(this Curve curve)
    {
      if (!curve.IsClosed) { throw new ArgumentException("Curve is not closed, cannot be an Ellipse"); }
      double[] parameters = new double[4] { 0, 0.25, 0.5, 0.75 };
      Point[] points = parameters.Select(p => curve.PointAtParameter(p)).ToArray();
      double a = points[0].DistanceTo(points[2]) * 0.5; // Max Radius
      double b = points[1].DistanceTo(points[3]) * 0.5; // Min Radius

      using (Point centre = Point.ByCoordinates(Median(points[0].X, points[2].X), Median(points[0].Y, points[2].Y), Median(points[0].Z, points[2].Z)))
      {
        points.ForEach(p => p.Dispose());

        return Ellipse.ByPlaneRadii(
            Plane.ByOriginNormalXAxis(centre, curve.Normal, Vector.ByTwoPoints(centre, curve.StartPoint)),
            a,
            b
            );
      }
    }

    #endregion

    #region Curves

    /// <summary>
    /// DS Circle to SpeckleCircle.
    /// </summary>
    /// <param name="circ"></param>
    /// <returns></returns>
    public static SpeckleCircle ToSpeckle(this Circle circ)
    {
      using (Vector xAxis = Vector.ByTwoPoints(circ.CenterPoint, circ.StartPoint))
      using (Plane plane = Plane.ByOriginNormalXAxis(circ.CenterPoint, circ.Normal, xAxis))
      {
        return new SpeckleCircle(plane.ToSpeckle(), circ.Radius, appId, circ.GetSpeckleProperties());
      }
    }

    /// <summary>
    /// SpeckleCircle to DS Circle. Rotating the circle is due to a bug in ProtoGeometry
    /// that will be solved on Dynamo 2.1.
    /// </summary>
    /// <param name="circ"></param>
    /// <returns></returns>
    public static Circle ToNative(this SpeckleCircle circ)
    {
      using (Plane basePlane = circ.Plane.ToNative())
      using (Circle preCircle = Circle.ByPlaneRadius(basePlane, circ.Radius.Value))
      using (Vector preXvector = Vector.ByTwoPoints(preCircle.CenterPoint, preCircle.StartPoint))
      {
        double angle = preXvector.AngleAboutAxis(basePlane.XAxis, basePlane.Normal);
        var circle = (Circle)preCircle.Rotate(basePlane, angle);

        return circle.SetSpeckleProperties<Circle>(circ.Properties);
      }
    }

    /// <summary>
    /// DS Arc to SpeckleArc
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static SpeckleArc ToSpeckle(this Arc a)
    {
      using (Vector xAxis = Vector.ByTwoPoints(a.CenterPoint, a.StartPoint))
      using (Plane basePlane = Plane.ByOriginNormalXAxis(a.CenterPoint, a.Normal, xAxis))
      {
        return new SpeckleArc(
            basePlane.ToSpeckle(),
            a.Radius,
            0, // This becomes 0 as arcs are interpreted to start from the plane's X axis.
            a.SweepAngle.ToRadians(),
            a.SweepAngle.ToRadians(),
            appId,
            a.GetSpeckleProperties());
      }
    }

    /// <summary>
    /// SpeckleArc to DS Arc
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Arc ToNative(this SpeckleArc a)
    {
      using (Plane basePlane = a.Plane.ToNative())
      using (Point startPoint = (Point)basePlane.Origin.Translate(basePlane.XAxis, a.Radius.Value))
      {
        var arc = Arc.ByCenterPointStartPointSweepAngle(
            basePlane.Origin,
            startPoint,
            a.AngleRadians.Value.ToDegrees(),
            basePlane.Normal
          );
        return arc.SetSpeckleProperties<Arc>(a.Properties);
      }
    }

    /// <summary>
    /// DS Ellipse to SpeckleEllipse
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static SpeckleEllipse ToSpeckle(this Ellipse e)
    {
      using (Plane basePlane = Plane.ByOriginNormalXAxis(e.CenterPoint, e.Normal, e.MajorAxis))
      {
        return new SpeckleEllipse(
          basePlane.ToSpeckle(),
          e.MajorAxis.Length,
          e.MinorAxis.Length,
          appId,
          e.GetSpeckleProperties());
      }
    }

    /// <summary>
    /// SpeckleEllipse to DS Ellipse
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static Ellipse ToNative(this SpeckleEllipse e)
    {
      var ellipse = Ellipse.ByPlaneRadii(
          e.Plane.ToNative(),
          e.FirstRadius.Value,
          e.SecondRadius.Value
      );
      return ellipse.SetSpeckleProperties<Ellipse>(e.Properties);
    }

    /// <summary>
    /// DS EllipsArc to SpeckleCurve?????
    /// </summary>
    /// <param name="arc"></param>
    /// <returns></returns>
    public static SpeckleObject ToSpeckle(this EllipseArc arc)
    {
      //EllipseArcs as NurbsCurves
      using (NurbsCurve nurbsCurve = arc.ToNurbsCurve())
      {
        var nurbs = nurbsCurve.ToSpeckle();
        nurbs.Properties = arc.GetSpeckleProperties();
        return nurbs;
      }
    }

    //public static EllipseArc ToNative(this SpeckleCurve arc)
    //{
    //  //TODO: Implement EllipseArc converter
    //  throw new NotImplementedException("EllipsArc not implemented yet.");
    //}

    /// <summary>
    /// DS Polycurve to SpecklePolyline if all curves are linear
    /// SpecklePolycurve otherwise
    /// </summary>
    /// <param name="polycurve"></param>
    /// <returns name="speckleObject"></returns>
    public static SpeckleObject ToSpeckle(this PolyCurve polycurve)
    {
      if (polycurve.IsPolyline())
      {
        var points = polycurve.Curves().SelectMany(c => c.StartPoint.ToArray()).ToList();
        points.AddRange(polycurve.Curves().Last().EndPoint.ToArray());
        return new SpecklePolyline(
          points,
          appId,
          polycurve.GetSpeckleProperties());
      }
      else
      {
        SpecklePolycurve spkPolycurve = new SpecklePolycurve();
        spkPolycurve.Properties = polycurve.GetSpeckleProperties();
        spkPolycurve.Segments = polycurve.Curves().Select(c => c.ToSpeckle()).ToList();
        spkPolycurve.GenerateHash();
        return spkPolycurve;
      }
    }

    public static PolyCurve ToNative(this SpecklePolycurve polycurve)
    {
      Curve[] curves = new Curve[polycurve.Segments.Count];
      for (var i = 0; i < polycurve.Segments.Count; i++)
      {
        switch (polycurve.Segments[i])
        {
          case SpeckleLine curve:
            curves[i] = curve.ToNative();
            break;
          case SpeckleArc curve:
            curves[i] = curve.ToNative();
            break;
          case SpeckleCircle curve:
            curves[i] = curve.ToNative();
            break;
          case SpeckleEllipse curve:
            curves[i] = curve.ToNative();
            break;
          case SpecklePolycurve curve:
            curves[i] = curve.ToNative();
            break;
          case SpecklePolyline curve:
            curves[i] = curve.ToNative();
            break;
          case SpeckleCurve curve:
            curves[i] = curve.ToNative();
            break;
        }
      }
      var polyCrv = PolyCurve.ByJoinedCurves(curves);
      return polyCrv.SetSpeckleProperties<PolyCurve>(polycurve.Properties);
    }

    public static SpeckleObject ToSpeckle(this Curve curve)
    {
      SpeckleObject speckleCurve;
      if (curve.IsLinear())
      {
        using (Line line = curve.GetAsLine()) { speckleCurve = line.ToSpeckle(); }
      }
      else if (curve.IsArc())
      {
        using (Arc arc = curve.GetAsArc()) { speckleCurve = arc.ToSpeckle(); }
      }
      else if (curve.IsCircle())
      {
        using (Circle circle = curve.GetAsCircle()) { speckleCurve = circle.ToSpeckle(); }
      }
      else if (curve.IsEllipse())
      {
        using (Ellipse ellipse = curve.GetAsEllipse()) { speckleCurve = ellipse.ToSpeckle(); }
      }
      else
      {
        speckleCurve = curve.ToNurbsCurve().ToSpeckle();
      }

      speckleCurve.Properties = curve.GetSpeckleProperties();
      return speckleCurve;
    }

    public static NurbsCurve ToNative(this SpeckleCurve curve)
    {
      var points = curve.Points.ToPoints();
      var dsKnots = curve.Knots;
      dsKnots.Insert(0, dsKnots.First());
      dsKnots.Add(dsKnots.Last());

      NurbsCurve nurbsCurve = NurbsCurve.ByControlPointsWeightsKnots(
          points,
          curve.Weights.ToArray(),
          dsKnots.ToArray(),
          curve.Degree
          );

      return nurbsCurve.SetSpeckleProperties<NurbsCurve>(curve.Properties);
    }

    public static SpeckleObject ToSpeckle(this NurbsCurve curve)
    {
      SpeckleObject speckleCurve;
      if (curve.IsLinear())
      {
        using (Line line = curve.GetAsLine()) { speckleCurve = line.ToSpeckle(); }
      }
      else if (curve.IsArc())
      {
        using (Arc arc = curve.GetAsArc()) { speckleCurve = arc.ToSpeckle(); }
      }
      else if (curve.IsCircle())
      {
        using (Circle circle = curve.GetAsCircle()) { speckleCurve = circle.ToSpeckle(); }
      }
      else if (curve.IsEllipse())
      {
        using (Ellipse ellipse = curve.GetAsEllipse()) { speckleCurve = ellipse.ToSpeckle(); }
      }
      else
      {
        // SpeckleCurve DisplayValue
        Curve[] curves = curve.ApproximateWithArcAndLineSegments();
        List<double> polylineCoordinates = curves.SelectMany(c => new Point[2] { c.StartPoint, c.EndPoint }.ToFlatArray()).ToList();
        polylineCoordinates.AddRange(curves.Last().EndPoint.ToArray());
        curves.ForEach(c => c.Dispose());

        SpecklePolyline displayValue = new SpecklePolyline(polylineCoordinates);
        List<double> dsKnots = curve.Knots().ToList();
        dsKnots.RemoveAt(dsKnots.Count - 1);
        dsKnots.RemoveAt(0);

        SpeckleCurve spkCurve = new SpeckleCurve(displayValue);
        spkCurve.Weights = curve.Weights().ToList();
        spkCurve.Points = curve.ControlPoints().ToFlatArray().ToList();
        spkCurve.Knots = dsKnots;
        spkCurve.Degree = curve.Degree;
        spkCurve.Periodic = curve.IsPeriodic;
        spkCurve.Rational = curve.IsRational;
        spkCurve.Closed = curve.IsClosed;
        spkCurve.Domain = new SpeckleInterval(curve.StartParameter(), curve.EndParameter());
        //spkCurve.Properties

        spkCurve.GenerateHash();

        speckleCurve = spkCurve;
      }
      speckleCurve.Properties = curve.GetSpeckleProperties();
      return speckleCurve;

    }

    public static SpeckleObject ToSpeckle(this Helix helix)
    {
      using (NurbsCurve nurbsCurve = helix.ToNurbsCurve())
      {
        var curve = nurbsCurve.ToSpeckle();
        curve.Properties = helix.GetSpeckleProperties();
        return curve;
      }
    }

    #endregion
    // Brep fallback

    public static Mesh ToNative(this SpeckleBrep brep)
    {
      if (brep.DisplayValue != null)
      {
        return brep.DisplayValue.ToNative();
      }
      return null;
    }

    // Meshes
    public static SpeckleMesh ToSpeckle(this Mesh mesh)
    {
      var vertices = mesh.VertexPositions.ToFlatArray();
      var defaultColour = System.Drawing.Color.FromArgb(255, 100, 100, 100);

      var faces = mesh.FaceIndices.SelectMany(f =>
     {
       if (f.Count == 4) { return new int[5] { 1, (int)f.A, (int)f.B, (int)f.C, (int)f.D }; }
       else { return new int[4] { 0, (int)f.A, (int)f.B, (int)f.C }; }
     })
      .ToArray();

      var colors = Enumerable.Repeat(defaultColour.ToArgb(), vertices.Count()).ToArray();
      //double[] textureCoords;

      //if (SpeckleRhinoConverter.AddMeshTextureCoordinates)
      //{
      //  textureCoords = mesh.TextureCoordinates.Select(pt => pt).ToFlatArray();
      //  return new SpeckleMesh(vertices, faces, Colors, textureCoords, properties: mesh.UserDictionary.ToSpeckle());
      //}
     
      var speckleMesh = new SpeckleMesh(vertices, faces, colors, null, appId, mesh.GetSpeckleProperties());
      
      return speckleMesh;
    }

    public static Mesh ToNative(this SpeckleMesh mesh)
    {
      var points = mesh.Vertices.ToPoints();
      List<IndexGroup> faces = new List<IndexGroup>();
      int i = 0;

      while (i < mesh.Faces.Count)
      {
        if (mesh.Faces[i] == 0)
        { // triangle
          var ig = IndexGroup.ByIndices((uint)mesh.Faces[i + 1], (uint)mesh.Faces[i + 2], (uint)mesh.Faces[i + 3]);
          faces.Add(ig);
          i += 4;
        }
        else
        { // quad
          var ig = IndexGroup.ByIndices((uint)mesh.Faces[i + 1], (uint)mesh.Faces[i + 2], (uint)mesh.Faces[i + 3], (uint)mesh.Faces[i + 4]);
          faces.Add(ig);
          i += 5;
        }
      }

      var dsMesh = Mesh.ByPointsFaceIndices(points, faces);
      if (mesh.Properties != null)
      {
        dsMesh.SetSpeckleProperties<Mesh>(mesh.Properties);
      }
      return dsMesh;
    }
  }
}
