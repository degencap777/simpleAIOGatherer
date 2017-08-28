using Ennui.Api;
using Ennui.Api.Util;

namespace Ennui.Script.Official
{
    public static class AreaExtensions
    {
        public static void Render(this IArea<float> area, IApi api, Color col, Color fill)
        {
            var leftBottomBack = api.Game.ProjectToScreen(area.Start);
            var leftBottomFront = api.Game.ProjectToScreen( area.Start.Translate(0, 0, area.End.Z - area.Start.Z));
            var rightBottomBack = api.Game.ProjectToScreen( area.Start.Translate(area.End.X - area.Start.X, 0, 0));
            var rightBottomFront = api.Game.ProjectToScreen( area.End.Translate(0, area.Start.Y - area.End.Y, 0));
            var rightTopFront = api.Game.ProjectToScreen( area.End);
            var rightTopBack = api.Game.ProjectToScreen( area.End.Translate(0, 0, area.Start.Z - area.End.Z));
            var leftTopFront = api.Game.ProjectToScreen( area.End.Translate(area.Start.X - area.End.X, 0, 0));
            var leftTopBack = api.Game.ProjectToScreen( area.Start.Translate(0, area.End.Y - area.Start.Y, 0));

            if (leftBottomBack != null && leftBottomFront != null)
            {
                api.Rendering.DrawLine(col, leftBottomBack, leftBottomFront);
            }

            if (leftTopBack != null && leftTopFront != null)
            {
                api.Rendering.DrawLine(col, leftTopBack, leftTopFront);
            }

            if (rightBottomBack != null && rightBottomFront != null)
            {
                api.Rendering.DrawLine(col, rightBottomBack, rightBottomFront);
            }

            if (rightTopBack != null && rightTopFront != null)
            {
                api.Rendering.DrawLine(col, rightTopBack, rightTopFront);
            }

            if (leftBottomBack != null && rightBottomBack != null)
            {
                api.Rendering.DrawLine(col, leftBottomBack, rightBottomBack);
            }

            if (leftTopBack != null && rightTopBack != null)
            {
                api.Rendering.DrawLine(col, leftTopBack, rightTopBack);
            }

            if (leftBottomFront != null && rightBottomFront != null)
            {
                api.Rendering.DrawLine(col, leftBottomFront, rightBottomFront);
            }

            if (leftTopFront != null && rightTopFront != null)
            {
                api.Rendering.DrawLine(col, leftTopFront, rightTopFront);
            }

            if (leftBottomFront != null && leftTopFront != null)
            {
                api.Rendering.DrawLine(col, leftBottomFront, leftTopFront);
            }

            if (rightBottomFront != null && rightTopFront != null)
            {
                api.Rendering.DrawLine(col, rightBottomFront, rightTopFront);
            }

            if (leftBottomBack != null && leftTopBack != null)
            {
                api.Rendering.DrawLine(col, leftBottomBack, leftTopBack);
            }

            if (rightBottomBack != null && rightTopBack != null)
            {
                api.Rendering.DrawLine(col, rightBottomBack, rightTopBack);
            }

            //back
            if (leftBottomBack != null && leftTopBack != null && rightTopBack != null && rightBottomBack != null)
            {
                api.Rendering.FillPolygon(fill, leftBottomBack, leftTopBack, rightTopBack, rightBottomBack);
            }

            //front
            if (leftBottomFront != null && leftTopFront != null && rightTopFront != null && rightBottomFront != null)
            {
                api.Rendering.FillPolygon(fill, leftBottomFront, leftTopFront, rightTopFront, rightBottomFront);
            }

            //left
            if (leftBottomFront != null && leftTopFront != null && leftTopBack != null && leftBottomBack != null)
            {
                api.Rendering.FillPolygon(fill, leftBottomFront, leftTopFront, leftTopBack, leftBottomBack);
            }

            //right
            if (rightBottomFront != null && rightTopFront != null && rightTopBack != null && rightBottomBack != null)
            {
                api.Rendering.FillPolygon(fill, rightBottomFront, rightTopFront, rightTopBack, rightBottomBack);
            }

            //top
            if (leftTopBack != null && leftTopFront != null && rightTopFront != null && rightTopBack != null)
            {
                api.Rendering.FillPolygon(fill, leftTopBack, leftTopFront, rightTopFront, rightTopBack);
            }

            //bottom
            if (leftBottomBack != null && leftBottomFront != null && rightBottomFront != null && rightBottomBack != null)
            {
                api.Rendering.FillPolygon(fill, leftBottomBack, leftBottomFront, rightBottomFront, rightBottomBack);
            }
        }
    }
}
