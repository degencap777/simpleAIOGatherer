using Ennui.Api.Util;

namespace Ennui.Script.Official
{
    public class SafeArea
    {
        public SafeVector3 Start;
        public SafeVector3 End;

        public SafeArea() { }

        public SafeArea(IArea<float> area)
        {
            this.Start = new SafeVector3(area.Start);
            this.End = new SafeVector3(area.End);
        }

        public Area RealArea()
        {
            return new Area(Start.RealVector3(), End.RealVector3());
        }
    }
}
