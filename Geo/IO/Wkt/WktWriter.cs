﻿using System.Runtime.Serialization;
using System.Text;
using Geo.Geometries;
using Geo.Interfaces;

namespace Geo.IO.Wkt
{
    public class WktWriter
    {
        public string Write(IGeometry geometry)
        {
            var builder = new StringBuilder();
            AppendGeometry(builder, geometry);
            return builder.ToString();
        }

        private void AppendGeometry(StringBuilder builder, IGeometry geometry)
        {
            var point = geometry as Point;
            if (point != null)
            {
                AppendPoint(builder, point);
                return;
            }

            var lineString = geometry as LineString;
            if (lineString != null)
            {
                AppendLineString(builder, lineString);
                return;
            }

            var linearRing = geometry as LinearRing;
            if (linearRing != null)
            {
                AppendLinearRing(builder, linearRing);
                return;
            }

            var polygon = geometry as Polygon;
            if (polygon != null)
            {
                AppendPolygon(builder, polygon);
                return;
            }

            var multiPoint = geometry as MultiPoint;
            if (multiPoint != null)
            {
                AppendMultiPoint(builder, multiPoint);
                return;
            }

            var multiLineString = geometry as MultiLineString;
            if (multiLineString != null)
            {
                AppendMultiLineString(builder, multiLineString);
                return;
            }

            var multiPolygon = geometry as MultiPolygon;
            if (multiPolygon != null)
            {
                AppendMultiPolygon(builder, multiPolygon);
                return;
            }

            var geometryCollection = geometry as GeometryCollection;
            if (geometryCollection != null)
            {
                AppendGeometryCollection(builder, geometryCollection);
                return;
            }

            throw new SerializationException("Geometry of type '" + geometry.GetType().Name + "' is not supported");
        
        }

        private void AppendPoint(StringBuilder builder, Point point)
        {
            builder.Append("POINT");
            AppendDimensions(builder, point);
            builder.Append(" ");
            AppendPointInner(builder, point);
        }

        private void AppendPointInner(StringBuilder builder, Point point)
        {
            if (point.IsEmpty)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
            AppendCoordinate(builder,point.Coordinate);
            builder.Append(")");
        }

        private void AppendLineString(StringBuilder builder, LineString lineString)
        {
            builder.Append("LINESTRING");
            AppendDimensions(builder, lineString);
            builder.Append(" ");
            AppendLineStringInner(builder, lineString.Coordinates);
        }

        private void AppendLineStringInner(StringBuilder builder, CoordinateSequence lineString)
        {
            if (lineString.IsEmpty)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
            AppendCoordinates(builder, lineString);
            builder.Append(")");
        }

        private void AppendLinearRing(StringBuilder builder, LinearRing linearRing)
        {
            builder.Append("LINEARRING");
            AppendDimensions(builder, linearRing);
            builder.Append(" ");
            AppendLineStringInner(builder, linearRing.Coordinates);
        }

        private void AppendPolygon(StringBuilder builder, Polygon polygon)
        {
            builder.Append("POLYGON");
            AppendDimensions(builder, polygon);
            builder.Append(" ");
            AppendPolygonInner(builder, polygon);
        }

        private void AppendPolygonInner(StringBuilder builder, Polygon polygon)
        {
            if (polygon.IsEmpty)
            {
                builder.Append("EMPTY");
                return;
            }

            builder.Append("(");
            AppendLineStringInner(builder, polygon.Shell.Coordinates);
            for (var i = 0; i < polygon.Holes.Count; i++)
            {
                builder.Append(", ");
                AppendLineStringInner(builder, polygon.Holes[i].Coordinates);
            }
            builder.Append(")");
        }

        private void AppendMultiPoint(StringBuilder builder, MultiPoint multiPoint)
        {
            builder.Append("MULTIPOINT");
            if (multiPoint.IsEmpty)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, multiPoint);
            builder.Append(" (");
            for (var i = 0; i < multiPoint.Geometries.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendPointInner(builder, multiPoint.Geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendMultiLineString(StringBuilder builder, MultiLineString multiLineString)
        {
            builder.Append("MULTILINESTRING");
            if (multiLineString.IsEmpty)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, multiLineString);
            builder.Append(" (");
            for (var i = 0; i < multiLineString.Geometries.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendLineStringInner(builder, multiLineString.Geometries[i].Coordinates);
            }
            builder.Append(")");
        }

        private void AppendMultiPolygon(StringBuilder builder, MultiPolygon multiPolygon)
        {
            builder.Append("MULTIPOLYGON");
            if (multiPolygon.IsEmpty)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, multiPolygon);
            builder.Append(" (");
            for (var i = 0; i < multiPolygon.Geometries.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendPolygonInner(builder, multiPolygon.Geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendGeometryCollection(StringBuilder builder, GeometryCollection geometryCollection)
        {
            builder.Append("GEOMETRYCOLLECTION");
            if (geometryCollection.IsEmpty)
            {
                builder.Append(" EMPTY");
                return;
            }

            AppendDimensions(builder, geometryCollection);
            builder.Append(" (");
            for (var i = 0; i < geometryCollection.Geometries.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendGeometry(builder, geometryCollection.Geometries[i]);
            }
            builder.Append(")");
        }

        private void AppendDimensions(StringBuilder builder, IGeometry geometry)
        {
            if (geometry.HasElevation || geometry.HasM)
                builder.Append(" ");

            if (geometry.HasElevation)
                builder.Append("Z");

            if (geometry.HasM)
                builder.Append("M");
        }

        private void AppendCoordinates(StringBuilder builder, CoordinateSequence coordinates)
        {
            for (var i = 0; i < coordinates.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                AppendCoordinate(builder, coordinates[i]);
            }
        }

        private void AppendCoordinate(StringBuilder builder, Coordinate coordinate)
        {
            builder.Append(coordinate.Longitude);
            builder.Append(" ");
            builder.Append(coordinate.Latitude);

            if (coordinate.HasElevation)
            {
                builder.Append(" ");
                builder.Append(coordinate.Elevation);
            }

            if (coordinate.HasM)
            {
                builder.Append(" ");
                builder.Append(coordinate.M);
            }
        }
    }
}