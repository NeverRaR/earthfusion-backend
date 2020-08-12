namespace GeometryDataTypes
{
    public enum GeometryDataType
    {
        NonGeometryData = -1,
        Unknown = 0,
        LineOrBoundary = 1,
        Entity = 2
    }
    public enum LineOrBoundaryType
    {
        NonLineOrBoundary = -1,
        Unknown = 0,
        LandBoundary = 1,
        OceanBoundary = 2,
        // Mountains, lakes, etc.
        GeoTopoBoundary = 3,
        CountryBoundary = 4,
        ProvinceBoundary = 5,
        CityBoundary = 6,
        CountyBoundary = 7,
        RoadLine = 8,
        PublicTransportationLine = 9
    }
    public enum EntityType
    {
        NonEntity = -1,
        Unknown = 0,
        Building = 1,
        PublicTransportation = 2,
        Store = 3,
    }
}
