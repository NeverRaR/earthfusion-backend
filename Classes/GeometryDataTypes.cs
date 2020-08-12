namespace GeometryDataTypes
{
    public enum GeometryDataType
    {
        // This table does not contain geometry data(geom column).
        NonGeometryData = -1,
        // This table contains geometry data(geom column), but I don't know what it is.
        Unknown = 0,
        // This table contains geometry data(geom column), and it's about line data of something or boundary of one entity.
        LineOrBoundary = 1,
        // This table contains geometry data(geom column), and it's about an entity.
        Entity = 2
    }
    public enum LineOrBoundaryType
    {
        // This is not a line or boundary data.
        NonLineOrBoundary = -1,
        // It's remain unknown to me what those are.
        Unknown = 0,
        // Land or continent
        LandBoundary = 1,
        // Ocean
        OceanBoundary = 2,
        // Natural scenes(?)
        // like ountains, lakes, etc.
        GeoTopoBoundary = 3,
        // Country level of boundary.
        CountryBoundary = 4,
        // Provincial level of boundary.
        ProvinceBoundary = 5,
        // City level of boundary.
        CityBoundary = 6,
        // County level of boundary.
        CountyBoundary = 7,
        // Roadline.
        RoadLine = 8,
        // Maybe the route of public transportation
        PublicTransportationLine = 9
    }
    public enum EntityType
    {
        // This is not an entity data.
        NonEntity = -1,
        // It's remain unknown to me what those are.
        Unknown = 0,
        // This is a building.
        Building = 1,
        // This is a public transportation stop(?)
        PublicTransportation = 2,
        // This is a store.
        Store = 3,
    }
}
