﻿using FluentAssertions;
using GShark.Core;
using GShark.ExtendedMethods;
using GShark.Geometry;
using GShark.Geometry.Enum;
using GShark.Operation;
using GShark.Operation.Enum;
using GShark.Test.XUnit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace GShark.Test.XUnit.Geometry
{
    public class NurbsSurfaceTests
    {
        private readonly ITestOutputHelper _testOutput;
        public NurbsSurfaceTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void It_Returns_A_NURBS_Surface_By_Four_Points()
        {
            // Arrange
            Point3 p1 = new Point3(0.0, 0.0, 0.0);
            Point3 p2 = new Point3(10.0, 0.0, 0.0);
            Point3 p3 = new Point3(10.0, 10.0, 2.0);
            Point3 p4 = new Point3(0.0, 10.0, 4.0);

            Point3 expectedPt = new Point3(5.0, 5.0, 1.5);

            // Act
            NurbsSurface surface = NurbsSurface.CreateFromCorners(p1, p2, p3, p4);
            Point3 evalPt = new Point3(Evaluation.SurfacePointAt(surface, 0.5, 0.5));

            // Assert
            surface.Should().NotBeNull();
            surface.LocationPoints.Count.Should().Be(2);
            surface.LocationPoints[0].Count.Should().Be(2);
            surface.LocationPoints[0][1].Equals(p4).Should().BeTrue();
            evalPt.EpsilonEquals(expectedPt, GeoSharkMath.MinTolerance).Should().BeTrue();
        }

        [Theory]
        [InlineData(0.1, 0.1, new double[] { -0.020397, -0.392974, 0.919323 })]
        [InlineData(0.5, 0.5, new double[] { 0.091372, -0.395944, 0.913717 })]
        [InlineData(1.0, 1.0, new double[] { 0.507093, -0.169031, 0.845154 })]
        public void It_Returns_The_Surface_Normal_At_A_Given_U_And_V_Parameter(double u, double v, double[] pt)
        {
            // Assert
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            Vector3 expectedNormal = new Vector3(pt[0], pt[1], pt[2]);

            // Act
            Vector3 normal = surface.EvaluateAt(u, v, EvaluateSurfaceDirection.Normal);

            // Assert
            normal.EpsilonEquals(expectedNormal, GeoSharkMath.MinTolerance).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_The_Evaluated_Surface_At_A_Given_U_And_V_Parameter()
        {
            // Assert
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            Vector3 expectedUDirection = new Vector3(0.985802, 0.152837, 0.069541);
            Vector3 expectedVDirection = new Vector3(0.053937, 0.911792, 0.407096);

            // Act
            Vector3 uDirection = surface.EvaluateAt(0.3, 0.5, EvaluateSurfaceDirection.U);
            Vector3 vDirection = surface.EvaluateAt(0.3, 0.5, EvaluateSurfaceDirection.V);

            // Assert
            uDirection.EpsilonEquals(expectedUDirection, GeoSharkMath.MinTolerance).Should().BeTrue();
            vDirection.EpsilonEquals(expectedVDirection, GeoSharkMath.MinTolerance).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_The_Surface_Split_At_The_Given_Parameter_At_V_Direction()
        {
            // Arrange
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            List<List<Point3>> surfacePtsLeft = new List<List<Point3>>
            {
                new List<Point3>{ new Point3(0.0, 0.0, 0.0), new Point3(0.0, 5.0, 2.0)},
                new List<Point3>{ new Point3(5.0, 0.0, 0.0), new Point3(5.0,6.666666,3.333333)},
                new List<Point3>{ new Point3(10.0, 0.0, 0.0), new Point3(10.0, 5.0, 1.0)}
            };

            List<List<Point3>> surfacePtsRight = new List<List<Point3>>
            {
                new List<Point3>{ new Point3(0.0, 5.0, 2.0), new Point3(0.0, 10.0, 4.0)},
                new List<Point3>{ new Point3(5.0, 6.666666, 3.333333), new Point3(5.0,10.0,5.0)},
                new List<Point3>{ new Point3(10.0, 5.0, 1.0), new Point3(10.0, 10.0, 2.0)}
            };

            List<List<double>> weightsLeft = new List<List<double>>
            {
                new List<double>{ 1, 1},
                new List<double>{ 1, 1.5},
                new List<double>{ 1, 1}
            };

            List<List<double>> weightsRight = new List<List<double>>
            {
                new List<double>{ 1, 1},
                new List<double>{ 1.5, 2},
                new List<double>{ 1, 1}
            };

            Point3 expectedPtLeft = new Point3(5.0, 3.333333, 1.444444);
            Point3 expectedPtRight = new Point3(5.0, 8.181818, 3.545455);

            // Act
            NurbsSurface[] surfaces = surface.Split(0.5, SplitDirection.V);
            Point3 evaluatePtLeft = surfaces[0].PointAt(0.5, 0.5);
            Point3 evaluatePtRight = surfaces[1].PointAt(0.5, 0.5);

            // Assert
            evaluatePtLeft.DistanceTo(expectedPtLeft).Should().BeLessThan(GeoSharkMath.MaxTolerance);
            evaluatePtRight.DistanceTo(expectedPtRight).Should().BeLessThan(GeoSharkMath.MaxTolerance);

            surfaces[0].Weights.Should().BeEquivalentTo(weightsLeft);
            surfaces[1].Weights.Should().BeEquivalentTo(weightsRight);

            _ = surfaces[0].LocationPoints.Select((pts, i) => pts.Select((pt, j) =>
                pt.EpsilonEquals(surfacePtsLeft[i][j], GeoSharkMath.MaxTolerance).Should().BeTrue()));
            _ = surfaces[1].LocationPoints.Select((pts, i) => pts.Select((pt, j) =>
                pt.EpsilonEquals(surfacePtsRight[i][j], GeoSharkMath.MaxTolerance).Should().BeTrue()));
        }

        [Fact]
        public void It_Returns_The_Surface_Split_At_The_Given_Parameter_At_U_Direction()
        {
            // Arrange
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            List<List<Point3>> surfacePtsTop = new List<List<Point3>>
            {
                new List<Point3>{ new Point3(0.0, 0.0, 0.0), new Point3(0.0, 10.0, 4.0) },
                new List<Point3>{ new Point3(2.5, 0.0, 0.0), new Point3(3.333333, 10.0,4.666666)},
                new List<Point3>{ new Point3(5.0, 0.0, 0.0), new Point3(5.0, 10.0, 4.333333)}
            };

            List<List<Point3>> surfacePtsBottom = new List<List<Point3>>
            {
                new List<Point3>{ new Point3(5.0, 0.0, 0.0), new Point3(5.0, 10.0, 4.333333)},
                new List<Point3>{ new Point3(7.5, 0.0, 0.0), new Point3(6.666666, 10.0, 4.0) },
                new List<Point3>{ new Point3(10.0, 0.0, 0.0), new Point3(10.0, 10.0, 2.0)}
            };

            List<List<double>> weightsTop = new List<List<double>>
            {
                new List<double>{ 1, 1},
                new List<double>{ 1, 1.5},
                new List<double>{ 1, 1.5}
            };

            List<List<double>> weightsBottom = new List<List<double>>
            {
                new List<double>{ 1, 1.5},
                new List<double>{ 1, 1.5},
                new List<double>{ 1, 1}
            };

            Point3 expectedPtTop = new Point3(2.894737, 5.789474, 2.578947);
            Point3 expectedPtBottom = new Point3(7.105263, 5.789474, 2.157895);

            // Act
            NurbsSurface[] surfaces = surface.Split(0.5, SplitDirection.U);
            Point3 evaluatePtTop = surfaces[0].PointAt(0.5, 0.5);
            Point3 evaluatePtBottom = surfaces[1].PointAt(0.5, 0.5);

            // Assert
            evaluatePtTop.DistanceTo(expectedPtTop).Should().BeLessThan(GeoSharkMath.MaxTolerance);
            evaluatePtBottom.DistanceTo(expectedPtBottom).Should().BeLessThan(GeoSharkMath.MaxTolerance);

            surfaces[0].Weights.Should().BeEquivalentTo(weightsTop);
            surfaces[1].Weights.Should().BeEquivalentTo(weightsBottom);

            _ = surfaces[0].LocationPoints.Select((pts, i) => pts.Select((pt, j) =>
                pt.EpsilonEquals(surfacePtsTop[i][j], GeoSharkMath.MaxTolerance).Should().BeTrue()));
            _ = surfaces[1].LocationPoints.Select((pts, i) => pts.Select((pt, j) =>
                pt.EpsilonEquals(surfacePtsBottom[i][j], GeoSharkMath.MaxTolerance).Should().BeTrue()));
        }

        [Fact]
        public void It_Returns_The_Surface_Split_At_The_Given_Parameter_At_Both_Direction()
        {
            // Arrange
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            List<Point3> expectedPts = new List<Point3>
            {
                new Point3(2.714286, 3.142857, 1.4),
                new Point3(7.285714, 3.142857, 1.171429),
                new Point3(3.04878, 8.04878, 3.585366),
                new Point3(6.95122, 8.04878, 3)
            };

            // Act
            NurbsSurface[] splitSrf = surface.Split(0.5, SplitDirection.Both);
            var ptsAt = splitSrf.Select(s => s.PointAt(0.5, 0.5));

            // Assert
            splitSrf.Length.Should().Be(4);
            _ = ptsAt.Select((pt, i) => pt.DistanceTo(expectedPts[i]).Should().BeLessThan(GeoSharkMath.MaxTolerance));
        }

        [Theory]
        [InlineData(-0.2)]
        [InlineData(1.3)]
        public void Split_Surface_Throws_An_Exception_If_Parameter_Is_Outside_The_Domain(double parameter)
        {
            // Arrange
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();

            // Act
            Func<object> func = () => surface.Split(parameter, SplitDirection.U);

            // Assert
            func.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(new double[] { 2.60009, 7.69754, 3.408162 }, new double[] { 2.5, 7, 5 })]
        [InlineData(new double[] { 2.511373, 1.994265, 0.887211 }, new double[] { 2.5, 1.5, 2 })]
        [InlineData(new double[] { 8.952827, 2.572942, 0.735217 }, new double[] { 9, 2.5, 1 })]
        [InlineData(new double[] { 5.073733, 4.577509, 1.978153 }, new double[] { 5, 5, 1 })]
        public void Returns_The_Closest_Point_On_The_Surface(double[] expectedPt, double[] testPt)
        {
            // Arrange
            NurbsSurface surface = NurbsSurfaceCollection.SurfaceFromPoints();
            Point3 pt = new Point3(testPt[0], testPt[1], testPt[2]);
            Point3 expectedClosestPt = new Point3(expectedPt[0], expectedPt[1], expectedPt[2]);

            // Act
            Point3 closestPt = surface.ClosestPoint(pt);

            // Assert
            closestPt.DistanceTo(expectedClosestPt).Should().BeLessThan(GeoSharkMath.MaxTolerance);
        }
    }
}
