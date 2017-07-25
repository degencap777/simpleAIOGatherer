using Ennui.Api;

namespace Ennui.Script.Official
{
    public class SafeMapArea
    {
        public string Cluster;
        public SafeArea Area;

        public SafeMapArea() { }

        public SafeMapArea(string cluster, Area area)
        {
            this.Cluster = cluster;
            this.Area = new SafeArea(area);
        }

        public SafeMapArea(string cluster, Vector3<float> start, Vector3<float> end)
        {
            this.Cluster = cluster;
            this.Area = new SafeArea(new Area(start, end));
        }

        public bool Contains(IApi api, Vector3<float> loc)
        {
            return new MapArea(api, Cluster, Area.Start.RealVector3(), Area.End.RealVector3()).Contains(loc);
        }

        public MapArea RealArea(IApi api)
        {
            return new MapArea(api, Cluster, Area.Start.RealVector3(), Area.End.RealVector3());
        }
    }
}
