using SC2APIProtocol;

namespace Sharky.Proxy
{
    internal interface IProxyLocationService
    {
        Point2D GetCliffProxyLocation(float offsetDistance);

        Point2D GetGroundProxyLocation(float offsetDistance);
    }
}