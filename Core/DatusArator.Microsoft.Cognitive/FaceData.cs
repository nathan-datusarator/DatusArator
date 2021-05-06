using System;

namespace DatusArator.Microsoft.Cognitive {
  public class FaceData {
    public Guid? FaceId { get; internal set; }

    public double? Age { get; internal set; }
    public double Beard { get; internal set; }
    public double Moustache { get; internal set; }
    public double Sideburns { get; internal set; }
    public string Gender { get; internal set; }
    public double? Smile { get; internal set; }
    public string Glasses { get; internal set; }
    public string HairColor { get; internal set; }

    public string Noise { get; internal set; }
    public bool? OccludedForehead { get; internal set; }
    public bool? OccludedMouth { get; internal set; }
    public bool? OccludedEye { get; internal set; }
    public int FaceLeft { get; internal set; }
    public int FaceTop { get; internal set; }
    public int FaceWidth { get; internal set; }
    public int FaceHeight { get; internal set; }
  }
}