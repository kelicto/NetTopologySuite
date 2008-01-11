﻿using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{

    /// <summary>
    /// Computes an approximate intersection of two line segments
    /// by taking the most central of the endpoints of the segments.
    /// </summary>
    /// <remarks>
    /// This is effective in cases where the segments are nearly parallel
    /// and should intersect at an endpoint.
    /// It is also a reasonable strategy for cases where the 
    /// endpoint of one segment lies on or almost on the interior of another one.
    /// Taking the most central endpoint ensures that the computed intersection
    /// point lies in the envelope of the segments.
    /// Also, by always returning one of the input points, this should result 
    /// in reducing segment fragmentation.
    /// Intended to be used as a last resort for 
    /// computing ill-conditioned intersection situations which 
    /// cause other methods to fail.
    /// </remarks>
    public class CentralEndpointIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static TCoordinate GetIntersection(TCoordinate p00, TCoordinate p01,
                TCoordinate p10, TCoordinate p11)
        {
            CentralEndpointIntersector<TCoordinate> intersector
                = new CentralEndpointIntersector<TCoordinate>(p00, p01, p10, p11);
            return intersector.GetIntersectionPoint();
        }

        private LineSegment<TCoordinate> _line0;
        private LineSegment<TCoordinate> _line1;
        private TCoordinate _intPt;

        public CentralEndpointIntersector(TCoordinate p00, TCoordinate p01,
                TCoordinate p10, TCoordinate p11)
            : this(new LineSegment<TCoordinate>(p00, p01), new LineSegment<TCoordinate>(p10, p11))
        { }

        public CentralEndpointIntersector(LineSegment<TCoordinate> line0, LineSegment<TCoordinate> line1)
        {
            _line0 = line0;
            _line1 = line1;
            compute();
        }

        private void compute()
        {
            TCoordinate centroid = average(_line0.P0, _line0.P1, _line1.P0, _line1.P1);
            _intPt = findNearestPoint(centroid, _line0.P0, _line0.P1, _line1.P0, _line1.P1);
        }

        public TCoordinate GetIntersectionPoint()
        {
            return _intPt;
        }

        private static TCoordinate average(params TCoordinate[] pts)
        {
            if (pts.Length == 0)
	        {
	            return default(TCoordinate);
	        }

            TCoordinate first = pts[0];

            if (pts.Length == 1)
	        {
	            return first;
	        }

            Int32 componentCount = first.ComponentCount;

            Double[] avg = new Double[componentCount];

            int n = pts.Length;

            for (int i = 0; i < pts.Length; i++)
            {
                for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
                {
                    avg[componentIndex] += (Double)pts[i][componentIndex];
                }
            }

            for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
            {
                avg[componentIndex] /= n;
            }

            return Coordinates<TCoordinate>.DefaultCoordinateFactory.Create(avg);
        }

        // Determines a point closest to the given point from a set of points.
        private static TCoordinate findNearestPoint(TCoordinate p, params TCoordinate[] pts)
        {
            Double minDist = Double.MaxValue;

            TCoordinate result = default(TCoordinate);

            for (int i = 0; i < pts.Length; i++)
            {
                double dist = p.Distance(pts[i]);

                if (dist < minDist)
                {
                    minDist = dist;
                    result = pts[i];
                }
            }

            return result;
        }
    }
}
