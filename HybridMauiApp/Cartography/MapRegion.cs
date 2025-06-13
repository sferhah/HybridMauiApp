namespace HybridMauiApp.Cartography;

public class MapRegion
{
    public MauiPosition NearLeft { get; set; }
    public MauiPosition FarRight { get; set; }    

    public MauiPosition Center
    {
        get
        {
            double lat1 = NearLeft.Latitude;
            double lon1 = NearLeft.Longitude;
            double lat2 = FarRight.Latitude;
            double lon2 = FarRight.Longitude;

            // Calculate average latitude
            double avgLat = (lat1 + lat2) / 2;

            // Handle 180th meridian crossing for longitude
            if (Math.Abs(lon1 - lon2) > 180)
            {
                if (lon1 > lon2)
                {
                    lon2 += 360;
                }
                else
                {
                    lon1 += 360;
                }
            }

            // Calculate average longitude
            double avgLon = (lon1 + lon2) / 2;

            // Normalize longitude to be within the range [-180, 180]
            if (avgLon > 180)
            {
                avgLon -= 360;
            }

            // Handle crossing the poles
            if (Math.Abs(lat1 - lat2) > 180)
            {
                if (lat1 > lat2)
                {
                    lat2 += 360;
                }
                else
                {
                    lat1 += 360;
                }
                avgLat = (lat1 + lat2) / 2;
                if (avgLat > 180)
                {
                    avgLat -= 360;
                }
            }

            return new MauiPosition(avgLat, avgLon);
        }
    }

    public double LatitudeSpan => Math.Abs(FarRight.Latitude - NearLeft.Latitude);
    public double LongitudeSpan => Math.Abs(FarRight.Longitude - NearLeft.Longitude);
}
