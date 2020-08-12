using System;
using System.Collections.Generic;
using GeometryDataTypes;

namespace OracleTest
{
    public class KnownTables
    {
        public KnownTables()
        {
            AddAllTables();
        }
        private void AddFrom(string tableName, GeometryDataType geomTypeId, LineOrBoundaryType lineOrBoundaryTypeId, EntityType entityTypeId)
        {
            this.knownTableType.Add(tableName, new GeomTableTypes{
                GeomTypeId = geomTypeId,
                LineOrBoundaryTypeId = lineOrBoundaryTypeId,
                EntityTypeId = entityTypeId
            });
        }
        private void AddAllTables()
        {
            foreach ((string, GeometryDataType, LineOrBoundaryType, EntityType) temp in this.list)
            {
                AddFrom(temp.Item1, temp.Item2, temp.Item3, temp.Item4);
            }
        }
        private List<(string, GeometryDataType, LineOrBoundaryType, EntityType)> list = new List<(string, GeometryDataType, LineOrBoundaryType, EntityType)>
        {
            ("NE_10M_LAND", GeometryDataType.LineOrBoundary, LineOrBoundaryType.LandBoundary, EntityType.NonEntity),
            ("NE_10M_OCEAN", GeometryDataType.LineOrBoundary, LineOrBoundaryType.OceanBoundary, EntityType.NonEntity),
            ("GIS_OSM_TRANSPORT_A_FREE_1", GeometryDataType.Entity, LineOrBoundaryType.NonLineOrBoundary, EntityType.PublicTransportation),
            ("GIS_OSM_TRANSPORT_FREE_1", GeometryDataType.Entity, LineOrBoundaryType.NonLineOrBoundary, EntityType.PublicTransportation),
            ("PLACES_OF_PREFECTURE_LEVEL_CITY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.CityBoundary, EntityType.NonEntity),
            ("COUNTRY_BOUNDRY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.CountryBoundary, EntityType.NonEntity),
            ("GRATICULE", GeometryDataType.LineOrBoundary, LineOrBoundaryType.Unknown, EntityType.NonEntity),
            ("STATS_ON_COUNTY_LEVEL", GeometryDataType.Unknown, LineOrBoundaryType.Unknown, EntityType.Unknown),
            ("PROVINCIAL_CAPITAL_CITY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.CityBoundary, EntityType.NonEntity),
            ("PROVINCIAL_REGION", GeometryDataType.LineOrBoundary, LineOrBoundaryType.ProvinceBoundary, EntityType.NonEntity),
            ("CHINA_COUNTY_BOUNDRY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.CountyBoundary, EntityType.NonEntity),
            ("PLACES_OF_COUNTY_LEVEL", GeometryDataType.Entity, LineOrBoundaryType.NonLineOrBoundary, EntityType.Unknown),
            ("PROVINCIAL_BOUNDRY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.ProvinceBoundary, EntityType.NonEntity),
            ("COUNTY_BOUNDRY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.CountyBoundary, EntityType.NonEntity),
            ("CHINA_PLACES_BOUNDRY", GeometryDataType.LineOrBoundary, LineOrBoundaryType.Unknown, EntityType.NonEntity),
            ("CHINA_ROAD_NETWORK", GeometryDataType.LineOrBoundary, LineOrBoundaryType.RoadLine, EntityType.NonEntity),

        };
        public Dictionary<string, GeomTableTypes> knownTableType = new Dictionary<string, GeomTableTypes>();
    }
}