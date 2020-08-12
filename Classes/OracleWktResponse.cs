using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using GeometryDataTypes;

namespace OracleTest
{
    public class OracleWktResponse
    {
        public OracleWktResponse()
        {
            Contents = new List<WktWithName>();
            
            // assume all tables don't have geom data.
            GeomTypeId = GeometryDataType.NonGeometryData;
            LineOrBoundaryTypeId = LineOrBoundaryType.NonLineOrBoundary;
            EntityTypeId = EntityType.NonEntity;
        }
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string TableName { get; set; }
        public string Message { get; set; }
        public GeometryDataType GeomTypeId { get; set; }
        public LineOrBoundaryType LineOrBoundaryTypeId { get; set; }
        public EntityType EntityTypeId { get; set; }
        public List<WktWithName> Contents { get; set; }
    }
}
