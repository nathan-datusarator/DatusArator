using DatusArator.Core.Json;
using System;

namespace DatusArator.HTML.Turo {
  public class TuroEntry {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public string Year { get; set; }

    public decimal Rate { get; set; }
    public int? Trips { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewCount { get; set; }

    public decimal? EstValue { get; set; }

    public string DetailsUrl { get; set; }
    public string ImageUrl { get; set; }
    public string OriginalImageUrl { get; set; }

    public DateTime? CreateDate { get; set; }

    public string City { get; set; }
    public string State { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }

    public int HostId { get; set; }

    public decimal? DailyRate { get; set; }
    public decimal? MonthlyRate { get; set; }
    public decimal? WeeklyRate { get; set; }

    public static TuroEntry Populate(JsonWrapper item) {
      var result = new TuroEntry();

      result.Id = item.GetAsInt("vehicle.id", 0).Value;
      result.Name = item.Get("vehicle.name");
      result.Make = item.Get("vehicle.make");
      result.Model = item.Get("vehicle.model");
      result.Year = item.Get("vehicle.year");

      result.Trips = item.GetAsInt("renterTripsTaken", null);
      result.Rating = item.GetAsDecimal("rating", null);
      result.Rate = item.GetAsDecimal("rate.averageDailyPrice", -1).Value;
      result.ReviewCount = item.GetAsInt("reviewCount", null);

      result.DetailsUrl = item.Get("vehicle.url");
      result.ImageUrl = item.Get("vehicle.image.thumbnails.620x372");
      if (string.IsNullOrEmpty(result.ImageUrl))
        result.ImageUrl = item.Get("vehicle.image.thumbnails.574x343");
      if (string.IsNullOrEmpty(result.ImageUrl))
        result.ImageUrl = item.Get("vehicle.image.thumbnails.100x60");
      result.OriginalImageUrl = item.Get("vehicle.image.originalImageUrl");

      result.City = item.Get("location.city");
      result.State = item.Get("location.state");
      result.Lat = item.GetAsDecimal("location.latitude", null);
      result.Lng = item.GetAsDecimal("location.longitude", null);

      result.CreateDate = item.GetAsDate("vehicle.listingCreatedTime");

      result.HostId = item.GetAsInt("owner.id", 0).Value;

      result.DailyRate = item.GetAsDecimal("rate.daily", -1).Value;
      result.WeeklyRate = item.GetAsDecimal("rate.monthly", -1).Value;
      result.MonthlyRate = item.GetAsDecimal("rate.weekly", -1).Value;


      return result;
    }

    public void LookupEstValue() {
      EstValue = CarGurusService.LookupValue(Make, Model, Year);
    }
  }
}
