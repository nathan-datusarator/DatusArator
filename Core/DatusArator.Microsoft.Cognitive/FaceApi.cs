using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using DatusArator.Core.Util;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace DatusArator.Microsoft.Cognitive {
  public class FaceApi : IDisposable {
    private const string fSubscriptionKey = "ecb75d2b40814075a564274a1063367b";
    private const string fEndpoint = "https://westus2.api.cognitive.microsoft.com";

    private readonly IFaceClient fFaceClient = new FaceClient(
          new ApiKeyServiceClientCredentials(fSubscriptionKey),
          new System.Net.Http.DelegatingHandler[] { });

    private static FaceApi fInstance;

    public static FaceApi Instance {
      get {
        if (fInstance == null)
          fInstance = new FaceApi();

        return fInstance;
      }
    }

    private FaceApi() {
      fFaceClient.Endpoint = fEndpoint;
    }

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing)
        fFaceClient.Dispose();
    }

    public async Task<List<FaceData>> UploadAndDetectFaces(Stream imageStream) {
      IList<FaceAttributeType> faceAttributes = new FaceAttributeType[] {
                    FaceAttributeType.Gender, FaceAttributeType.Age,
                    FaceAttributeType.Smile,
                    FaceAttributeType.Glasses, FaceAttributeType.Hair,
                    FaceAttributeType.FacialHair, FaceAttributeType.Noise,
                    FaceAttributeType.Occlusion
      };

      IList<DetectedFace> faceList = await fFaceClient.Face.DetectWithStreamAsync(imageStream, true, false, faceAttributes);

      return CreateFaceData(faceList);
    }

    // Decoupling the Cognitive Library from the code so don't need to include package in using packages
    private List<FaceData> CreateFaceData(IList<DetectedFace> faceList) {
      var result = new List<FaceData>();
      
      foreach (var face in faceList) {
        var item = new FaceData {
          FaceId = face.FaceId,
          Age = face.FaceAttributes.Age,
          Beard = face.FaceAttributes.FacialHair.Beard,
          Moustache = face.FaceAttributes.FacialHair.Moustache,
          Sideburns = face.FaceAttributes.FacialHair.Sideburns,
          Gender = face.FaceAttributes.Gender?.ToString(),
          Smile = face.FaceAttributes.Smile,
          Glasses = face.FaceAttributes.Glasses?.ToString(),

          HairColor = null
        };
        IList<HairColor> hairColors = face.FaceAttributes.Hair.HairColor;
        foreach (HairColor hairColor in hairColors) {
          if (hairColor.Confidence >= 0.6f) {
            item.HairColor = StringUtils.Concat(item.HairColor, hairColor.Color.ToString(), " ");
          }
        }

        item.Noise = face.FaceAttributes.Noise?.NoiseLevel.ToString();
        item.OccludedForehead = face.FaceAttributes.Occlusion?.ForeheadOccluded;
        item.OccludedMouth = face.FaceAttributes.Occlusion?.MouthOccluded;
        item.OccludedEye = face.FaceAttributes.Occlusion?.EyeOccluded;

        item.FaceLeft = face.FaceRectangle.Left;
        item.FaceTop = face.FaceRectangle.Top;
        item.FaceWidth = face.FaceRectangle.Width;
        item.FaceHeight = face.FaceRectangle.Height;

        result.Add(item);
      }

      return result;
    }
  }
}
